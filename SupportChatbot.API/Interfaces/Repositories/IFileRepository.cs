using SupportChatbot.API.Models;

namespace SupportChatbot.API.Interfaces
{
    public interface IFileRepository : IRepository<Guid, FileUpload>
    {
        Task<IEnumerable<FileUpload>> GetFilesByChatSessionIdAsync(Guid chatSessionId);
        Task<FileUpload?> GetFileByFileNameAsync(string fileName);
    }
}
