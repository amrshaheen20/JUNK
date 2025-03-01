namespace ChatApi.server.Models.Dtos.Response
{
    public class LoginResponseDto
    {
        public string Id { get; set; }
        public string Token { get; set; }
        public string Email { get; set; }
        public string UserName { get; set; }
        public DateTime Expiration { get; set; }
    }
}
