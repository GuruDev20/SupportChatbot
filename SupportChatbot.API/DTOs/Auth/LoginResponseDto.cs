namespace SupportChatbot.API.DTOs
{
    public class LoginResponseDto
    {
        public string AccessToken{ get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public DateTime AccessTokenExpiration { get; set; }
    }
}