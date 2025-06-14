using System.Text;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Moq;
using SupportChatbot.API.DTOs;
using SupportChatbot.API.Interfaces;
using SupportChatbot.API.Models;
using SupportChatbot.API.Services;
using Xunit;

namespace SupportChatbot.Tests.Services
{
    public class FileServiceTests
    {
        private readonly Mock<IRepository<Guid, FileUpload>> _fileRepoMock;
        private readonly Mock<IRepository<Guid, ChatSession>> _chatSessionRepoMock;
        private readonly Mock<IRepository<Guid, Message>> _messageRepoMock;
        private readonly Mock<IWebHostEnvironment> _envMock;
        private readonly FileService _fileService;

        public FileServiceTests()
        {
            _fileRepoMock = new Mock<IRepository<Guid, FileUpload>>();
            _chatSessionRepoMock = new Mock<IRepository<Guid, ChatSession>>();
            _messageRepoMock = new Mock<IRepository<Guid, Message>>();
            _envMock = new Mock<IWebHostEnvironment>();

            _envMock.Setup(e => e.WebRootPath).Returns("TestRoot");

            _fileService = new FileService(
                _fileRepoMock.Object,
                _envMock.Object,
                _chatSessionRepoMock.Object,
                _messageRepoMock.Object
            );
        }

        [Fact]
        public async Task UploadFileAsync_ValidChatSession_UploadsFileAndReturnsResponse()
        {
            // Arrange
            var chatSessionId = Guid.NewGuid();
            var uploaderId = Guid.NewGuid();
            var fileContent = "Hello, world!";
            var fileName = "test.txt";
            var fileMock = new Mock<IFormFile>();

            var ms = new MemoryStream(Encoding.UTF8.GetBytes(fileContent));
            fileMock.Setup(f => f.OpenReadStream()).Returns(ms);
            fileMock.Setup(f => f.FileName).Returns(fileName);
            fileMock.Setup(f => f.Length).Returns(ms.Length);
            fileMock.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), default)).Returns((Stream s, CancellationToken _) =>
            {
                return ms.CopyToAsync(s);
            });

            var dto = new FileUploadDto
            {
                ChatSessionId = chatSessionId,
                UploaderId = uploaderId,
                File = fileMock.Object
            };

            _chatSessionRepoMock.Setup(r => r.GetByIdAsync(chatSessionId))
                                .ReturnsAsync(new ChatSession { Id = chatSessionId, Status = "Active" });

            _fileRepoMock.Setup(r => r.AddAsync(It.IsAny<FileUpload>()))
                         .ReturnsAsync((FileUpload f) => f);

            _messageRepoMock.Setup(r => r.AddAsync(It.IsAny<Message>()))
                            .ReturnsAsync((Message m) => m);

            // Act
            var result = await _fileService.UploadFileAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.EndsWith(fileName, result.FileName);
            Assert.StartsWith("/uploads/", result.FileUrl);
        }

        [Fact]
        public async Task UploadFileAsync_EndedChatSession_ThrowsInvalidOperationException()
        {
            var chatSessionId = Guid.NewGuid();

            _chatSessionRepoMock.Setup(r => r.GetByIdAsync(chatSessionId))
                                .ReturnsAsync(new ChatSession { Id = chatSessionId, Status = "Ended" });

            var dto = new FileUploadDto
            {
                ChatSessionId = chatSessionId,
                UploaderId = Guid.NewGuid(),
                File = Mock.Of<IFormFile>()
            };

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _fileService.UploadFileAsync(dto));
        }

        [Fact]
        public async Task GetFileAsync_FileExists_ReturnsFileContent()
        {
            var fileName = "test.txt";
            var content = Encoding.UTF8.GetBytes("Test content");
            var filePath = Path.Combine("TestRoot", "uploads", fileName);

            Directory.CreateDirectory(Path.Combine("TestRoot", "uploads"));
            await File.WriteAllBytesAsync(filePath, content);

            // Act
            var result = await _fileService.GetFileAsync(fileName);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(content, result.FileContent);
            Assert.Equal("application/octet-stream", result.ContentType);
            Assert.Equal(fileName, result.FileName);

            // Cleanup
            File.Delete(filePath);
        }

        [Fact]
        public async Task GetFileAsync_FileDoesNotExist_ThrowsFileNotFoundException()
        {
            var fileName = "nonexistent.txt";

            await Assert.ThrowsAsync<FileNotFoundException>(() =>
                _fileService.GetFileAsync(fileName));
        }
    }
}
