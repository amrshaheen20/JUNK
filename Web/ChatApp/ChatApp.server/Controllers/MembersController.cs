using ChatApi.server.Context;
using ChatApi.server.Extensions;
using ChatApi.server.Hubs;
using ChatApi.server.Models.DbSet;
using ChatApi.server.Models.Dtos.Request;
using ChatApi.server.Models.Dtos.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;


namespace ChatApi.server.Controllers
{

    [Route("/api/servers/{server_id}/[Controller]")]
    [ApiController]
    [Authorize]
    // [ApiExplorerSettings(GroupName = "Server")]
    public class MembersController : MainControllere
    {
        private readonly UserManager<Profile> userManager;
        private readonly DataBaseContext db;
        private readonly IHubContext<ChatHub> hubContext;

        public MembersController(UserManager<Profile> userManager, DataBaseContext db, IHubContext<ChatHub> hubContext)
        {
            this.userManager = userManager;
            this.db = db;
            this.hubContext = hubContext;
        }


        [HttpGet("{member_id?}")]
        public async Task<ActionResult<MemberResponseDto>> GetMember(
            [FromRoute] string server_id,
            CancellationToken cancellationToken,
            string? member_id = Constants.CURRENT_USER_TAG
            )
        {
            var ProfileId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (ProfileId == null)
            {
                return ERROR(Unauthorized, "Authentication failed");
            }

            var IsServerExists = await db.Servers.AnyAsync(s => s.Id == server_id, cancellationToken);
            if (!IsServerExists)
            {
                return ERROR(NotFound, "Server not found");
            }

            var member = await db.Members.AsNoTracking().Include(x => x.Profile).FirstOrDefaultAsync(m => m.ProfileId == ProfileId && m.ServerId == server_id, cancellationToken);
            if (member == null)
            {
                return ERROR(Forbid, "You are not a member of this server");
            }

            if (member_id != Constants.CURRENT_USER_TAG)
            {
                member = await db.Members.AsNoTracking().Include(x => x.Profile).FirstOrDefaultAsync(m => m.ProfileId == member_id && m.ServerId == server_id, cancellationToken);
                if (member == null)
                {
                    return ERROR(NotFound, "Member not found");
                }
            }

            return Ok(new MemberResponseDto(member));
        }

        [HttpGet()]
        public async Task<ActionResult<IEnumerable<MemberResponseDto>>> GetMembers(
            [FromRoute] string server_id,
            [FromQuery] PaginateRequest pagination,
            CancellationToken cancellationToken
            )
        {
            var ProfileId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (ProfileId == null)
            {
                return ERROR(Unauthorized, "Authentication failed");
            }

            var isMember = await db.Members.AnyAsync(m => m.ProfileId == ProfileId && m.ServerId == server_id, cancellationToken);
            if (!isMember)
                return ERROR(Forbid, "You are not a member of this server");

            var members = db.Members
                .AsNoTracking()
                .Include(m => m.Profile)
                .Where(m => m.ServerId == server_id)
                .PaginateByCursorWithCounter(pagination.GetCursor(), pagination.GetDirection(), pagination.Limit, s => s.Profile.DisplayName);


            var DataDtos = members.Data?.Select(c => new MemberResponseDto(c));

            if (pagination.ShowTotalNumber)
            {
                if (pagination.ShowTotalNumber)
                {
                    return Ok(new PageResponseDto<MemberResponseDto>(members.TotalCount, DataDtos));
                }
            }

            return Ok(DataDtos);
        }


        [HttpPatch("{member_id}")]
        public async Task<ActionResult<MemberResponseDto>> EditMemberRole(
            [FromRoute] string server_id,
            string member_id,
            [FromBody] eMemberRole memberRole,
            CancellationToken cancellationToken
            )
        {
            var ProfileId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (ProfileId == null)
            {
                return ERROR(Unauthorized, "Authentication failed");
            }

            var IsServerExists = await db.Servers.AnyAsync(s => s.Id == server_id, cancellationToken);
            if (!IsServerExists)
            {
                return ERROR(NotFound, "Server not found");
            }

            var ServerMember = await db.Members.FirstOrDefaultAsync(m => m.ProfileId == ProfileId, cancellationToken);
            if (ServerMember == null)
            {
                return ERROR(Forbid, "You are not member of this server");
            }

            var Roles = ServerMember.Role is eMemberRole.ADMIN or eMemberRole.MODERATOR;

            if (Roles)
            {
                return ERROR(Forbid, "You do not have permission to edit this member roles");
            }

            var member = db.Members.Include(x => x.Profile).FirstOrDefault(m => m.ProfileId == member_id && m.ServerId == server_id);
            if (member == null)
            {
                return ERROR(Forbid, "Member not found");
            }


            member.Role = memberRole;
            await db.SaveChangesAsync(cancellationToken);


            var newMember = new MemberResponseDto(member);
            var members = await db.Members.AsNoTracking().Where(x => x.ServerId == server_id && x.Profile.LastActiveTime.AddMinutes(Constants.LAST_ACTIVE_NUMBER) > DateTime.UtcNow).Select(x => x.ProfileId).ToListAsync(cancellationToken);
            await hubContext.Clients.Users(members).SendAsync($"{server_id}:member:update", newMember, cancellationToken);

            return Ok(newMember);
        }


        [HttpDelete("{member_id}")]
        public async Task<ActionResult<MemberResponseDto>> LeaveServer(
            [FromRoute] string server_id,
            string member_id,
            CancellationToken cancellationToken)
        {
            var profile = await userManager.GetUserAsync(HttpContext.User);
            if (profile == null)
            {
                return ERROR(Unauthorized, "Authentication failed");
            }

            var IsServerExists = await db.Servers.AnyAsync(s => s.Id == server_id, cancellationToken);
            if (!IsServerExists)
            {
                return ERROR(NotFound, "Server not found");
            }

            var ServerMember = await db.Members.FirstOrDefaultAsync(m => m.ProfileId == profile.Id, cancellationToken);
            if (ServerMember == null)
            {
                return ERROR(Forbid, "You are not member of this server");
            }

            var Roles = ServerMember.Role is eMemberRole.ADMIN or eMemberRole.MODERATOR;

            if (!Roles)
            {
                return ERROR(Forbid, "You do not have permission to delete this member");
            }

            var member = await db.Members.Include(x => x.Profile).FirstOrDefaultAsync(m => m.ProfileId == member_id && m.ServerId == server_id, cancellationToken);
            if (member == null)
            {
                return ERROR(Forbid, "Member not found");
            }

            db.Members.Remove(member);
            await db.SaveChangesAsync(cancellationToken);


            var deletedMember = new MemberResponseDto(member);
            var members = await db.Members.AsNoTracking().Where(x => x.ServerId == server_id && x.Profile.LastActiveTime.AddMinutes(Constants.LAST_ACTIVE_NUMBER) > DateTime.UtcNow).Select(x => x.ProfileId).ToListAsync(cancellationToken);

            await Task.WhenAll(
             hubContext.Clients.Users(members).SendAsync($"{server_id}:member:delete", new
             {
                 deletedMember.Id
             }, cancellationToken),

             hubContext.Clients.Users(member.ProfileId).SendAsync($"server:delete", new
             {
                 id = member.ServerId
             }, cancellationToken));

            return Ok(deletedMember);
        }

    }
}
