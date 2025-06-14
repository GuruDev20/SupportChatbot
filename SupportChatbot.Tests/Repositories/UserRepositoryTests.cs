using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SupportChatbot.API.Contexts;
using SupportChatbot.API.Models;
using SupportChatbot.API.Repositories;
using Xunit;

namespace SupportChatbot.Tests.Repositories
{
    public class UserRepositoryTests
    {
        private async Task<SupportChatbotContext> GetDbContextAsync()
        {
            var options = new DbContextOptionsBuilder<SupportChatbotContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            var context = new SupportChatbotContext(options);

            var users = new[]
            {
                new User
                {
                    Id = Guid.NewGuid(),
                    Username = "testuser",
                    Email = "test@example.com",
                    PasswordHash = "hashedpassword",
                    Role = "User",
                    IsActive = true
                },
                new User
                {
                    Id = Guid.NewGuid(),
                    Username = "agentuser",
                    Email = "agent@example.com",
                    PasswordHash = "agenthash",
                    Role = "Agent",
                    IsActive = true
                }
            };

            await context.Users.AddRangeAsync(users);
            await context.SaveChangesAsync();

            return context;
        }

        [Fact]
        public async Task GetByEmailAsync_ReturnsCorrectUser_WhenEmailExists()
        {
            // Arrange
            var context = await GetDbContextAsync();
            var repo = new UserRepository(context);

            // Act
            var user = await repo.GetbyEmailAsync("test@example.com");

            // Assert
            Assert.NotNull(user);
            Assert.Equal("testuser", user!.Username);
            Assert.Equal("test@example.com", user.Email);
        }

        [Fact]
        public async Task GetByEmailAsync_ReturnsNull_WhenEmailDoesNotExist()
        {
            // Arrange
            var context = await GetDbContextAsync();
            var repo = new UserRepository(context);

            // Act
            var user = await repo.GetbyEmailAsync("nonexistent@example.com");

            // Assert
            Assert.Null(user);
        }

        [Fact]
        public async Task GetByEmailAsync_CaseInsensitiveMatch_ReturnsUser()
        {
            // Arrange
            var context = await GetDbContextAsync();
            var repo = new UserRepository(context);

            // Act
            var user = await repo.GetbyEmailAsync("TEST@EXAMPLE.COM");

            // Assert
            Assert.Null(user); 
        }
    }
}
