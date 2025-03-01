namespace ChatApi.server.Models.Dtos.Response
{
    public class OnlineStatusResponse
    {
        public string UserId { get; set; } = string.Empty;
        public bool IsOnline { get; set; }
        public DateTime? LastActive { get; set; }
    }
}
