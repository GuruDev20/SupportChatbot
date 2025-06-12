using Microsoft.EntityFrameworkCore;
using SupportChatbot.API.Contexts;
using SupportChatbot.API.Interfaces;
using SupportChatbot.API.Models;

namespace SupportChatbot.API.Repositories
{
    public class UserRepository : Repository<Guid, User>, IUserRepository
    {
        private readonly SupportChatbotContext _context;
        public UserRepository(SupportChatbotContext context) : base(context)
        {
            _context = context;
        }

        public async Task<User?> GetbyEmailAsync(string email)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        }
    }
}