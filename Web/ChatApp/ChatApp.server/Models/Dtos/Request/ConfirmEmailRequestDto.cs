namespace ChatApi.server.Models.Dtos.Request
{
    public class ConfirmEmailRequestDto
    {
        public string Email { get; set; }
        public string Token { get; set; }
    }

}
