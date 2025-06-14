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
    public class FileRepositoryTests
    {
        private async Task<SupportChatbotContext> GetInMemoryDbContextAsync()
        {
            var options = new DbContextOptionsBuilder<SupportChatbotContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString()) 
                .Options;

            var context = new SupportChatbotContext(options);
            context.Database.EnsureCreated();

            var chatSessionId = Guid.NewGuid();
            var files = new List<FileUpload>
            {
                new FileUpload
                {
                    Id = Guid.NewGuid(),
                    ChatSessionId = chatSessionId,
                    UploaderId = Guid.NewGuid(),
                    FileName = "file1.pdf",
                    FilePath = "/uploads/file1.pdf"
                },
                new FileUpload
                {
                    Id = Guid.NewGuid(),
                    ChatSessionId = chatSessionId,
                    UploaderId = Guid.NewGuid(),
                    FileName = "file2.docx",
                    FilePath = "/uploads/file2.docx"
                }
            };

            await context.FileUploads.AddRangeAsync(files);
            await context.SaveChangesAsync();

            return context;
        }

        [Fact]
        public async Task GetFilesByChatSessionIdAsync_ReturnsCorrectFiles()
        {
            // Arrange
            var context = await GetInMemoryDbContextAsync();
            var repo = new FileRepository(context);
            var sessionId = context.FileUploads.First().ChatSessionId;

            // Act
            var result = await repo.GetFilesByChatSessionIdAsync(sessionId);

            // Assert
            Assert.NotNull(result);
            var list = result.ToList();
            Assert.Equal(2, list.Count);
            Assert.All(list, f => Assert.Equal(sessionId, f.ChatSessionId));
        }

        [Fact]
        public async Task GetFilesByChatSessionIdAsync_ReturnsEmpty_WhenSessionIdNotFound()
        {
            // Arrange
            var context = await GetInMemoryDbContextAsync();
            var repo = new FileRepository(context);
            var nonexistentSessionId = Guid.NewGuid();

            // Act
            var result = await repo.GetFilesByChatSessionIdAsync(nonexistentSessionId);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetFileByFileNameAsync_ReturnsCorrectFile()
        {
            // Arrange
            var context = await GetInMemoryDbContextAsync();
            var repo = new FileRepository(context);

            // Act
            var result = await repo.GetFileByFileNameAsync("file1.pdf");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("file1.pdf", result.FileName);
            Assert.Equal("/uploads/file1.pdf", result.FilePath);
        }

        [Fact]
        public async Task GetFileByFileNameAsync_ReturnsNull_WhenNotFound()
        {
            // Arrange
            var context = await GetInMemoryDbContextAsync();
            var repo = new FileRepository(context);

            // Act
            var result = await repo.GetFileByFileNameAsync("nonexistent_file.txt");

            // Assert
            Assert.Null(result);
        }
    }
}
