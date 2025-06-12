using SupportChatbot.API.Models;

namespace SupportChatbot.API.Interfaces
{
    public interface IAuthRepository
    {
        public Task<User?> GetUserbyEmailAsync(string email);
        public Task<bool> ValidateCredentialsAsync(string email, string password);
        public Task SaveRefreshTokenAsync(Guid userId, string refreshToken);
        public Task<string?> GetRefreshTokenAsync(Guid userId);
        public Task RemoveRefreshTokenAsync(Guid userId);
    }
}