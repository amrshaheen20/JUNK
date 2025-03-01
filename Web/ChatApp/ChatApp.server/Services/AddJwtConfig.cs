using ChatApi.server.Context;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ChatApi.server.Services
{
    public static class Jwt
    {
        public static void AddJwtConfiguration(this IServiceCollection services, WebApplicationBuilder builder)
        {
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;

            }).AddJwtBearer(options =>
            {
                options.SaveToken = true;
                options.RequireHttpsMetadata = false;
                options.TokenValidationParameters = new TokenValidationParameters()
                {
                    ValidateIssuer = false,
                    ValidIssuer = builder.Configuration["JWT:ValidIssuer"],
                    ValidateAudience = false,
                    ValidAudience = builder.Configuration["JWT:ValidAudience"],
                    IssuerSigningKey =
                    new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(builder.Configuration["JWT:SecretKey"]))
                };

                options.Events = new JwtBearerEvents
                {
                    OnTokenValidated = async context =>
                    {
                        var cancellationToken = context.HttpContext.RequestAborted;
                        var db = context.HttpContext.RequestServices.GetRequiredService<DataBaseContext>();
                        var claimsPrincipal = context.Principal;
                        var userId = claimsPrincipal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                        var TokenId = claimsPrincipal?.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;

                        if (userId == null)
                        {
                            context.Fail("Invalid token. name identifier not found");
                            return;
                        }


                        var user = await db.Users.FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
                        var token = await db.ProfileSessions.AsNoTracking().FirstOrDefaultAsync(x => x.ProfileId == userId && x.TokenId == TokenId && x.ExpirationTime >= DateTime.UtcNow, cancellationToken);


                        if (user == null || token == null)
                        {
                            context.Fail("Invalid or expired token. Please log in again.");
                        }
                        else
                        {
                            user.LastActiveTime = DateTime.UtcNow;
                            db.Update(user);
                            await db.SaveChangesAsync(cancellationToken);
                        }
                    }
                };
            });
        }
    }
}
