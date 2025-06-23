using FoodServiceInventoryApp.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FoodServiceInventoryApp.Services
{
    public interface IUserService
    {
        Task<User> GetUserByUsernameAsync(string username);
        Task<IEnumerable<User>> GetAllUsersAsync();
        Task AddUserAsync(User user);
        Task UpdateUserAsync(User user);
        Task DeleteUserAsync(int userId);
        Task<bool> VerifyPasswordAsync(string username, string password);
    }
}