using SupportChatbot.API.Models;

namespace SupportChatbot.API.Interfaces
{
    public interface IUserRepository : IRepository<Guid, User>
    {
        public Task<User?> GetbyEmailAsync(string email);
    }
}