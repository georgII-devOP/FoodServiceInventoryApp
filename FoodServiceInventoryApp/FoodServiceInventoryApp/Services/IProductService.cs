using FoodServiceInventoryApp.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FoodServiceInventoryApp.Services
{
    public interface IProductService
    {
        Task<IEnumerable<Product>> GetAllProductsAsync();
        Task<Product> GetProductByIdAsync(int id);
        Task<Product> AddProductAsync(Product product);
        Task UpdateProductAsync(Product product);
        Task DeleteProductAsync(int id);
        Task<bool> ProductExistsAsync(int id);
        Task<bool> ProductExistsByNameAsync(string productName);
        Task UpdateProductQuantityAsync(int productId, decimal quantityChange);
    }
}