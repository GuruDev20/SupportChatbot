using SupportChatbot.API.Models;

namespace SupportChatbot.API.Interfaces.Services
{
    public interface ITokenService
    {
        string GenerateAccessToken(User user);
        string GenerateRefreshToken();
        Guid? ValidateRefreshToken(string refreshToken);
    }
}
