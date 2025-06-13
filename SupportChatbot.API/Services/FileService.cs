using SupportChatbot.API.DTOs;
using SupportChatbot.API.Interfaces;
using SupportChatbot.API.Models;

namespace SupportChatbot.API.Services
{
    public class FileService : IFileService
    {
        private readonly IRepository<Guid, FileUpload> _fileRepository;
        private readonly IRepository<Guid, ChatSession> _chatSessionRepository;
        private readonly IRepository<Guid, Message> _messageRepository;
        private readonly IWebHostEnvironment _env;
        public FileService(IRepository<Guid, FileUpload> fileRepository, IWebHostEnvironment env, IRepository<Guid, ChatSession> chatSessionRepository, IRepository<Guid, Message> messageRepository)
        {
            _messageRepository = messageRepository;
            _chatSessionRepository = chatSessionRepository;
            _fileRepository = fileRepository;
            _env = env;
        }

        public async Task<(byte[] FileContent, string ContentType, string FileName)> GetFileAsync(string fileName)
        {
            var uploadsFolder = Path.Combine(_env.WebRootPath ?? "wwwroot", "uploads");
            var filePath = Path.Combine(uploadsFolder, fileName);
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("File not found.", fileName);
            }

            var content = await File.ReadAllBytesAsync(filePath);
            var contentType = "application/octet-stream";
            return (content, contentType, fileName);
        }

        public async Task<FileResponseDto> UploadFileAsync(FileUploadDto dto)
        {
            var chatSession = await _chatSessionRepository.GetByIdAsync(dto.ChatSessionId);
            if (chatSession == null || chatSession.Status == "Ended")
            {
                throw new InvalidOperationException("Cannot upload file. Chat session is either invalid or has ended.");
            }
            var uploadsFolder = Path.Combine(_env.WebRootPath ?? "wwwroot", "uploads");
            Directory.CreateDirectory(uploadsFolder);
            var fileName = $"{Guid.NewGuid()}_{dto.File.FileName}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await dto.File.CopyToAsync(stream);
            }

            var fileUpload = new FileUpload
            {
                ChatSessionId = dto.ChatSessionId,
                UploaderId = dto.UploaderId,
                FileName = fileName,
                FilePath = $"/uploads/{fileName}",
            };
            await _fileRepository.AddAsync(fileUpload);
            var message = new Message
            {
                ChatSessionId = dto.ChatSessionId,
                SenderId = dto.UploaderId,
                Content = fileUpload.FilePath,
                IsFile = true,
                SentAt = DateTime.UtcNow
            };
            await _messageRepository.AddAsync(message);

            return new FileResponseDto
            {
                FileName = fileUpload.FileName,
                FileUrl = fileUpload.FilePath,
            };
        }
    }
}