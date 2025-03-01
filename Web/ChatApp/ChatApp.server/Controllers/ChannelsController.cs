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
    [Route("api/servers/{server_id}/[controller]")]
    [ApiController]
    [Authorize]
    //  [ApiExplorerSettings(GroupName ="Server")]
    public partial class ChannelsController : MainControllere
    {
        private readonly DataBaseContext db;
        private readonly IHubContext<ChatHub> hubContext;

        public ChannelsController(DataBaseContext db, IHubContext<ChatHub> hubContext)
        {
            this.db = db;
            this.hubContext = hubContext;
        }


        [HttpGet()]
        public async Task<ActionResult<IEnumerable<ChannelResponseDto>>> GetChannels(
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

            var isMember = await db.Members.AsNoTracking().AnyAsync(m => m.ProfileId == ProfileId && m.ServerId == server_id, cancellationToken);
            if (!isMember)
                return ERROR(Forbid, "You are not a member of this server");

            var channels = db.Channels
                .AsNoTracking()
                .Where(c => c.ServerId == server_id)
                .PaginateByCursorWithCounter(pagination.GetCursor(), pagination.GetDirection(), pagination.Limit, s => s.CreatedAt);


            var DataDtos = channels.Data?.Select(c => new ChannelResponseDto(c));

            if (pagination.ShowTotalNumber)
            {
                return Ok(new PageResponseDto<ChannelResponseDto>(channels.TotalCount, DataDtos));
            }

            return Ok(DataDtos);
        }

        [HttpGet("search")]
        public async Task<ActionResult<PageResponseDto<ChannelResponseDto>>> GetChannels(
            [FromRoute] string server_id,
             [FromQuery] SearchRequest request,
             CancellationToken cancellationToken
            )
        {
            var ProfileId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (ProfileId == null)
            {
                return ERROR(Unauthorized, "Authentication failed");
            }

            var isMember = await db.Members.AsNoTracking().AnyAsync(m => m.ProfileId == ProfileId && m.ServerId == server_id, cancellationToken);
            if (!isMember)
                return ERROR(Forbid, "You are not a member of this server");

            var channels = db.Channels
                .AsNoTracking()
                .Where(c => c.ServerId == server_id)
                .Search(request.Query, s => s.CreatedAt, x => x.Name)
                .PaginateByPageWithCounter(request.PageNumber, request.Limit);

            var DataDtos = channels.Data?.Select(c => new ChannelResponseDto(c));

            return Ok(new PageResponseDto<ChannelResponseDto>(channels.TotalCount, DataDtos));
        }

        [HttpGet("{channel_id}")]
        public async Task<ActionResult<ChannelResponseDto>> GetChannel([FromRoute] string server_id, string channel_id, CancellationToken cancellationToken)
        {
            var ProfileId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (ProfileId == null)
            {
                return ERROR(Unauthorized, "Authentication failed");
            }

            var channel = await db.Channels.AsNoTracking().FirstOrDefaultAsync(x => x.Id == channel_id &&
            x.Server!.Id == server_id && x.Server.Members.Any(m => m.ProfileId == ProfileId),
            cancellationToken
            );

            if (channel == null)
                return ERROR(NotFound, "Channel not found");

            return Ok(new ChannelResponseDto(channel));
        }





        [HttpPost()]
        public async Task<ActionResult<ChannelResponseDto>> CreateChannel([FromRoute] string server_id,
            [FromBody] CreateChannelRequestDto request
            , CancellationToken cancellationToken)
        {
            var ProfileId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (ProfileId == null)
            {
                return ERROR(Unauthorized, "Authentication failed");
            }

            var server = await db.Servers.AsNoTracking().FirstOrDefaultAsync(x => x.Id == server_id, cancellationToken);
            if (server == null)
                return ERROR(NotFound, "Server not found");

            var member = await db.Members.AsNoTracking().FirstOrDefaultAsync(x => x.ProfileId == ProfileId && x.ServerId == server_id, cancellationToken);
            if (member == null)
                return ERROR(NotFound, "Member not found");

            bool CanCreateChannel = member.Role == eMemberRole.ADMIN || member.Role == eMemberRole.MODERATOR;

            if (!CanCreateChannel)
                return ERROR(Forbid, "You do not have permission to create a channel");

            var channel = new Channel
            {
                ServerId = server_id,
                Name = request.name,
                ProfileId = ProfileId,
                Type = request.channelType ?? eChannelType.TEXT,
                Category = request.category,
            };

            db.Channels.Add(channel);
            await db.SaveChangesAsync(cancellationToken);

            var newChannel = new ChannelResponseDto(channel);
            var members = await db.Members.AsNoTracking().Where(x => x.ServerId == server_id && x.Profile.LastActiveTime.AddMinutes(Constants.LAST_ACTIVE_NUMBER) > DateTime.UtcNow).Select(x => x.ProfileId).ToListAsync(cancellationToken);
            await hubContext.Clients.Users(members).SendAsync($"{server_id}:channel:add", newChannel, cancellationToken);

            return Created(newChannel);
        }




        [HttpPatch("{channel_id}")]
        public async Task<ActionResult<ChannelResponseDto>> EditChannel(
            [FromRoute] string server_id,
            string channel_id,
            [FromBody] EditChannelRequestDto? request,
            CancellationToken cancellationToken
            )
        {
            var ProfileId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (ProfileId == null)
            {
                return ERROR(Unauthorized, "Authentication failed");
            }

            var server = await db.Servers.AsNoTracking().FirstOrDefaultAsync(x => x.Id == server_id, cancellationToken);
            if (server == null)
                return ERROR(NotFound, "Server not found");

            var member = await db.Members.AsNoTracking().FirstOrDefaultAsync(x => x.ProfileId == ProfileId && x.ServerId == server_id, cancellationToken);
            if (member == null)
                return ERROR(NotFound, "Member not found");


            bool CanEditChannel = member.Role is eMemberRole.ADMIN or eMemberRole.MODERATOR;

            if (!CanEditChannel)
            {
                return ERROR(Forbid, "You do not have permission to edit this channel");
            }

            var channel = await db.Channels.FirstOrDefaultAsync(x => x.Id == channel_id && x.ServerId == server_id, cancellationToken);
            if (channel == null)
                return ERROR(NotFound, "Channel not found");


            channel.Name = request?.Name ?? channel.Name;
            channel.Category = request?.Category ?? channel.Category;
            channel.UpdatedAt = DateTime.UtcNow;

            await db.SaveChangesAsync(cancellationToken);
            var newChannel = new ChannelResponseDto(channel);

            var members = await db.Members.AsNoTracking().Where(x => x.ServerId == server_id && x.Profile.LastActiveTime.AddMinutes(Constants.LAST_ACTIVE_NUMBER) > DateTime.UtcNow).Select(x => x.ProfileId).ToListAsync(cancellationToken);
            ;
            await hubContext.Clients.Users(members).SendAsync($"{server_id}:channel:update", newChannel, cancellationToken);

            return Ok(newChannel);
        }



        [HttpDelete("{channel_id}")]
        public async Task<ActionResult<ChannelResponseDto>> DeleteChannel(
            [FromRoute] string server_id,
            string channel_id
            , CancellationToken cancellationToken)
        {
            var ProfileId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (ProfileId == null)
            {
                return ERROR(Unauthorized, "Authentication failed");
            }

            var server = await db.Servers.AsNoTracking().FirstOrDefaultAsync(x => x.Id == server_id, cancellationToken);
            if (server == null)
                return ERROR(NotFound, "Server not found");

            var member = await db.Members.AsNoTracking().FirstOrDefaultAsync(x => x.ProfileId == ProfileId && x.ServerId == server_id, cancellationToken);
            if (member == null)
                return ERROR(NotFound, "Member not found");

            bool CanEditChannel = member.Role is eMemberRole.ADMIN or eMemberRole.MODERATOR;

            if (!CanEditChannel)
            {
                return ERROR(Forbid, "You do not have permission to delete this channel");
            }

            var channel = await db.Channels.FirstOrDefaultAsync(x => x.Id == channel_id && x.ServerId == server_id, cancellationToken);
            if (channel == null)
                return ERROR(NotFound, "Channel not found");


            db.Channels.Remove(channel);
            await db.SaveChangesAsync(cancellationToken);

            var deletedChannel = new ChannelResponseDto(channel);
            var members = await db.Members.AsNoTracking().Where(x => x.ServerId == server_id && x.Profile.LastActiveTime.AddMinutes(Constants.LAST_ACTIVE_NUMBER) > DateTime.UtcNow).Select(x => x.ProfileId).ToListAsync(cancellationToken);
            await hubContext.Clients.Users(members).SendAsync($"{server_id}:channel:delete", new 
            {
                deletedChannel.Id
            }, cancellationToken);

            return Ok(deletedChannel);
        }

    }
}
