using SupportChatbot.API.DTOs;
using SupportChatbot.API.DTOs.Auth;
using SupportChatbot.API.Interfaces;
using SupportChatbot.API.Interfaces.Services;

namespace SupportChatbot.API.Services
{
    public class AuthService : IAuthService
    {
        private readonly IAuthRepository _authRepository;
        private readonly ITokenService _tokenService;
        public AuthService(IAuthRepository authRepository, ITokenService tokenService)
        {
            _authRepository = authRepository;
            _tokenService = tokenService;
        }

        public async Task<UserInfoDto> GetUserInfoAsync(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                throw new ArgumentException("Email cannot be null or empty.", nameof(email));
            }
            var user = await _authRepository.GetUserbyEmailAsync(email);
            if (user == null)
            {
                throw new UnauthorizedAccessException("User not found.");
            }
            return new UserInfoDto
            {
                Id = user.Id.ToString(),
                Email = user.Email,
                Username = user.Username,
                Role = user.Role,
                ProfilePictureUrl = user.ProfilePictureUrl ?? string.Empty
            };
        }

        public async Task<LoginResponseDto> LoginAsync(LoginRequestDto loginRequest)
        {
            var user = await _authRepository.GetUserbyEmailAsync(loginRequest.Email);
            if (user == null)
            {
                throw new UnauthorizedAccessException("Invalid email or password.");
            }
            if(user.IsDeleted)
            {
                throw new UnauthorizedAccessException("User account is deleted.");
            }
            var passwordValid = BCrypt.Net.BCrypt.Verify(loginRequest.Password, user.PasswordHash);
            if (!passwordValid)
            {
                return null!;
            }
            user.IsActive = true;
            user.UpdatedAt = DateTime.UtcNow;
            await _authRepository.UpdateUserAsync(user);
            var accessToken = _tokenService.GenerateAccessToken(user);
            var refreshToken = _tokenService.GenerateRefreshToken();
            await _authRepository.SaveRefreshTokenAsync(user.Id, refreshToken);
            return new LoginResponseDto
            {
                UserId = user.Id,
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                AccessTokenExpiration = DateTime.UtcNow.AddMinutes(60)
            };
        }

        public async Task LogoutAsync(string refreshToken)
        {
            var userId = _tokenService.ValidateRefreshToken(refreshToken);
            if (userId != null)
            {
                await _authRepository.RemoveRefreshTokenAsync(userId.Value);   
            }
        }

        public async Task<LoginResponseDto> RefreshTokenAsync(RefreshTokenRequestDto refreshTokenRequest)
        {
            var userId = _tokenService.ValidateRefreshToken(refreshTokenRequest.RefreshToken);
            if (userId == null)
            {
                throw new UnauthorizedAccessException("Invalid refresh token.");
            }
            var storedToken = await _authRepository.GetRefreshTokenAsync(userId.Value);
            if (storedToken != refreshTokenRequest.RefreshToken)
            {
                throw new UnauthorizedAccessException("Invalid refresh token.");
            }
            var user = await _authRepository.GetUserbyEmailAsync(userId.ToString()!);
            if (user == null)
            {
                throw new UnauthorizedAccessException("User not found.");
            }
            var newAccessToken = _tokenService.GenerateAccessToken(user);
            var newRefreshToken = _tokenService.GenerateRefreshToken();
            await _authRepository.SaveRefreshTokenAsync(user.Id, newRefreshToken);
            return new LoginResponseDto
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken,
                AccessTokenExpiration = DateTime.UtcNow.AddMinutes(60)
            };
        }
    }
}