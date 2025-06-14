using System;
using System.Threading.Tasks;
using Moq;
using SupportChatbot.API.DTOs.Auth;
using SupportChatbot.API.Interfaces;
using SupportChatbot.API.Interfaces.Services;
using SupportChatbot.API.Models;
using SupportChatbot.API.Services;
using Xunit;

namespace SupportChatbot.Tests.Services
{
    public class AuthServiceTests
    {
        private readonly Mock<IAuthRepository> _authRepositoryMock;
        private readonly Mock<ITokenService> _tokenServiceMock;
        private readonly AuthService _authService;

        public AuthServiceTests()
        {
            _authRepositoryMock = new Mock<IAuthRepository>();
            _tokenServiceMock = new Mock<ITokenService>();
            _authService = new AuthService(_authRepositoryMock.Object, _tokenServiceMock.Object);
        }

        [Fact]
        public async Task RefreshTokenAsync_ValidToken_ReturnsLoginResponse()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var refreshToken = "valid-refresh-token";
            var user = new User
            {
                Id = userId,
                Email = "test@example.com",
                Username = "testuser",
                Role = "User"
            };

            _tokenServiceMock.Setup(x => x.ValidateRefreshToken(refreshToken)).Returns(userId);
            _authRepositoryMock.Setup(x => x.GetRefreshTokenAsync(userId)).ReturnsAsync(refreshToken);
            _authRepositoryMock.Setup(x => x.GetUserbyEmailAsync(userId.ToString())).ReturnsAsync(user);
            _tokenServiceMock.Setup(x => x.GenerateAccessToken(user)).Returns("new-access-token");
            _tokenServiceMock.Setup(x => x.GenerateRefreshToken()).Returns("new-refresh-token");

            // Act
            var result = await _authService.RefreshTokenAsync(new RefreshTokenRequestDto { RefreshToken = refreshToken });

            // Assert
            Assert.NotNull(result);
            Assert.Equal("new-access-token", result.AccessToken);
            Assert.Equal("new-refresh-token", result.RefreshToken);
        }

        [Fact]
        public async Task RefreshTokenAsync_InvalidToken_ThrowsUnauthorizedAccessException()
        {
            // Arrange
            _tokenServiceMock.Setup(x => x.ValidateRefreshToken(It.IsAny<string>())).Returns((Guid?)null);

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                _authService.RefreshTokenAsync(new RefreshTokenRequestDto { RefreshToken = "invalid" }));
        }

        [Fact]
        public async Task RefreshTokenAsync_TokenMismatch_ThrowsUnauthorizedAccessException()
        {
            // Arrange
            var userId = Guid.NewGuid();
            _tokenServiceMock.Setup(x => x.ValidateRefreshToken(It.IsAny<string>())).Returns(userId);
            _authRepositoryMock.Setup(x => x.GetRefreshTokenAsync(userId)).ReturnsAsync("stored-token");

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                _authService.RefreshTokenAsync(new RefreshTokenRequestDto { RefreshToken = "different-token" }));
        }

        [Fact]
        public async Task RefreshTokenAsync_UserNotFound_ThrowsUnauthorizedAccessException()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var refreshToken = "valid-refresh-token";
            _tokenServiceMock.Setup(x => x.ValidateRefreshToken(refreshToken)).Returns(userId);
            _authRepositoryMock.Setup(x => x.GetRefreshTokenAsync(userId)).ReturnsAsync(refreshToken);
            _authRepositoryMock.Setup(x => x.GetUserbyEmailAsync(userId.ToString())).ReturnsAsync((User)null);

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                _authService.RefreshTokenAsync(new RefreshTokenRequestDto { RefreshToken = refreshToken }));
        }
    }
}