using FoodServiceInventoryApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoodServiceInventoryApp.Services
{
    public interface IAuthenticationService
    {
        Task<bool> AuthenticateUserAsync(string username, string password);
        Task<User> RegisterUserAsync(string firstName, string lastName, string patronymic, string position, string username, string password);
    }
}
