using Microsoft.EntityFrameworkCore;
using SupportChatbot.API.Contexts;
using SupportChatbot.API.Interfaces;
using SupportChatbot.API.Models;

namespace SupportChatbot.API.Repositories
{
    public class FileRepository : Repository<Guid, FileUpload>, IFileRepository
    {
        private readonly SupportChatbotContext _context;

        public FileRepository(SupportChatbotContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<FileUpload>> GetFilesByChatSessionIdAsync(Guid chatSessionId)
        {
            return await _context.FileUploads
                .Where(f => f.ChatSessionId == chatSessionId)
                .ToListAsync();
        }

        public async Task<FileUpload?> GetFileByFileNameAsync(string fileName)
        {
            return await _context.FileUploads
                .FirstOrDefaultAsync(f => f.FileName == fileName);
        }
    }
}
