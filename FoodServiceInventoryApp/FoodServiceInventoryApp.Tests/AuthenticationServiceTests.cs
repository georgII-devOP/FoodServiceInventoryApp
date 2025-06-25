using Xunit;
using Microsoft.EntityFrameworkCore;
using FoodServiceInventoryApp.Services;
using FoodServiceInventoryApp.Models;
using System.Threading.Tasks;
using System;
using System.Linq;
using System.Collections.Generic;
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

        private async Task AddTestUserForAuthentication(string username, string password, string firstName = "Test", string lastName = "User", string patronymic = "", string position = "Test Position")
        {
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);
            var user = new User
            {
                FirstName = firstName,
                LastName = lastName,
                Patronymic = patronymic,
                Position = position,
                Username = username,
                PasswordHash = hashedPassword
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
        }

        [Fact]
        public async Task AuthenticateUserAsync_ReturnsTrue_ForValidCredentials()
        {
            string testUsername = "testuser";
            string testPassword = "securePassword123";
            await AddTestUserForAuthentication(testUsername, testPassword, "Иван", "Иванов", "Иванович", "Администратор");

            // Act
            bool result = await _sut.AuthenticateUserAsync(testUsername, testPassword);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task AuthenticateUserAsync_ReturnsFalse_ForNonExistentUser()
        {
            // Act
            bool result = await _sut.AuthenticateUserAsync("nonexistentuser", "anypassword");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task AuthenticateUserAsync_ReturnsFalse_ForIncorrectPassword()
        {
            string testUsername = "anotheruser";
            string correctPassword = "correctPassword";
            await AddTestUserForAuthentication(testUsername, correctPassword, "Петр", "Петров", "Петрович", "Менеджер");

            // Act
            bool result = await _sut.AuthenticateUserAsync(testUsername, "incorrectPassword");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task RegisterUserAsync_ReturnsNonNullUser()
        {
            // Act
            var newUser = await _sut.RegisterUserAsync("Новый", "Пользователь", "Новичок", "Работник", "newUser", "newSecurePassword");

            // Assert
            Assert.NotNull(newUser);
        }

        [Fact]
        public async Task RegisterUserAsync_AssignsUserId()
        {
            // Act
            var newUser = await _sut.RegisterUserAsync("Новый", "Пользователь", "Новичок", "Работник", "newUser", "newSecurePassword");

            // Assert
            Assert.True(newUser.UserId > 0);
        }

        [Fact]
        public async Task RegisterUserAsync_SavesUserToDatabase()
        {
            string username = "userToVerify";
            string password = "passwordForVerification";

            // Act
            await _sut.RegisterUserAsync("Тест", "Свойства", "Пользователь", "Должность", username, password);

            // Assert
            var userInDb = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            Assert.NotNull(userInDb);
        }

        [Fact]
        public async Task RegisterUserAsync_SavesUserWithCorrectFirstName()
        {
            string username = "userFirstName";
            string password = "passwordForFirstName";
            string expectedFirstName = "Ольга";

            // Act
            await _sut.RegisterUserAsync(expectedFirstName, "Фамилия", "", "Должность", username, password);

            // Assert
            var userInDb = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            Assert.Equal(expectedFirstName, userInDb.FirstName);
        }

        [Fact]
        public async Task RegisterUserAsync_SavesUserWithCorrectLastName()
        {
            string username = "userLastName";
            string password = "passwordForLastName";
            string expectedLastName = "Смирнова";

            // Act
            await _sut.RegisterUserAsync("Имя", expectedLastName, "", "Должность", username, password);

            // Assert
            var userInDb = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            Assert.Equal(expectedLastName, userInDb.LastName);
        }

        [Fact]
        public async Task RegisterUserAsync_SavesUserWithCorrectPatronymic()
        {
            string username = "userPatronymic";
            string password = "passwordForPatronymic";
            string expectedPatronymic = "Александровна";

            // Act
            await _sut.RegisterUserAsync("Имя", "Фамилия", expectedPatronymic, "Должность", username, password);

            // Assert
            var userInDb = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            Assert.Equal(expectedPatronymic, userInDb.Patronymic);
        }

        [Fact]
        public async Task RegisterUserAsync_SavesUserWithCorrectPosition()
        {
            string username = "userPosition";
            string password = "passwordForPosition";
            string expectedPosition = "Руководитель";

            // Act
            await _sut.RegisterUserAsync("Имя", "Фамилия", "", expectedPosition, username, password);

            // Assert
            var userInDb = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            Assert.Equal(expectedPosition, userInDb.Position);
        }

        [Fact]
        public async Task RegisterUserAsync_SavesUserWithCorrectUsername()
        {
            string username = "expectedUsername";
            string password = "passwordForUsername";

            // Act
            await _sut.RegisterUserAsync("Имя", "Фамилия", "", "Должность", username, password);

            // Assert
            var userInDb = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            Assert.Equal(username, userInDb.Username);
        }

        [Fact]
        public async Task RegisterUserAsync_HashesPasswordBeforeSaving()
        {
            string username = "hashedUser";
            string password = "passwordToHash";

            // Act
            var newUser = await _sut.RegisterUserAsync("Тест", "Хеш", "", "Тестировщик", username, password);

            // Assert
            Assert.True(BCrypt.Net.BCrypt.Verify(password, newUser.PasswordHash));
        }
    }
}