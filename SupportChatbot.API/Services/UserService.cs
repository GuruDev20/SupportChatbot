using AutoMapper;
using SupportChatbot.API.DTOs;
using SupportChatbot.API.Interfaces;
using SupportChatbot.API.Models;

namespace SupportChatbot.API.Services
{
    public class UserService : IUserService
    {
        private readonly IRepository<Guid, User> _userRepository;
        private readonly IMapper _mapper;
        public UserService(IRepository<Guid, User> userRepository, IMapper mapper)
        {
            _userRepository = userRepository;
            _mapper = mapper;
        }

        public async Task<bool> DeleteUserAsync(Guid userId)
        {
            if (userId == Guid.Empty)
            {
                throw new ArgumentException("User ID cannot be empty.", nameof(userId));
            }
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                throw new KeyNotFoundException($"User with ID {userId} not found.");
            }
            user.IsDeleted = true;
            user.UpdatedAt = DateTime.UtcNow;

            await _userRepository.UpdateAsync(user);
            return true;
        }

        public async Task<UserResponseDto> RegisterUserAsync(RegisterUserDto registerUserDto)
        {
            if (registerUserDto == null)
            {
                throw new ArgumentNullException(nameof(registerUserDto), "Register user DTO cannot be null.");
            }

            var existingUsers = await _userRepository.GetAllAsync();

            var deletedUser = existingUsers
                .FirstOrDefault(u => u.Email == registerUserDto.Email && u.IsDeleted);

            var activeUser = existingUsers
                .FirstOrDefault(u => u.Email == registerUserDto.Email && !u.IsDeleted);

            if (activeUser != null)
            {
                throw new InvalidOperationException("A user with this email already exists.");
            }

            if (deletedUser != null)
            {
                deletedUser.IsDeleted = false;
                deletedUser.Username = registerUserDto.Username;
                deletedUser.PasswordHash = BCrypt.Net.BCrypt.HashPassword(registerUserDto.Password);
                deletedUser.ProfilePictureUrl = registerUserDto.ProfilePictureUrl;
                deletedUser.Role = registerUserDto.Role ?? "User";
                deletedUser.UpdatedAt = DateTime.UtcNow;

                await _userRepository.UpdateAsync(deletedUser.Id, deletedUser);

                return _mapper.Map<UserResponseDto>(deletedUser);
            }

            var user = new User
            {
                Username = registerUserDto.Username,
                Email = registerUserDto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(registerUserDto.Password),
                ProfilePictureUrl = registerUserDto.ProfilePictureUrl,
                Role = registerUserDto.Role ?? "User"
            };

            await _userRepository.AddAsync(user);
            return _mapper.Map<UserResponseDto>(user);
        }


        public async Task<UserResponseDto?> UpdateUserAsync(Guid userId, UpdateUserDto updateUserDto)
        {
            if (userId == Guid.Empty)
            {
                throw new ArgumentException("User ID cannot be empty.", nameof(userId));
            }
            if (updateUserDto == null)
            {
                throw new ArgumentNullException(nameof(updateUserDto), "Update user DTO cannot be null.");
            }
            var user =await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                throw new KeyNotFoundException($"User with ID {userId} not found.");
            }
            if (!string.IsNullOrWhiteSpace(updateUserDto.Username))
            {
                user.Username = updateUserDto.Username;
            }

            if (!string.IsNullOrWhiteSpace(updateUserDto.ProfilePictureUrl))
            {
                user.ProfilePictureUrl = updateUserDto.ProfilePictureUrl;
            }

            if (!string.IsNullOrWhiteSpace(updateUserDto.Password))
            {
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(updateUserDto.Password);
            }

            await _userRepository.UpdateAsync(userId, user);
            return _mapper.Map<UserResponseDto>(user);
        }
    }
}