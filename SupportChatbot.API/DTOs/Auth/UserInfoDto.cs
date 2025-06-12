namespace SupportChatbot.API.DTOs.Auth
{
    public class UserInfoDto
    {
        public string Id { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Username { get; set; } = null!;
        public string ProfilePictureUrl { get; set; } = null!;
        public string Role { get; set; } = null!;
    }
}
