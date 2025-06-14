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
    public class MessageRepositoryTests
    {
        private async Task<SupportChatbotContext> GetInMemoryDbContextAsync()
        {
            var options = new DbContextOptionsBuilder<SupportChatbotContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            var context = new SupportChatbotContext(options);
            context.Database.EnsureCreated();

            var chatSessionId = Guid.NewGuid();
            var messages = new List<Message>
            {
                new Message
                {
                    Id = Guid.NewGuid(),
                    ChatSessionId = chatSessionId,
                    SenderId = Guid.NewGuid(),
                    Content = "Hello",
                    SentAt = DateTime.UtcNow.AddMinutes(-2),
                    IsFile = false
                },
                new Message
                {
                    Id = Guid.NewGuid(),
                    ChatSessionId = chatSessionId,
                    SenderId = Guid.NewGuid(),
                    Content = "How can I help you?",
                    SentAt = DateTime.UtcNow.AddMinutes(-1),
                    IsFile = false
                }
            };

            await context.Messages.AddRangeAsync(messages);
            await context.SaveChangesAsync();

            return context;
        }

        [Fact]
        public async Task GetMessagesByChatSessionIdAsync_ReturnsOrderedMessages()
        {
            // Arrange
            var context = await GetInMemoryDbContextAsync();
            var repo = new MessageRepository(context);
            var sessionId = context.Messages.First().ChatSessionId;

            // Act
            var result = await repo.GetMessagesByChatSessionIdAsync(sessionId);

            // Assert
            Assert.NotNull(result);
            var resultList = result.ToList();
            Assert.Equal(2, resultList.Count);
            Assert.True(resultList[0].SentAt < resultList[1].SentAt);
        }

        [Fact]
        public async Task GetMessagesByChatSessionIdAsync_ReturnsEmpty_WhenNoMessages()
        {
            // Arrange
            var context = await GetInMemoryDbContextAsync();
            var repo = new MessageRepository(context);
            var nonExistentSessionId = Guid.NewGuid();

            // Act
            var result = await repo.GetMessagesByChatSessionIdAsync(nonExistentSessionId);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }
    }
}
