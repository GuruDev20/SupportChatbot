using Microsoft.EntityFrameworkCore;
using SupportChatbot.API.Contexts;
using SupportChatbot.API.Models;
using SupportChatbot.API.Repositories;
using Xunit;
using System;
using System.Threading.Tasks;

namespace SupportChatbot.Tests.Repositories
{
    public class AuthRepositoryTests : IDisposable
    {
        private readonly SupportChatbotContext _context;
        private readonly AuthRepository _authRepository;

        public AuthRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<SupportChatbotContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new SupportChatbotContext(options);
            _authRepository = new AuthRepository(_context);

            // Seed initial data
            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = "test@example.com",
                PasswordHash = "hashedpassword"
            };
            _context.Users.Add(user);
            _context.SaveChanges();
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        [Fact]
        public async Task GetUserbyEmailAsync_ReturnsUser_WhenExists()
        {
            var result = await _authRepository.GetUserbyEmailAsync("test@example.com");

            Assert.NotNull(result);
            Assert.Equal("test@example.com", result.Email);
        }

        [Fact]
        public async Task GetUserbyEmailAsync_ReturnsNull_WhenNotFound()
        {
            var result = await _authRepository.GetUserbyEmailAsync("missing@example.com");

            Assert.Null(result);
        }

        [Fact]
        public async Task SaveRefreshTokenAsync_AddsToken_WhenNotExists()
        {
            var userId = _context.Users.First().Id;

            await _authRepository.SaveRefreshTokenAsync(userId, "token123");

            var token = await _context.RefreshTokens.FirstOrDefaultAsync(t => t.UserId == userId);
            Assert.NotNull(token);
            Assert.Equal("token123", token.Token);
        }

        [Fact]
        public async Task SaveRefreshTokenAsync_UpdatesToken_WhenExists()
        {
            var userId = _context.Users.First().Id;

            // Add initial token
            _context.RefreshTokens.Add(new RefreshToken { UserId = userId, Token = "old" });
            await _context.SaveChangesAsync();

            await _authRepository.SaveRefreshTokenAsync(userId, "new");

            var token = await _context.RefreshTokens.FirstOrDefaultAsync(t => t.UserId == userId);
            Assert.NotNull(token);
            Assert.Equal("new", token.Token);
        }

        [Fact]
        public async Task GetRefreshTokenAsync_ReturnsToken()
        {
            var userId = _context.Users.First().Id;
            _context.RefreshTokens.Add(new RefreshToken { UserId = userId, Token = "xyz" });
            await _context.SaveChangesAsync();

            var token = await _authRepository.GetRefreshTokenAsync(userId);
            Assert.Equal("xyz", token);
        }

        [Fact]
        public async Task RemoveRefreshTokenAsync_RemovesToken()
        {
            var userId = _context.Users.First().Id;
            _context.RefreshTokens.Add(new RefreshToken { UserId = userId, Token = "to-remove" });
            await _context.SaveChangesAsync();

            await _authRepository.RemoveRefreshTokenAsync(userId);

            var token = await _context.RefreshTokens.FirstOrDefaultAsync(t => t.UserId == userId);
            Assert.Null(token);
        }

        [Fact]
        public async Task UpdateUserAsync_UpdatesUserProperties()
        {
            var user = _context.Users.First();
            user.Email = "updated@example.com";

            await _authRepository.UpdateUserAsync(user);

            var updatedUser = await _context.Users.FindAsync(user.Id);
            Assert.Equal("updated@example.com", updatedUser.Email);
        }

        [Fact]
        public async Task ValidateCredentialsAsync_ReturnsTrue_WhenValid()
        {
            var result = await _authRepository.ValidateCredentialsAsync("test@example.com", "hashedpassword");
            Assert.True(result);
        }

        [Fact]
        public async Task ValidateCredentialsAsync_ReturnsFalse_WhenInvalidPassword()
        {
            var result = await _authRepository.ValidateCredentialsAsync("test@example.com", "wrong");
            Assert.False(result);
        }

        [Fact]
        public async Task ValidateCredentialsAsync_ReturnsFalse_WhenUserNotFound()
        {
            var result = await _authRepository.ValidateCredentialsAsync("noone@example.com", "whatever");
            Assert.False(result);
        }
    }
}
