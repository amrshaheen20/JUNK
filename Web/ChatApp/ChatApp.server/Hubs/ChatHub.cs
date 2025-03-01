using ChatApi.server.Context;
using ChatApi.server.Models.DbSet;
using ChatApi.server.Models.Dtos.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using SignalRSwaggerGen.Attributes;
using System.Security.Claims;

namespace ChatApi.server.Hubs
{
    [Authorize]
    [SignalRHub("api/socket/chathub")]
    public class ChatHub : Hub
    {
        private readonly DataBaseContext db;

        public ChatHub(DataBaseContext db)
        {
            this.db = db;
        }

        private string? GetUserId() =>
            Context.User?.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value;

        private async Task<bool> IsUserAuthorized(string channelId, string userId) =>
            await db.Channels
                .AsNoTracking()
                .AnyAsync(c => c.Id == channelId &&
                    (c.Server!.Members.Any(m => m.ProfileId == userId) ||
                     c.Conversation!.ProfileOneId == userId ||
                     c.Conversation.ProfileTwoId == userId));

        private async Task<List<Profile>> OnlineChannelMembersAsync(string channelId)
        {
            string? userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
                return new List<Profile>();

            var channel = await db.Channels
                .AsNoTracking()
                .Where(c => c.Id == channelId &&
                    (c.Server!.Members.Any(m => m.ProfileId == userId) ||
                     c.Conversation!.ProfileOneId == userId ||
                     c.Conversation.ProfileTwoId == userId))
                .Select(c => new
                {
                    c.ServerId,
                    ProfileOne = c.Conversation!.ProfileOne,
                    ProfileTwo = c.Conversation.ProfileTwo
                })
                .FirstOrDefaultAsync();

            if (channel == null)
                return new List<Profile>();

            var serverMembers = await db.Members
                .AsNoTracking()
                .Where(m => m.ServerId == channel.ServerId)
                .Select(m => m.Profile)
                .ToListAsync();


            return serverMembers
                .Union(new[] { channel.ProfileOne, channel.ProfileTwo })
                .Where(p => p != null && p.Id != userId && p.LastActiveTime.AddMinutes(Constants.LAST_ACTIVE_NUMBER) > DateTime.UtcNow)
                .ToList()!;
        }

        public async Task JoinChannel(string channelId)
        {
            string? userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
                throw new UnauthorizedAccessException("Unauthorized!");

            if (!await IsUserAuthorized(channelId, userId))
                throw new InvalidOperationException("Forbidden: You are not a member of this channel.");

            var onlineMembers = await OnlineChannelMembersAsync(channelId);
            if (!onlineMembers.Any())
                throw new InvalidOperationException("No online members found.");

            var profile = await db.Profiles
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == userId)
                ?? throw new InvalidOperationException("Profile not found.");

            await Clients.Users(onlineMembers.Select(x => x.Id))
                .SendAsync($"{channelId}:call:join", new ProfileResponseDto(profile));
        }
        public async Task SendSignalToChannel(string channelId, string data, string from, string to)
        {
            string? userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
                throw new UnauthorizedAccessException("Unauthorized!");

            if (!await IsUserAuthorized(channelId, userId))
                throw new InvalidOperationException("Forbidden: You are not a member of this channel.");

            var channel = await db.Channels
                .AsNoTracking()
                .Where(c => c.Id == channelId)
                .Select(c => new
                {
                    SenderIsMember = c.Server!.Members.Any(m => m.ProfileId == from)
                                  || c.Conversation!.ProfileOneId == from
                                  || c.Conversation.ProfileTwoId == from,
                    ReceiverIsMember = c.Server.Members.Any(m => m.ProfileId == to)
                                    || c.Conversation!.ProfileOneId == to
                                    || c.Conversation.ProfileTwoId == to
                })
                .FirstOrDefaultAsync();


            if (channel == null || !channel.SenderIsMember)
                throw new InvalidOperationException("Forbidden: Sender is not a member of the channel.");

            if (!channel.ReceiverIsMember)
                throw new InvalidOperationException("Forbidden: Receiver is not a member of the channel.");

            var profile = await db.Profiles
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == from)
                ?? throw new InvalidOperationException("Profile not found.");

            await Clients.User(to).SendAsync($"{channelId}:call:signal", data, profile);
        }

        [SignalRHidden]
        public override async Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();
        }
        [SignalRHidden]
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            await base.OnDisconnectedAsync(exception);
        }
    }
}
