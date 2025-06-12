using SupportChatbot.API.DTOs;

namespace SupportChatbot.API.Interfaces
{
    public interface IUserService
    {
        public Task<UserResponseDto> RegisterUserAsync(RegisterUserDto registerUserDto);
        public Task<bool> DeleteUserAsync(Guid userId);
        public Task<UserResponseDto?> UpdateUserAsync(Guid userId, UpdateUserDto updateUserDto);
    }
}