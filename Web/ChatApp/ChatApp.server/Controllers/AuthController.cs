using ChatApi.server.Context;
using ChatApi.server.Models.DbSet;
using ChatApi.server.Models.Dtos.Request;
using ChatApi.server.Models.Dtos.Response;
using ChatApi.server.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Web;

namespace ChatApi.server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous]
    public class AuthController : MainControllere
    {
        private readonly UserManager<Profile> userManager;
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly IConfiguration config;
        private readonly DataBaseContext db;
        private readonly EmailSender emailSender;
        private readonly AttachmentManger attachmentManger;

        public AuthController(UserManager<Profile> userManager,
            RoleManager<IdentityRole> roleManager,
            IConfiguration config,
            DataBaseContext db,
            EmailSender emailSender,
             AttachmentManger attachmentManger
            )
        {
            this.userManager = userManager;
            this.roleManager = roleManager;
            this.config = config;
            this.db = db;
            this.emailSender = emailSender;
            this.attachmentManger = attachmentManger;
        }

        private async Task<IActionResult> SendConfirmationEmail(Profile profile)
        {
            var token = await userManager.GenerateEmailConfirmationTokenAsync(profile);

            try
            {
                await emailSender.SendEmailConfirmationAsync(profile.Email, token);
                return Ok("Please check your email to confirm your account.");
            }
            catch
            {
                return ERROR(InternalServerError, "Error sending confirmation email");
            }
        }

        private async Task<JwtSecurityToken> GenerateJwtTokenAsync(Profile user)
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.Name, user.UserName),
                new(ClaimTypes.NameIdentifier, user.Id),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            };


            var roles = await userManager.GetRolesAsync(user);
            claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

            // RsaSecurityKey or you can use rsa, rsa use public and private key
            var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["JWT:SecretKey"]));
            var signingCredentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

            var jwtToken = new JwtSecurityToken(
                issuer: config["JWT:ValidIssuer"],
                audience: config["JWT:ValidAudience"],
                expires: DateTime.Now.AddMonths(1),
                claims: claims,
                signingCredentials: signingCredentials
            );

            return jwtToken;
        }

        private async Task LogOutAllUsers(Profile profile, CancellationToken cancellationToken)
        {
            var sessions = await db.ProfileSessions
                .Where(x => x.ProfileId == profile.Id)
                .ToListAsync(cancellationToken);

            if (sessions.Any())
            {
                db.ProfileSessions.RemoveRange(sessions);
                await db.SaveChangesAsync(cancellationToken);
            }
        }




        [HttpGet("CheckAvailability")]
        public async Task<ActionResult<bool>> CheckAvailability([FromQuery] CheckAvailabilityRequestDto request)
        {
            if (!string.IsNullOrEmpty(request.UserName) && !string.IsNullOrEmpty(request.Email))
            {
                return ERROR(BadRequest, "Send email or username only");
            }

            if (!string.IsNullOrEmpty(request.Email))
            {

                var user = await userManager.FindByEmailAsync(request.Email);
                if (user == null)
                {
                    return Ok(true);
                }
            }

            if (!string.IsNullOrEmpty(request.UserName))
            {
                var user = await userManager.FindByNameAsync(request.UserName);
                if (user == null)
                {
                    return Ok(true);
                }
            }

            return Ok(false);
        }


        [HttpPost("Register")]
        public async Task<IActionResult> Register([FromForm] RegisterRequestDto userDto, CancellationToken cancellationToken)
        {
            if (await userManager.FindByEmailAsync(userDto.Email) != null)
                return ERROR(BadRequest, "Email is already in use");

            if (await userManager.FindByNameAsync(userDto.UserName) != null)
                return ERROR(BadRequest, "Username is already in use");

            using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                var isOwner = !await db.Profiles.AnyAsync(cancellationToken);

                var profile = new Profile
                {
                    DisplayName = userDto.DisplayName,
                    UserName = userDto.UserName,
                    Email = userDto.Email
                };

                var result = await userManager.CreateAsync(profile, userDto.Password);
                if (!result.Succeeded)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return ERROR(BadRequest, result.Errors.First().Description);
                }

                if (userDto.Image != null)
                {
                    var attachment = await attachmentManger.UploadFileAsync(userDto.Image, profile.Id, cancellationToken);
                    if (attachment != null)
                    {
                        profile.ImageId = attachment.Id;
                        await db.SaveChangesAsync(cancellationToken);
                    }
                }

                var roleName = isOwner ? ProfileRoles.Admin : ProfileRoles.Member;

                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    var roleResult = await roleManager.CreateAsync(new IdentityRole(roleName));
                    if (!roleResult.Succeeded)
                    {
                        await transaction.RollbackAsync(cancellationToken);
                        return ERROR(BadRequest, "Failed to create role.");
                    }
                }

                await userManager.AddToRoleAsync(profile, roleName);

                await transaction.CommitAsync(cancellationToken);

                return await SendConfirmationEmail(profile);
            }
            catch 
            {
                await transaction.RollbackAsync(cancellationToken);
                return ERROR(InternalServerError, $"Registration failed");
            }
        }


        [Authorize]
        [HttpPost("IsEmailConfirmed")]
        public async Task<ActionResult<bool>> IsEmailConfirmed()
        {
            var profile = await userManager.GetUserAsync(HttpContext.User);

            if (profile == null)
            {
                return ERROR(Unauthorized, "Authentication failed");
            }

            return Ok(profile.EmailConfirmed);
        }

        [HttpPost("ResendEmailConfirmation")]
        public async Task<IActionResult> ResendEmailConfirmation(ResetRequestDto request)
        {
            var user = await userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                return ERROR(Unauthorized, "User not found");
            }

            if (user.EmailConfirmed)
            {
                return Ok("Email already confirmed");
            }

            return await SendConfirmationEmail(user);
        }

        [HttpPost("ConfirmEmail")]
        public async Task<IActionResult> ConfirmEmail(ConfirmEmailRequestDto request)
        {
            var user = await userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                return ERROR(Unauthorized, "User not found");
            }

            if (user.EmailConfirmed)
            {
                return Ok("Email already confirmed");
            }

            var result = await userManager.ConfirmEmailAsync(user, request.Token);
            if (result.Succeeded)
            {
                return Ok("Email confirmed successfully");
            }

            return ERROR(BadRequest, result.Errors.First().Description);
        }


        [HttpPost("Login")]
        public async Task<ActionResult<LoginResponseDto>> Login(LoginRequestDto userDto, CancellationToken cancellationToken)
        {

            Profile user = await userManager.FindByEmailAsync(userDto.Email) ?? await userManager.FindByNameAsync(userDto.Email);
            if (user == null || !await userManager.CheckPasswordAsync(user, userDto.Password))
            {
                return ERROR(Unauthorized, "Invalid credentials");
            }


            var ExpirateredTokens = await db.ProfileSessions.Where(x => x.ProfileId == user.Id && x.ExpirationTime < DateTime.UtcNow).ToListAsync(cancellationToken);
            if (ExpirateredTokens.Any())
            {
                db.ProfileSessions.RemoveRange(ExpirateredTokens);
                await db.SaveChangesAsync(cancellationToken);
            }


            var token = await GenerateJwtTokenAsync(user);
            var tokenId = token.Claims.First(x => x.Type == JwtRegisteredClaimNames.Jti).Value;
            var NewSession = new ProfileSession
            {
                ProfileId = user.Id,
                ExpirationTime = token.ValidTo,
                TokenId = tokenId,
                UserAgent = HttpContext.Request.Headers.UserAgent,
            };

            db.ProfileSessions.Add(NewSession);
            await db.SaveChangesAsync(cancellationToken);

            return Ok(new LoginResponseDto
            {
                Id = user.Id,
                Token = new JwtSecurityTokenHandler().WriteToken(token),

                //Should i remove this?
                Email = user.Email,
                UserName = user.UserName,
                /////////////////////

                Expiration = NewSession.ExpirationTime
            });
        }

        [HttpPost("ForgotPassword")]
        public async Task<IActionResult> ForgotPassword(ResetRequestDto request)
        {
            var user = await userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                return ERROR(Unauthorized, "User not found");
            }

            var token = await userManager.GeneratePasswordResetTokenAsync(user);


            var uriBuilder = new UriBuilder(Constants.FRONTEND_URL_PASSWORD_RESET);
            var query = HttpUtility.ParseQueryString(uriBuilder.Query);
            query[nameof(PasswordResetRequestDto.UserId)] = user.Id;
            query[nameof(PasswordResetRequestDto.Token)] = token;
            uriBuilder.Query = query.ToString();
            var resetLink = uriBuilder.ToString();

            try
            {
                await emailSender.SendPasswordResetAsync(request.Email, resetLink);
                return Ok("Please check your email to reset your password.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error sending password reset email: {ex.Message}");
            }
        }

        [HttpPost("ResetPassword")]
        public async Task<IActionResult> ResetPassword(PasswordResetRequestDto request, CancellationToken cancellationToken)
        {
            var user = await userManager.FindByIdAsync(request.UserId);
            if (user == null)
            {
                return ERROR(Unauthorized, "User not found");
            }

            var result = await userManager.ResetPasswordAsync(user, request.Token, request.Password);
            if (!result.Succeeded)
            {
                return ERROR(BadRequest, result.Errors.First().Description);
            }

            await LogOutAllUsers(user, cancellationToken);

            db.Profiles.Update(user);
            await db.SaveChangesAsync(cancellationToken);
            return Ok("Password reset successfully");
        }


        [Authorize]
        [HttpPatch("UpdatePassword")]
        public async Task<IActionResult> UpdatePassword(
            UpdatePasswordRequestDto updatePasswordDto,
            CancellationToken cancellationToken)
        {
            var profile = await userManager.GetUserAsync(HttpContext.User);
            if (profile == null)
            {
                return ERROR(Unauthorized, "User not found");
            }

            var isPasswordValid = await userManager.CheckPasswordAsync(profile, updatePasswordDto.OldPassword);
            if (!isPasswordValid)
            {
                return ERROR(BadRequest, "Invalid old password");
            }

            var result = await userManager.ChangePasswordAsync(profile, updatePasswordDto.OldPassword, updatePasswordDto.NewPassword);
            if (!result.Succeeded)
            {
                return ERROR(BadRequest, result.Errors.First().Description);
            }

            await LogOutAllUsers(profile, cancellationToken);

            return Ok("Password updated successfully");
        }


    }
}
