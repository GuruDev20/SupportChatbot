using SupportChatbot.API.DTOs;
using SupportChatbot.API.DTOs.Auth;

namespace SupportChatbot.API.Interfaces
{
    public interface IAuthService
    {
        public Task<LoginResponseDto> LoginAsync(LoginRequestDto loginRequest);
        public Task<LoginResponseDto> RefreshTokenAsync(RefreshTokenRequestDto refreshTokenRequest);
        public Task<UserInfoDto> GetUserInfoAsync(string userId);
        public Task LogoutAsync(string refreshToken);
    }
}