using Microsoft.EntityFrameworkCore;
using SupportChatbot.API.Contexts;
using SupportChatbot.API.Interfaces;
using SupportChatbot.API.Models;

namespace SupportChatbot.API.Repositories
{
    public class AuthRepository : IAuthRepository
    {
        private readonly SupportChatbotContext _context;
        public AuthRepository(SupportChatbotContext context)
        {
            _context = context;
        }

        public async Task<string?> GetRefreshTokenAsync(Guid userId)
        {
            if (userId == Guid.Empty)
            {
                throw new ArgumentException("User ID cannot be empty", nameof(userId));
            }
            var token=await _context.RefreshTokens.FirstOrDefaultAsync(t => t.UserId == userId);
            return token?.Token;
        }

        public async Task<User?> GetUserbyEmailAsync(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                throw new ArgumentException("Email cannot be null or empty", nameof(email));
            }
            return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task RemoveRefreshTokenAsync(Guid userId)
        {
            if (userId == Guid.Empty)
            {
                throw new ArgumentException("User ID cannot be empty", nameof(userId));
            }
            var token= _context.RefreshTokens.FirstOrDefault(t => t.UserId == userId);
            if (token != null)
            {
                _context.RefreshTokens.Remove(token);
                await _context.SaveChangesAsync();
            }
        }

        public async Task SaveRefreshTokenAsync(Guid userId, string refreshToken)
        {
            if (string.IsNullOrEmpty(refreshToken))
            {
                throw new ArgumentException("Refresh token cannot be null or empty", nameof(refreshToken));
            }
            var token = await _context.RefreshTokens.FirstOrDefaultAsync(t => t.UserId == userId);
            if (token == null)
            {
                _context.RefreshTokens.Add(new RefreshToken
                {
                    UserId = userId,
                    Token = refreshToken
                });
            }
            else
            {
                token.Token = refreshToken;
            }
            await _context.SaveChangesAsync();
        }

        public async Task UpdateUserAsync(User user)
        {
            var existingUser = await _context.Users.FindAsync(user.Id);
            if (existingUser == null)
            {
                throw new KeyNotFoundException($"User with ID {user.Id} not found.");
            }

            _context.Entry(existingUser).CurrentValues.SetValues(user);
            await _context.SaveChangesAsync();

        }

        public async Task<bool> ValidateCredentialsAsync(string email, string password)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                throw new ArgumentException("Email and password cannot be null or empty");
            }
            var user = await GetUserbyEmailAsync(email);
            if (user == null)
            {
                return false;
            }
            return user.PasswordHash==password;
        }
    }
}