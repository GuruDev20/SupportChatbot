using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SupportChatbot.API.Contexts;
using SupportChatbot.API.Models;
using SupportChatbot.API.Repositories;
using Xunit;

namespace SupportChatbot.Tests.Repositories
{
    public class ChatRepositoryTests
    {
        private async Task<SupportChatbotContext> GetDbContextAsync()
        {
            var options = new DbContextOptionsBuilder<SupportChatbotContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            var context = new SupportChatbotContext(options);

            var userId = Guid.NewGuid();
            var agentId = Guid.NewGuid();

            var sessions = new List<ChatSession>
            {
                new ChatSession
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    AgentId = agentId,
                    Status = "Active",
                    Messages = new List<Message>
                    {
                        new Message { Id = Guid.NewGuid(), Content = "Hello", SentAt = DateTime.UtcNow, SenderId = userId }
                    }
                },
                new ChatSession
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    AgentId = Guid.NewGuid(), 
                    Status = "Ended",
                    Messages = new List<Message>()
                }
            };

            await context.ChatSessions.AddRangeAsync(sessions);
            await context.SaveChangesAsync();

            return context;
        }

        [Fact]
        public async Task GetByUserIdAsync_ReturnsCorrectChatSessions()
        {
            // Arrange
            var context = await GetDbContextAsync();
            var repo = new ChatRepository(context);
            var userId = context.ChatSessions.First().UserId;

            // Act
            var result = await repo.GetByUserIdAsync(userId);

            // Assert
            Assert.NotNull(result);
            var list = result.ToList();
            Assert.Equal(2, list.Count);
            Assert.All(list, cs => Assert.Equal(userId, cs.UserId));
        }

        [Fact]
        public async Task GetByAgentIdAsync_ReturnsOnlyAgentSpecificSessions()
        {
            // Arrange
            var context = await GetDbContextAsync();
            var repo = new ChatRepository(context);
            var agentId = context.ChatSessions.First().AgentId;

            // Act
            var result = await repo.GetByAgentIdAsync(agentId);

            // Assert
            Assert.NotNull(result);
            var list = result.ToList();
            Assert.Single(list);
            Assert.All(list, cs => Assert.Equal(agentId, cs.AgentId));
        }

        [Fact]
        public async Task GetByUserIdAsync_ThrowsException_WhenUserIdEmpty()
        {
            var context = await GetDbContextAsync();
            var repo = new ChatRepository(context);

            await Assert.ThrowsAsync<ArgumentException>(() => repo.GetByUserIdAsync(Guid.Empty));
        }

        [Fact]
        public async Task GetByAgentIdAsync_ThrowsException_WhenAgentIdEmpty()
        {
            var context = await GetDbContextAsync();
            var repo = new ChatRepository(context);

            await Assert.ThrowsAsync<ArgumentException>(() => repo.GetByAgentIdAsync(Guid.Empty));
        }
    }
}
