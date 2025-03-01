using ChatApi.server.Context;
using ChatApi.server.Extensions;
using ChatApi.server.Models.DbSet;
using ChatApi.server.Models.Dtos.Request;
using ChatApi.server.Models.Dtos.Response;
using ChatApi.server.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Security.Claims;


namespace ChatApi.server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ProfileController : MainControllere
    {
        private readonly UserManager<Profile> userManager;
        private readonly DataBaseContext db;
        private readonly EmailSender emailSender;
        private readonly AttachmentManger attachmentManger;

        public ProfileController(UserManager<Profile> userManager, DataBaseContext db, EmailSender emailSender, AttachmentManger attachmentManger)
        {
            this.userManager = userManager;
            this.db = db;
            this.emailSender = emailSender;
            this.attachmentManger = attachmentManger;
        }

        [HttpGet("{profile_id?}")]
        public async Task<ActionResult<ProfileWithMoreDataResponseDto>> GetProfile(
            CancellationToken cancellationToken,
            string? profile_id = Constants.CURRENT_USER_TAG)
        {
            var profile = await userManager.GetUserAsync(HttpContext.User);
            if (profile == null)
            {
                return ERROR(Unauthorized, "Authentication failed");
            }

            var TargetProfile = profile;
            if (profile_id != Constants.CURRENT_USER_TAG)
            {
                TargetProfile = await db.Profiles.AsNoTracking().FirstOrDefaultAsync(x => x.Id == profile_id, cancellationToken);
                if (TargetProfile == null)
                {
                    return ERROR(NotFound, "User not found");
                }
            }


            var MutualServers = await db.Servers
                .AsNoTracking()
                .Where(s => s.Members.Any(m => m.ProfileId == profile.Id) &&
                    s.Members.Any(m => m.ProfileId == profile_id) &&
                    profile.Id != profile_id
                    )
                .Select(s => new MutualServerDto(s))
                .ToListAsync(cancellationToken);

            var FriendshipStatus = (await db.Friendships.FirstOrDefaultAsync(f => (f.UserId == profile.Id && f.FriendId == TargetProfile.Id) ||
                    (f.UserId == TargetProfile.Id && f.FriendId == profile.Id), cancellationToken))?.Status;

            return Ok(new ProfileWithMoreDataResponseDto(TargetProfile, MutualServers, FriendshipStatus));
        }

        [HttpPatch]
        public async Task<ActionResult<ProfileResponseDto>> UpdateProfile(
            [FromForm] UpdateProfileRequestDto updateProfileDto,
            CancellationToken cancellationToken
            )
        {
            var profile = await userManager.GetUserAsync(HttpContext.User);
            if (profile == null)
            {
                return ERROR(Unauthorized, "Authentication failed");
            }

            if (!string.IsNullOrEmpty(updateProfileDto.Email) && updateProfileDto.Email != profile.Email && await userManager.FindByEmailAsync(updateProfileDto.Email) != null)
            {
                return ERROR(BadRequest, "Email is already in use");
            }

            if (!string.IsNullOrEmpty(updateProfileDto.UserName) && updateProfileDto.UserName != profile.UserName && await userManager.FindByNameAsync(updateProfileDto.UserName) != null)
            {
                return ERROR(BadRequest, "Username is already in use");
            }

            var attachment = await attachmentManger.UploadFileAsync(updateProfileDto.Image, profile.Id, cancellationToken);
            bool EmailChanged = !string.IsNullOrEmpty(updateProfileDto.Email) && updateProfileDto.Email != profile.Email;
            profile.UserName = updateProfileDto.UserName ?? profile.UserName;
            profile.Email = updateProfileDto.Email ?? profile.Email;
            profile.DisplayName = updateProfileDto.DisplayName ?? profile.DisplayName;
            profile.ImageId = attachment?.Id ?? profile.ImageId;
            profile.Bio = updateProfileDto.Bio ?? profile.Bio;

            profile.UpdatedAt = DateTime.UtcNow;
            var result = await userManager.UpdateAsync(profile);
            if (!result.Succeeded)
            {
                return ERROR(BadRequest, result.Errors.First().Description);
            }

            if (EmailChanged)
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




            return Ok(new ProfileResponseDto(profile));
        }

        //leave this for now
        //[Delete Profile]



        [HttpGet("servers")]
        public ActionResult<IEnumerable<ServerResponseDto>> GetServers(
        [FromQuery] PaginateRequest pagination
        )
        {
            var ProfileId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (ProfileId == null)
            {
                return ERROR(Unauthorized, "Authentication failed");
            }

            var servers = db.Servers
                .AsNoTracking()
                .Include(m => m.Members)
                .Include(c => c.Channels)
                .Where(s => s.Members.Any(m => m.ProfileId == ProfileId))
                .PaginateByCursorWithCounter(pagination.GetCursor(), pagination.GetDirection(), pagination.Limit, s => s.Members.First(x => x.ProfileId == ProfileId).CreatedAt);

            var DataDtos = servers.Data?.Select(x => new ServerResponseDto(x, ProfileId));

            if (pagination.ShowTotalNumber)
            {
                return Ok(new PageResponseDto<ServerResponseDto>(servers.TotalCount, DataDtos));
            }

            return Ok(DataDtos);
        }



        [HttpGet("Conversations")]
        public ActionResult<IEnumerable<ConversationResponseDto>> GetConversations(
           [FromQuery] PaginateRequest pagination)
        {

            var ProfileId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (ProfileId == null)
            {
                return ERROR(Unauthorized, "Authentication failed");
            }

            var conversations = db.Conversations
                .AsNoTracking()
                .Include(x => x.ProfileOne)
                .Include(x => x.ProfileTwo)
                .Where(x => (x.ProfileOneId == ProfileId || x.ProfileTwoId == ProfileId))
                .PaginateByCursorWithCounter(pagination.GetCursor(), pagination.GetDirection(), pagination.Limit, s => s.CreatedAt);

            var DataDtos = conversations.Data?.Select(x => new ConversationResponseDto(x, ProfileId == x.ProfileOneId ? x.ProfileTwo! : x.ProfileOne!)).ToList();

            if (pagination.ShowTotalNumber)
            {

                return Ok(new PageResponseDto<ConversationResponseDto>(conversations.TotalCount, DataDtos));
            }

            return Ok(DataDtos);

        }

        [HttpGet("{profile_id?}/online")]
        public async Task<ActionResult<bool>> IsOnline(
            CancellationToken cancellationToken,
            string? profile_id = Constants.CURRENT_USER_TAG)
        {
            var profile = await userManager.GetUserAsync(HttpContext.User);
            if (profile == null)
            {
                return ERROR(Unauthorized, "Authentication failed");
            }

            var TargetProfile = profile;
            if (profile_id != Constants.CURRENT_USER_TAG)
            {
                TargetProfile = await db.Profiles.AsNoTracking().FirstOrDefaultAsync(x => x.Id == profile_id, cancellationToken);
                if (TargetProfile == null)
                {
                    return ERROR(NotFound, "User not found");
                }
            }

            var isOnline = TargetProfile.LastActiveTime.AddMinutes(Constants.LAST_ACTIVE_NUMBER) > DateTime.UtcNow;

            return Ok(new OnlineStatusResponse
            {
                UserId = TargetProfile.Id,
                IsOnline = isOnline,
                LastActive = TargetProfile.LastActiveTime
            });

        }



        //Friendships
        [HttpPost("friends/{friend_id}")]
        public async Task<IActionResult> SendFriendRequest(string friend_id, CancellationToken cancellationToken)
        {
            var ProfileId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (ProfileId == null)
            {
                return ERROR(Unauthorized, "Authentication failed");
            }


            if (ProfileId == friend_id)
            {
                return ERROR(BadRequest, "You cannot send a friend request to yourself.");
            }

            var friend = await db.Users.FirstOrDefaultAsync(x => x.Id == friend_id, cancellationToken);
            if (friend == null)
            {
                return ERROR(NotFound, "Friend not found.");
            }

            var existingFriendship = await db.Friendships.FirstOrDefaultAsync(f =>
                (f.UserId == ProfileId && f.FriendId == friend_id) ||
                (f.UserId == friend_id && f.FriendId == ProfileId),
                cancellationToken
            );

            if (existingFriendship != null)
            {
                if (existingFriendship.Status == FriendshipStatus.Pending)
                {
                    return ERROR(BadRequest, "Friend request is already pending.");
                }
                if (existingFriendship.Status == FriendshipStatus.Accepted)
                {
                    return ERROR(BadRequest, "You are already friends.");
                }
            }

            var friendship = new Friendship
            {
                UserId = ProfileId,
                FriendId = friend_id,
                Status = FriendshipStatus.Pending,
            };

            db.Friendships.Add(friendship);
            await db.SaveChangesAsync(cancellationToken);

            return Ok(new { Message = "Friend request sent successfully." });
        }


        [HttpPatch("friends/{friend_id}/accept")]
        public async Task<IActionResult> AcceptFriendRequest(string friend_id, CancellationToken cancellationToken)
        {
            var ProfileId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (ProfileId == null)
            {
                return ERROR(Unauthorized, "Authentication failed");
            }


            var friendship = await db.Friendships
                .FirstOrDefaultAsync(f => f.UserId == friend_id && f.FriendId == ProfileId, cancellationToken);
            if (friendship == null)
            {
                return ERROR(NotFound, "Friend request not found");
            }
            friendship.Status = FriendshipStatus.Accepted;
            friendship.UpdatedAt = DateTime.UtcNow;
            await db.SaveChangesAsync(cancellationToken);

            return Ok(new { Message = "Request sent" });
        }


        [HttpDelete("friends/{friend_id}/delete")]
        public async Task<IActionResult> RemoveFriend(string friend_id, CancellationToken cancellationToken)
        {
            var ProfileId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (ProfileId == null)
            {
                return ERROR(Unauthorized, "Authentication failed");
            }

            var friendship = await db.Friendships
                .FirstOrDefaultAsync(f => (f.UserId == ProfileId && f.FriendId == friend_id) ||
                    (f.UserId == friend_id && f.FriendId == ProfileId), cancellationToken);
            if (friendship == null)
            {
                return ERROR(NotFound, "Friend request not found");
            }
            db.Friendships.Remove(friendship);
            await db.SaveChangesAsync(cancellationToken);
            return Ok(new { Message = "Friend removed" });
        }

        [HttpGet("friends")]

        public ActionResult<PageResponseDto<ProfileResponseDto>> GetFriends(
            [FromQuery] PageRequest request)
        {
            var ProfileId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (ProfileId == null)
            {
                return ERROR(Unauthorized, "Authentication failed");
            }


            var friends = db.Friendships
                .AsNoTracking()
                .Include(f => f.User)
                .Include(f => f.Friend)
                .OrderByDescending(f => f.CreatedAt)
                .Where(f => (f.UserId == ProfileId || f.FriendId == ProfileId) && f.Status == FriendshipStatus.Accepted)
                .Select(f => f.UserId == ProfileId ? f.Friend : f.User)
                .PaginateByPageWithCounter(request.PageNumber, request.Limit);

            var DataDtos = friends.Data?.Select(x => new ProfileResponseDto(x!));

            return Ok(new PageResponseDto<ProfileResponseDto>(friends.TotalCount, DataDtos));
        }

        [HttpGet("friends/requests")]
        public IActionResult GetFriendRequests([FromQuery] PageRequest request)
        {
            var ProfileId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (ProfileId == null)
            {
                return ERROR(Unauthorized, "Authentication failed");
            }


            var friendRequests = db.Friendships
                .AsNoTracking()
                .Include(f => f.User)
                 .OrderByDescending(f => f.CreatedAt)
                .Where(f => f.FriendId == ProfileId && f.Status == FriendshipStatus.Pending)
                .Select(x => x.User)
                .PaginateByPageWithCounter(request.PageNumber, request.Limit);

            var DataDtos = friendRequests.Data?.Select(x => new ProfileResponseDto(x!));

            return Ok(new PageResponseDto<ProfileResponseDto>(friendRequests.TotalCount, DataDtos));
        }

        [HttpGet("friends/submitted")]
        public IActionResult GetSentFriendRequests(
            [FromQuery] PageRequest request)
        {

            var ProfileId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (ProfileId == null)
            {
                return ERROR(Unauthorized, "Authentication failed");
            }

            var sentFriendRequests = db.Friendships
                .AsNoTracking()
                .Include(f => f.Friend)
                .OrderByDescending(f => f.CreatedAt)
                .Where(f => f.UserId == ProfileId && f.Status == FriendshipStatus.Pending)
                .Select(f => f.Friend)
                .PaginateByPageWithCounter(request.PageNumber, request.Limit);

            var DataDtos = sentFriendRequests.Data?.Select(x => new ProfileResponseDto(x!));

            return Ok(new PageResponseDto<ProfileResponseDto>(sentFriendRequests.TotalCount, DataDtos));
        }


    }
}
