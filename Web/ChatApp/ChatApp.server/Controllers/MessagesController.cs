using ChatApi.server.Context;
using ChatApi.server.Extensions;
using ChatApi.server.Hubs;
using ChatApi.server.Models.DbSet;
using ChatApi.server.Models.Dtos.Request;
using ChatApi.server.Models.Dtos.Response;
using ChatApi.server.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;



namespace ChatApi.server.Controllers
{
    [Route("/api/Channels/{channel_id}/[controller]")]
    [ApiController]
    [Authorize]
    // [ApiExplorerSettings(GroupName = "Server")]
    public class MessagesController : MainControllere
    {


        private readonly DataBaseContext db;
        private readonly IHubContext<ChatHub> _hubContext;
        private readonly AttachmentManger attachmentManger;

        public MessagesController(DataBaseContext db, IHubContext<ChatHub> hubContext, AttachmentManger attachmentManger)
        {
            this.db = db;
            _hubContext = hubContext;
            this.attachmentManger = attachmentManger;
        }

        private async Task<bool> IsMember(string channel_id, string? profileId, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(profileId))
            {
                return false;
            }

            var isMember = await db.Channels
                .AsNoTracking()
                .AnyAsync(c => c.Id == channel_id &&
                     (
                         c.Server!.Members.Any(m => m.ProfileId == profileId) || //Server
                         c.Conversation!.ProfileOneId == profileId || c.Conversation.ProfileTwoId == profileId//Conversation
                     ), cancellationToken);
            return isMember;
        }

        private async Task<IEnumerable<string>> ChannelMembers(string channel_id, CancellationToken cancellationToken)
        {
            var serverMembers = await db.Members
                .AsNoTracking()
                .Where(x => x.Server.Channels.Any(c => c.Id == channel_id) &&
                            x.Profile.LastActiveTime.AddMinutes(Constants.LAST_ACTIVE_NUMBER) > DateTime.UtcNow)
                .Select(x => x.ProfileId)
                .ToListAsync(cancellationToken);

            var conversationMembers = await db.Conversations
                .AsNoTracking()
                .Where(x => x.ChannelId == channel_id)
                .Select(x => new { x.ProfileOneId, x.ProfileTwoId })
                .FirstOrDefaultAsync(cancellationToken);


            return serverMembers
                .Concat(new[] { conversationMembers?.ProfileOneId, conversationMembers?.ProfileTwoId })
                .Where(x => !string.IsNullOrEmpty(x))
                .ToHashSet()!;
        }


        [HttpGet()]
        public async Task<ActionResult<IEnumerable<MessageResponseDto>>> GetMessages(
           [FromRoute] string channel_id,
           [FromQuery] PaginateRequest pagination,
           CancellationToken cancellationToken
            )
        {
            var ProfileId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (ProfileId == null)
            {
                return ERROR(Unauthorized, "Authentication failed");
            }

            var isMember = await IsMember(channel_id, ProfileId, cancellationToken);

            if (!isMember)
                return ERROR(Forbid, "You are not a member of this channel");

            var messages = db.Messages
                .AsNoTracking()
                .Include(m => m.Profile)
                .Include(m => m.Attachments)
                .Where(m => m.ChannelId == channel_id)
                .PaginateByCursorWithCounter(pagination.GetCursor(), pagination.GetDirection(), pagination.Limit, s => s.CreatedAt);

            var DataDtos = messages.Data?.AsQueryable().Select(x =>
            new MessageResponseDto(x,
                x.Channel.Server!.Members.Any(m => m.ProfileId == ProfileId) ||
                x.Channel.Conversation!.ProfileOneId == ProfileId || x.Channel.Conversation.ProfileTwoId == ProfileId));

            if (pagination.ShowTotalNumber)
            {
                return Ok(new PageResponseDto<MessageResponseDto>(messages.TotalCount, DataDtos));
            }

            return Ok(DataDtos);

        }

        [HttpGet("search")]
        public async Task<ActionResult<PageResponseDto<MessageResponseDto>>> GetMessages(
           [FromRoute] string channel_id,
           [FromQuery] SearchRequest request,
           CancellationToken cancellationToken
            )
        {
            var ProfileId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (ProfileId == null)
            {
                return ERROR(Unauthorized, "Authentication failed");
            }

            var isMember = await IsMember(channel_id, ProfileId, cancellationToken);

            if (!isMember)
                return ERROR(Forbid, "You are not a member of this channel");

            var messages = db.Messages
                .AsNoTracking()
                .Include(m => m.Profile)
                .Include(m => m.Attachments)
                .Where(m => m.ChannelId == channel_id)
                .Select(x => new
                {
                    message = x,
                    IsMember = x.Channel.Server!.Members.Any(m => m.ProfileId == ProfileId) || //Server
                         x.Channel.Conversation!.ProfileOneId == ProfileId || x.Channel.Conversation.ProfileTwoId == ProfileId //Conversation
                })
                .Search(request.Query, s => s.message.CreatedAt, x => x.message.Content!)
                .PaginateByPageWithCounter(request.PageNumber, request.Limit);


            var DataDtos = messages.Data?.Select(m => new MessageResponseDto(m.message, m.IsMember));

            return Ok(new PageResponseDto<MessageResponseDto>(messages.TotalCount, DataDtos));

        }

        [HttpGet("{message_id}")]
        public async Task<ActionResult<MessageResponseDto>> GetMessage(
            [FromRoute] string channel_id,
            string message_id,
            CancellationToken cancellationToken)
        {
            var ProfileId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (ProfileId == null)
            {
                return ERROR(Unauthorized, "Authentication failed");
            }

            var isMember = await IsMember(channel_id, ProfileId, cancellationToken);

            if (!isMember)
                return ERROR(Forbid, "You are not a member of this channel");


            var message = db.Messages.AsNoTracking()
                .Include(x => x.Profile)
                .Include(x => x.Attachments)
                .FirstOrDefault(m => m.Id == message_id);

            if (message == null)
                return ERROR(NotFound, "Message not found");

            var isUserChannelMember = await IsMember(message.ChannelId, message.Profile?.Id, cancellationToken);

            return Ok(new MessageResponseDto(message, isUserChannelMember));
        }


        [HttpPost()]
        [DisableRequestSizeLimit]
        [RequestFormLimits(ValueLengthLimit = int.MaxValue, MultipartBodyLengthLimit = int.MaxValue)]
        public async Task<ActionResult<MessageResponseDto>> SendMessage(
            [FromRoute] string channel_id,
            [FromForm] MessageRequestDto messageRequest,
            CancellationToken cancellationToken)
        {
            var ProfileId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (ProfileId == null)
            {
                return ERROR(Unauthorized, "Authentication failed");
            }

            var isMember = await IsMember(channel_id, ProfileId, cancellationToken);

            if (!isMember)
                return ERROR(Forbid, "You are not a member of this channel");


            var channel = await db.Channels.AsNoTracking().AnyAsync(c => c.Id == channel_id, cancellationToken);
            if (!channel)
            {
                return ERROR(NotFound, "Channel not found");
            }

            var message = new Message
            {
                ProfileId = ProfileId,
                ChannelId = channel_id,
                Content = messageRequest.Content,
            };

            var uploadedFiles = await attachmentManger.UploadFilesAsync(messageRequest.Files, ProfileId, cancellationToken);
            if (uploadedFiles != null)
            {
                uploadedFiles.ToList().ForEach(x => x.MessageId = message.Id);
                message.Attachments = uploadedFiles;
            }

            db.Messages.Add(message);
            await db.SaveChangesAsync(cancellationToken);

            var NewMessage = await db.Messages
                .Include(x => x.Profile)
                .Include(x => x.Attachments)
                .FirstAsync(x => x.Id == message.Id, cancellationToken);

            var messageResponse = new MessageResponseDto(NewMessage);

            var members = await ChannelMembers(channel_id, cancellationToken);
            var clients = _hubContext.Clients.Users(members);

            await Task.WhenAll(
             clients.SendAsync($"{channel_id}:message:add", messageResponse, cancellationToken),
             clients.SendAsync("ReceiveNotification", messageResponse, cancellationToken)
         );

            return Created(messageResponse);
        }


        [HttpPatch("{message_id}")]
        public async Task<ActionResult<MessageResponseDto>> EditMessage(
            [FromRoute] string channel_id,
            string message_id,
            [FromForm] MessageRequestDto messageRequest,
             CancellationToken cancellationToken
            )
        {
            var ProfileId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (ProfileId == null)
            {
                return ERROR(Unauthorized, "Authentication failed");
            }

            var isMember = await IsMember(channel_id, ProfileId, cancellationToken);
            if (!isMember)
                return ERROR(Forbid, "You are not a member of this channel");


            var message = await db.Messages
                .Include(m => m.Profile)
                .Include(m => m.Attachments)
                .FirstOrDefaultAsync(m => m.Id == message_id && m.ChannelId == channel_id, cancellationToken);
            if (message == null)
            {
                return ERROR(NotFound, "Message not found");
            }

            bool UserCanEditMessage = message.ProfileId == ProfileId;

            if (!UserCanEditMessage)
                return ERROR(Forbid, "You do not have permission to edit this message");


            message.Content = messageRequest?.Content ?? message.Content;
            message.UpdatedAt = DateTime.UtcNow;

            if (messageRequest?.Files != null && messageRequest.Files.Count > 0)
            {
                var uploadedFiles = await attachmentManger.UploadFilesAsync(messageRequest.Files, ProfileId, cancellationToken);
                if (uploadedFiles != null)
                {
                    message?.Attachments?.ToList().ForEach(a => a.MessageId = null);
                    uploadedFiles.ToList().ForEach(x => x.MessageId = message?.Id);
                    message!.Attachments = uploadedFiles;
                }
            }

            await db.SaveChangesAsync(cancellationToken);


            var messageResponse = new MessageResponseDto(message);

            var members = await ChannelMembers(channel_id, cancellationToken);
            await _hubContext.Clients.Users(members).SendAsync($"{channel_id}:message:update", messageResponse, cancellationToken);


            return Ok(messageResponse);
        }



        [HttpDelete("{message_id}")]
        public async Task<ActionResult<MessageResponseDto>> DeleteMessage(
            [FromRoute] string channel_id,
            string message_id,
            CancellationToken cancellationToken
            )
        {
            var ProfileId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (ProfileId == null)
            {
                return ERROR(Unauthorized, "Authentication failed");
            }

            var isMember = await IsMember(channel_id, ProfileId, cancellationToken);
            if (!isMember)
                return ERROR(Forbid, "You are not a member of this channel");


            var message = await db.Messages
                .Include(m => m.Profile)
                .Include(m => m.Attachments)
                .FirstOrDefaultAsync(m => m.Id == message_id && m.ChannelId == channel_id, cancellationToken);
            if (message == null)
            {
                return ERROR(NotFound, "Message not found");
            }

            var member = await db.Members.FirstOrDefaultAsync(x =>
            x.ProfileId == ProfileId
            && x.Server.Channels.Any(c => c.Id == channel_id),
            cancellationToken
            );

            bool UserCanDeleteMessage = member?.Role is eMemberRole.ADMIN or eMemberRole.MODERATOR || message.ProfileId == ProfileId;
            if (!UserCanDeleteMessage)
                return ERROR(Forbid, "You do not have permission to delete this message");

            db.Messages.Remove(message);
            await db.SaveChangesAsync(cancellationToken);

            var messageResponse = new MessageResponseDto(message);

            var members = await ChannelMembers(channel_id, cancellationToken);
            await _hubContext.Clients.Users(members).SendAsync($"{channel_id}:message:delete", new
            {
                messageResponse.Id
            }, cancellationToken);

            return Ok(messageResponse);
        }


    }
}
