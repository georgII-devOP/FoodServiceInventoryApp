using Xunit;
using Microsoft.EntityFrameworkCore;
using FoodServiceInventoryApp.Services;
using FoodServiceInventoryApp.Models;
using System.Threading.Tasks;
using System;
using System.Linq;
using BCrypt.Net;

namespace FoodServiceInventoryApp.Tests
{
    public class AuthenticationServiceTests : IDisposable
    {
        private FoodServiceDbContext _context;
        private AuthenticationService _sut;

        public AuthenticationServiceTests()
        {
            var options = new DbContextOptionsBuilder<FoodServiceDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new FoodServiceDbContext(options);
            _context.Database.EnsureDeleted();
            _context.Database.EnsureCreated();

            _sut = new AuthenticationService(_context);
        }

        public void Dispose()
        {
            _context.Dispose();
        }


        [Fact]
        public async Task AuthenticateUserAsync_ReturnsTrue_ForValidCredentials()
        {
            string testPassword = "securePassword123";
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(testPassword);
            var user = new User
            {
                FirstName = "Иван",
                LastName = "Иванов",
                Patronymic = "Иванович",
                Position = "Администратор",
                Username = "testuser",
                PasswordHash = hashedPassword
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            bool result = await _sut.AuthenticateUserAsync("testuser", testPassword);

            Assert.True(result);
        }

        [Fact]
        public async Task AuthenticateUserAsync_ReturnsFalse_ForNonExistentUser()
        {
            bool result = await _sut.AuthenticateUserAsync("nonexistentuser", "anypassword");

            Assert.False(result);
        }

        [Fact]
        public async Task AuthenticateUserAsync_ReturnsFalse_ForIncorrectPassword()
        {
            string correctPassword = "correctPassword";
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(correctPassword);
            var user = new User
            {
                FirstName = "Петр",
                LastName = "Петров",
                Patronymic = "Петрович",
                Position = "Менеджер",
                Username = "anotheruser",
                PasswordHash = hashedPassword
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            bool result = await _sut.AuthenticateUserAsync("anotheruser", "incorrectPassword");

            Assert.False(result);
        }


        [Fact]
        public async Task RegisterUserAsync_AddsNewUserToDatabase()
        {
            string username = "newUser";
            string password = "newSecurePassword";

            var newUser = await _sut.RegisterUserAsync("Новый", "Пользователь", "Новичок", "Работник", username, password);

            Assert.NotNull(newUser);
            Assert.True(newUser.UserId > 0);
            Assert.Equal(username, newUser.Username);

            var userInDb = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            Assert.NotNull(userInDb);
            Assert.Equal("Новый", userInDb.FirstName);
            Assert.True(BCrypt.Net.BCrypt.Verify(password, userInDb.PasswordHash));
        }

        [Fact]
        public async Task RegisterUserAsync_ThrowsException_WhenUserAlreadyExists()
        {
            string existingUsername = "existingUser";
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword("somepass");
            _context.Users.Add(new User
            {
                FirstName = "Старый",
                LastName = "Пользователь",
                Patronymic = "",
                Position = "Кассир",
                Username = existingUsername,
                PasswordHash = hashedPassword
            });
            await _context.SaveChangesAsync();

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _sut.RegisterUserAsync("Другой", "Пользователь", "", "Грузчик", existingUsername, "anotherPassword")
            );

            Assert.Contains("Пользователь с таким именем уже существует.", exception.Message);

            var userCount = await _context.Users.CountAsync();
            Assert.Equal(1, userCount);
        }

        [Fact]
        public async Task RegisterUserAsync_HashesPasswordBeforeSaving()
        {
            string username = "hashedUser";
            string password = "passwordToHash";

            var newUser = await _sut.RegisterUserAsync("Тест", "Хеш", "", "Тестировщик", username, password);

            Assert.NotNull(newUser.PasswordHash);
            Assert.NotEqual(password, newUser.PasswordHash);
            Assert.True(BCrypt.Net.BCrypt.Verify(password, newUser.PasswordHash));
        }
    }
}