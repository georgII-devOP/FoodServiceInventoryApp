using FoodServiceInventoryApp.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FoodServiceInventoryApp.Services
{
    public interface ICategoryService
    {
        Task<IEnumerable<Category>> GetAllCategoriesAsync();
        Task<Category> GetCategoryByIdAsync(int id);
        Task<Category> AddCategoryAsync(Category category);
        Task UpdateCategoryAsync(Category category);
        Task DeleteCategoryAsync(int id);
        Task<bool> CategoryExistsAsync(int id);
        Task<Category> GetCategoryByNameAsync(string name);
    }
}