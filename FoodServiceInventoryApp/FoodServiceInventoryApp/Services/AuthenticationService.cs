using Microsoft.EntityFrameworkCore;
using FoodServiceInventoryApp.Models;
using System.Threading.Tasks;
using BCrypt.Net;

namespace FoodServiceInventoryApp.Services
{
    public class AuthenticationService
    {
        private readonly FoodServiceDbContext _dbContext;

        public AuthenticationService(FoodServiceDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<bool> AuthenticateUserAsync(string username, string password)
        {
            var user = await _dbContext.Users
                                     .FirstOrDefaultAsync(u => u.Username == username);

            if (user == null)
            {
                return false;
            }

            bool isPasswordCorrect = BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);

            return isPasswordCorrect;
        }

        public async Task<User> RegisterUserAsync(string firstName, string lastName, string patronymic, string position, string username, string password)
        {
            if (await _dbContext.Users.AnyAsync(u => u.Username == username))
            {
                throw new InvalidOperationException("Пользователь с таким именем уже существует.");
            }

            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);

            var newUser = new User
            {
                FirstName = firstName,
                LastName = lastName,
                Patronymic = patronymic,
                Position = position,
                Username = username,
                PasswordHash = hashedPassword
            };

            _dbContext.Users.Add(newUser);
            await _dbContext.SaveChangesAsync();
            return newUser;
        }
    }
}