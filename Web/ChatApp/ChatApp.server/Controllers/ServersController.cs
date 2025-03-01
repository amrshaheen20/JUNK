using ChatApi.server.Context;
using ChatApi.server.Extensions;
using ChatApi.server.Hubs;
using ChatApi.server.Models.DbSet;
using ChatApi.server.Models.Dtos.Request;
using ChatApi.server.Models.Dtos.Response;
using ChatApi.server.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;


namespace ChatApi.server.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    //[ApiExplorerSettings(GroupName = "Server")]
    public class ServersController : MainControllere
    {
        private readonly UserManager<Profile> userManager;
        private readonly DataBaseContext db;
        private readonly IHubContext<ChatHub> hubContext;
        private readonly AttachmentManger attachmentManger;

        public ServersController(UserManager<Profile> userManager, DataBaseContext db, IHubContext<ChatHub> hubContext, AttachmentManger attachmentManger)
        {
            this.userManager = userManager;
            this.db = db;
            this.hubContext = hubContext;
            this.attachmentManger = attachmentManger;
        }



        [HttpPost()]
        public async Task<ActionResult<ServerResponseDto>> CreateServer(
            [FromForm] CreateServerRequestDto request,
            CancellationToken cancellationToken)
        {
            var ProfileId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (ProfileId == null)
            {
                return ERROR(Unauthorized, "Authentication failed");
            }

            var attachment = await attachmentManger.UploadFileAsync(request.Image, ProfileId, cancellationToken);

            var server = new Server
            {
                ProfileId = ProfileId,
                Name = request.name,
                ImageId = attachment?.Id,
                Channels = new List<Channel> { new() { Name = "general", ProfileId = ProfileId } },
                Members = new List<Member> { new() { ProfileId = ProfileId, Role = eMemberRole.ADMIN } }
            };

            db.Servers.Add(server);
            await db.SaveChangesAsync(cancellationToken);

            var newServer = new ServerResponseDto(server, ProfileId);
            await hubContext.Clients.Users(ProfileId).SendAsync($"server:add", newServer, cancellationToken);

            return Created(newServer);
        }

        [HttpGet("{server_id}")]
        public async Task<ActionResult<ServerResponseDto>> GetServer(string server_id,
            CancellationToken cancellationToken)
        {
            var ProfileId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (ProfileId == null)
            {
                return ERROR(Unauthorized, "Authentication failed");
            }


            var server = await db.Servers
                .Include(s => s.Members).ThenInclude(x => x.Profile)
                .Include(s => s.Channels)
                .Include(s => s.Profile)
                .FirstOrDefaultAsync(s => s.Id == server_id, cancellationToken);

            if (server == null)
            {
                return ERROR(NotFound, "Server not found");
            }

            var member = server.Members.FirstOrDefault(m => m.ProfileId == ProfileId);

            if (member == null)
            {
                return ERROR(Forbid, "You are not a member of this server");
            }

            return Ok(new ServerResponseDto(server, ProfileId));
        }

        [HttpGet("search")]
        public ActionResult<PageResponseDto<ServerResponseDto>> GetServers(
[FromQuery] SearchRequest request)
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
                .Search(request.Query, s => s.Members.First(x => x.ProfileId == ProfileId).CreatedAt, x => x.Name)
                .PaginateByPageWithCounter(request.PageNumber, request.Limit);

            var DataDtos = servers.Data?.Select(s => new ServerResponseDto(s, ProfileId));

            return Ok(new PageResponseDto<ServerResponseDto>(servers.TotalCount, DataDtos));
        }

        [HttpPatch("{server_id}")]
        public async Task<ActionResult<ServerResponseDto>> EditServer(
            string server_id,
            [FromForm] EditServerRequestDto? request,
            CancellationToken cancellationToken
            )
        {
            var ProfileId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (ProfileId == null)
            {
                return ERROR(Unauthorized, "Authentication failed");
            }


            var server = await db.Servers
                .Include(s => s.Members).ThenInclude(x => x.Profile)
                .Include(s => s.Channels)
                .Include(s => s.Profile)
                .FirstOrDefaultAsync(s => s.Id == server_id, cancellationToken);

            if (server == null)
            {
                return ERROR(NotFound, "Server not found");
            }

            var member = server.Members.FirstOrDefault(x => x.ProfileId == ProfileId);
            if (member == null)
            {
                return ERROR(Forbid, "You are not a member of this server");
            }


            var CanEditServer = member.Role is eMemberRole.ADMIN or eMemberRole.MODERATOR;


            if (!CanEditServer)
                return ERROR(Forbid, "You do not have permission to edit this server");


            var attachment = await attachmentManger.UploadFileAsync(request?.Image, ProfileId, cancellationToken);


            server.Name = request?.name ?? server.Name;
            server.ImageId = attachment?.Id ?? server.ImageId;
            server.UpdatedAt = DateTime.UtcNow;

            await db.SaveChangesAsync(cancellationToken);

            var newServer = new ServerResponseDto(server, ProfileId);
            var members = await db.Members.AsNoTracking().Where(x => x.ServerId == server_id && x.Profile.LastActiveTime.AddMinutes(Constants.LAST_ACTIVE_NUMBER) > DateTime.UtcNow).Select(x => x.ProfileId).ToListAsync(cancellationToken);
            await hubContext.Clients.Users(members).SendAsync($"server:update", newServer, cancellationToken);

            return Ok(newServer);
        }

        [HttpDelete("{server_id}")]
        public async Task<ActionResult<ServerResponseDto>> DeleteServer(
            string server_id,
            CancellationToken cancellationToken
            )
        {
            var ProfileId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (ProfileId == null)
            {
                return ERROR(Unauthorized, "Authentication failed");
            }

            var roles = await userManager.GetRolesAsync(await userManager.FindByIdAsync(ProfileId));

            var server = await db.Servers
                .Include(s => s.Members).ThenInclude(x => x.Profile)
                .Include(s => s.Channels)
                .Include(s => s.Profile)
                .FirstOrDefaultAsync(s => s.Id == server_id, cancellationToken);

            if (server == null)
            {
                return ERROR(NotFound, "Server not found");
            }

            var member = server.Members.FirstOrDefault(x => x.ProfileId == ProfileId);
            if (member == null)
            {
                return ERROR(Forbid, "You are not a member of this server");
            }

            var CanDeleteServer = ProfileId == server.ProfileId || ProfileRoles.IsSuperAdmin(roles);

            if (!CanDeleteServer)
                return ERROR(Forbid, "You do not have permission to delete this server");


            db.Servers.Remove(server);
            await db.SaveChangesAsync(cancellationToken);


            var deletedServer = new ServerResponseDto(server, ProfileId);
            var members = await db.Members.AsNoTracking().Where(x => x.ServerId == server_id && x.Profile.LastActiveTime.AddMinutes(Constants.LAST_ACTIVE_NUMBER) > DateTime.UtcNow).Select(x => x.ProfileId).ToListAsync(cancellationToken);
            await hubContext.Clients.Users(members).SendAsync($"server:delete", new
            {
                deletedServer.Id
            }, cancellationToken);


            return Ok(deletedServer);
        }







        [HttpPatch("{server_id}/leave")]
        public async Task<ActionResult<MemberResponseDto>> LeaveServer(
            string server_id,
            CancellationToken cancellationToken)
        {
            var ProfileId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (ProfileId == null)
            {
                return ERROR(Unauthorized, "Authentication failed");
            }

            var server = await db.Servers
                .Include(s => s.Members).ThenInclude(x => x.Profile)
                .FirstOrDefaultAsync(s => s.Id == server_id, cancellationToken);

            if (server == null)
            {
                return ERROR(NotFound, "Server not found");
            }


            var member = server.Members.FirstOrDefault(m => m.ProfileId == ProfileId);
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

             hubContext.Clients.User(ProfileId).SendAsync($"server:delete", new
             {
                 member.ServerId
             }, cancellationToken)
            );

            return Ok(new MemberResponseDto(member));
        }


        [HttpPatch("{invite_code}/join")]
        public async Task<ActionResult<ServerResponseDto>> JoinServer(string invite_code, CancellationToken cancellationToken)
        {
            var ProfileId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (ProfileId == null)
            {
                return ERROR(Unauthorized, "Authentication failed");
            }

            var server = await db.Servers
                .Include(s => s.Members).ThenInclude(x => x.Profile)
                .Include(s => s.Channels)
                .Include(s => s.Profile)
                .FirstOrDefaultAsync(s => s.InviteCode == invite_code, cancellationToken);

            if (server == null)
            {
                return ERROR(NotFound, "Server not found");
            }

            bool exists = await db.Members.AnyAsync(m => m.ProfileId == ProfileId && m.ServerId == server.Id, cancellationToken);
            if (exists)
            {
                return Ok(new ServerResponseDto(server, ProfileId));
            }

            var newMember = new Member
            {
                ProfileId = ProfileId,
                Server = server,
                Role = eMemberRole.MEMBER
            };

            try
            {
                db.Members.Add(newMember);
                await db.SaveChangesAsync(cancellationToken);
            }
            catch
            {
                return ERROR(InternalServerError, "Failed to handle the request!");
            }


            var addedMember = new MemberResponseDto(db.Members.Include(x => x.Profile).First(x => x.Id == newMember.Id));
            var newServer = new ServerResponseDto(server, ProfileId);

            var members = await db.Members.AsNoTracking().Where(x => x.ServerId == server.Id && x.Profile.LastActiveTime.AddMinutes(Constants.LAST_ACTIVE_NUMBER) > DateTime.UtcNow).Select(x => x.ProfileId).ToListAsync(cancellationToken);

            await Task.WhenAll(
                    hubContext.Clients.Users(members).SendAsync($"{server?.Id}:member:add", addedMember, cancellationToken),
                    hubContext.Clients.Users(ProfileId).SendAsync($"server:add", newServer, cancellationToken)
            );

            return Ok(newServer);
        }
    }
}
