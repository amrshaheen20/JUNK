using ChatApi.server.Context;
using ChatApi.server.Hubs;
using ChatApi.server.Models.DbSet;
using ChatApi.server.Models.Dtos.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;


namespace ChatApi.server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ConversationsController : MainControllere
    {
        private readonly IHubContext<ChatHub> hubContext;
        private readonly DataBaseContext db;

        public ConversationsController(DataBaseContext db, IHubContext<ChatHub> hubContext)
        {
            this.hubContext = hubContext;
            this.db = db;
        }


        [HttpPost("{target_profile_id}")]
        public async Task<ActionResult<ConversationResponseDto>> CreateConversation(
            string target_profile_id,
            CancellationToken cancellationToken
            )
        {
            var ProfileId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (ProfileId == null)
            {
                return ERROR(Unauthorized, "Authentication failed");
            }

            var ProfileTarget = await db.Profiles.FirstOrDefaultAsync(x => x.Id == target_profile_id, cancellationToken);
            if (ProfileTarget == null)
                return ERROR(NotFound, "Profile not found");

            if (ProfileId == target_profile_id)
                return ERROR(BadRequest, "You can't create a conversation with yourself");



            var ConversationExists = await db.Conversations.FirstOrDefaultAsync(x =>
            x.ProfileOneId == ProfileId && x.ProfileTwoId == target_profile_id ||
            x.ProfileOneId == target_profile_id && x.ProfileTwoId == ProfileId,
            cancellationToken
            );

            if (ConversationExists != null)
                return Ok(new ConversationResponseDto(ConversationExists, ProfileTarget));

            var Conversation = new Conversation
            {
                ProfileOneId = ProfileId,
                ProfileTwoId = target_profile_id,
                Channel = new Channel()
                {
                    Type = eChannelType.CONVERSATION,
                    Name = "Conversation",
                    ProfileId = ProfileId
                }
            };


            db.Conversations.Add(Conversation);
            await db.SaveChangesAsync(cancellationToken);

            var newConversation = new ConversationResponseDto(Conversation, ProfileTarget);

            await hubContext.Clients.Users(Conversation.ProfileOneId, Conversation.ProfileTwoId).SendAsync($"conversation:add", newConversation, cancellationToken);


            return Created(newConversation);


        }


        [HttpGet("{conversation_id}")]
        public async Task<ActionResult<ConversationResponseDto>> GetConversation(
            string conversation_id,
            CancellationToken cancellationToken
            )
        {
            var ProfileId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (ProfileId == null)
            {
                return ERROR(Unauthorized, "Authentication failed");
            }

            var conversation = await db.Conversations
                .AsNoTracking()
                .Where(x => x.Id == conversation_id
                            && (x.ProfileOneId == ProfileId || x.ProfileTwoId == ProfileId))
                .Select(x => new
                {
                    TargetProfile = ProfileId == x.ProfileOneId ? x.ProfileTwo : x.ProfileOne,
                    Conversation = x
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (conversation?.Conversation == null)
            {
                return ERROR(NotFound, "Can't found the conversation");
            }


            return Ok(new ConversationResponseDto(conversation.Conversation, conversation.TargetProfile!));
        }

        [HttpDelete("{conversation_id}")]
        public async Task<ActionResult<ConversationResponseDto>> DeleteConversation(string conversation_id, CancellationToken cancellationToken)//delete from both sides
        {
            var ProfileId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (ProfileId == null)
            {
                return ERROR(Unauthorized, "Authentication failed");
            }

            var conversation = await db.Conversations
                .Where(x => x.Id == conversation_id
                            && (x.ProfileOneId == ProfileId || x.ProfileTwoId == ProfileId))
                .Select(x => new
                {
                    TargetProfile = ProfileId == x.ProfileOneId ? x.ProfileTwo : x.ProfileOne,
                    Conversation = x
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (conversation?.Conversation == null)
                return ERROR(NotFound, "Conversation not found");


            db.Conversations.Remove(conversation.Conversation);
            await db.SaveChangesAsync(cancellationToken);

            var deletedConversation = new ConversationResponseDto(conversation.Conversation, conversation.TargetProfile!);

            await hubContext.Clients.Users(conversation.Conversation.ProfileOneId!, conversation.Conversation.ProfileTwoId!).SendAsync($"conversation:delete",
                new
                {
                    deletedConversation.Id
                }, cancellationToken);


            return Ok(deletedConversation);
        }


        //What should i edit?!
        //[EDIT]

    }
}
