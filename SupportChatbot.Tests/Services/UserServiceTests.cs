using AutoMapper;
using Moq;
using SupportChatbot.API.DTOs;
using SupportChatbot.API.Interfaces;
using SupportChatbot.API.Models;
using SupportChatbot.API.Services;
using Xunit;

namespace SupportChatbot.Tests.Services
{
    public class UserServiceTests
    {
        private readonly Mock<IRepository<Guid, User>> _userRepoMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly UserService _userService;

        public UserServiceTests()
        {
            _userRepoMock = new Mock<IRepository<Guid, User>>();
            _mapperMock = new Mock<IMapper>();
            _userService = new UserService(_userRepoMock.Object, _mapperMock.Object);
        }

        [Fact]
        public async Task DeleteUserAsync_ShouldThrow_WhenUserIdIsEmpty()
        {
            // Arrange
            var userId = Guid.Empty;

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _userService.DeleteUserAsync(userId));
        }

        [Fact]
        public async Task DeleteUserAsync_ShouldThrow_WhenUserNotFound()
        {
            // Arrange
            var userId = Guid.NewGuid();
            _userRepoMock.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync((User)null!);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _userService.DeleteUserAsync(userId));
        }

        [Fact]
        public async Task DeleteUserAsync_ShouldMarkUserAsDeleted()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var user = new User { Id = userId, IsDeleted = false };
            _userRepoMock.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(user);
            _userRepoMock.Setup(r => r.UpdateAsync(user));

            // Act
            var result = await _userService.DeleteUserAsync(userId);

            // Assert
            Assert.True(result);
            Assert.True(user.IsDeleted);
            _userRepoMock.Verify(r => r.UpdateAsync(user), Times.Once);
        }

        [Fact]
        public async Task RegisterUserAsync_ShouldThrow_WhenDtoIsNull()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => _userService.RegisterUserAsync(null!));
        }

        [Fact]
        public async Task RegisterUserAsync_ShouldThrow_WhenActiveUserExists()
        {
            var dto = new RegisterUserDto
            {
                Email = "test@example.com",
                Username = "testuser",
                Password = "password123"
            };

            var existingUsers = new List<User>
            {
                new User { Email = dto.Email, IsDeleted = false }
            };

            _userRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(existingUsers);

            await Assert.ThrowsAsync<InvalidOperationException>(() => _userService.RegisterUserAsync(dto));
        }

        [Fact]
        public async Task RegisterUserAsync_ShouldReactivateDeletedUser()
        {
            var dto = new RegisterUserDto
            {
                Email = "test@example.com",
                Username = "testuser",
                Password = "password123",
                ProfilePictureUrl = "pic.png",
                Role = "User"
            };

            var deletedUser = new User { Id = Guid.NewGuid(), Email = dto.Email, IsDeleted = true };
            var users = new List<User> { deletedUser };

            _userRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(users);
            _userRepoMock.Setup(r => r.UpdateAsync(deletedUser.Id, It.IsAny<User>()));

            var expectedDto = new UserResponseDto { Id = deletedUser.Id, Email = dto.Email, Username = dto.Username };
            _mapperMock.Setup(m => m.Map<UserResponseDto>(It.IsAny<User>())).Returns(expectedDto);

            var result = await _userService.RegisterUserAsync(dto);

            Assert.Equal(expectedDto.Id, result.Id);
            Assert.Equal(dto.Email, result.Email);
        }

        [Fact]
        public async Task RegisterUserAsync_ShouldAddNewUser_WhenNoExistingUserFound()
        {
            var dto = new RegisterUserDto
            {
                Email = "new@example.com",
                Username = "newuser",
                Password = "newpass",
                Role = "User"
            };

            _userRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<User>());
            _userRepoMock.Setup(r => r.AddAsync(It.IsAny<User>()));

            _mapperMock.Setup(m => m.Map<UserResponseDto>(It.IsAny<User>()))
                       .Returns(new UserResponseDto
                       {
                           Id = Guid.NewGuid(),
                           Email = dto.Email,
                           Username = dto.Username
                       });

            var result = await _userService.RegisterUserAsync(dto);

            Assert.Equal(dto.Email, result.Email);
            Assert.Equal(dto.Username, result.Username);
        }

        [Fact]
        public async Task UpdateUserAsync_ShouldThrow_WhenUserIdEmpty()
        {
            await Assert.ThrowsAsync<ArgumentException>(() => _userService.UpdateUserAsync(Guid.Empty, new UpdateUserDto()));
        }

        [Fact]
        public async Task UpdateUserAsync_ShouldThrow_WhenDtoIsNull()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => _userService.UpdateUserAsync(Guid.NewGuid(), null!));
        }

        [Fact]
        public async Task UpdateUserAsync_ShouldThrow_WhenUserNotFound()
        {
            var id = Guid.NewGuid();
            _userRepoMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((User)null!);

            await Assert.ThrowsAsync<KeyNotFoundException>(() => _userService.UpdateUserAsync(id, new UpdateUserDto()));
        }

        [Fact]
        public async Task UpdateUserAsync_ShouldUpdateAndReturnUser()
        {
            var id = Guid.NewGuid();
            var user = new User { Id = id, Username = "Old", Email = "old@example.com" };
            var dto = new UpdateUserDto
            {
                Username = "NewName",
                Password = "newpass",
                ProfilePictureUrl = "newpic.png"
            };

            _userRepoMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(user);
            _userRepoMock.Setup(r => r.UpdateAsync(id, It.IsAny<User>()));

            _mapperMock.Setup(m => m.Map<UserResponseDto>(It.IsAny<User>()))
                       .Returns(new UserResponseDto
                       {
                           Id = id,
                           Username = dto.Username!,
                           Email = user.Email,
                           ProfilePictureUrl = dto.ProfilePictureUrl!
                       });

            var result = await _userService.UpdateUserAsync(id, dto);

            Assert.Equal(dto.Username, result?.Username);
            Assert.Equal(user.Email, result?.Email);
        }
    }
}
