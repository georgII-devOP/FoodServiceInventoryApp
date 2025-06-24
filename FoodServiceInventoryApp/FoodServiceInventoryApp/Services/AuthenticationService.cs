using FoodServiceInventoryApp.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using BCrypt.Net;

namespace FoodServiceInventoryApp.Services
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly FoodServiceDbContext _context;

        public AuthenticationService(FoodServiceDbContext context)
        {
            _context = context;
        }

        public async Task<bool> AuthenticateUserAsync(string username, string password)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username.ToLower() == username.ToLower());
            if (user == null)
            {
                return false;
            }

            return BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);
        }

        public async Task<User> RegisterUserAsync(string firstName, string lastName, string patronymic, string position, string username, string password)
        {
            if (await _context.Users.AnyAsync(u => u.Username.ToLower() == username.ToLower()))
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

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();
            return newUser;
        }
    }
}