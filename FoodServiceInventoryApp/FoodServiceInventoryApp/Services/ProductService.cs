using FoodServiceInventoryApp.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FoodServiceInventoryApp.Services
{
    public class ProductService : IProductService
    {
        private readonly FoodServiceDbContext _context;

        public ProductService(FoodServiceDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Product>> GetAllProductsAsync()
        {
            return await _context.Products
                                 .Include(p => p.Category)
                                 .OrderBy(p => p.ProductName)
                                 .ToListAsync();
        }

        public async Task<Product> GetProductByIdAsync(int id)
        {
            return await _context.Products
                                 .Include(p => p.Category)
                                 .FirstOrDefaultAsync(p => p.ProductId == id);
        }

        public async Task<Product> AddProductAsync(Product product)
        {
            _context.Products.Add(product);
            await _context.SaveChangesAsync();
            return product;
        }

        public async Task UpdateProductAsync(Product product)
        {
            _context.Entry(product).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteProductAsync(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                _context.Products.Remove(product);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> ProductExistsAsync(int id)
        {
            return await _context.Products.AnyAsync(p => p.ProductId == id);
        }

        public async Task<bool> ProductExistsByNameAsync(string productName)
        {
            return await _context.Products.AnyAsync(p => p.ProductName.ToLower() == productName.ToLower());
        }

        public async Task UpdateProductQuantityAsync(int productId, decimal quantityChange)
        {
            var product = await _context.Products.FindAsync(productId);
            if (product != null)
            {
                product.Quantity += quantityChange;
                if (product.Quantity < 0) product.Quantity = 0;
                await _context.SaveChangesAsync();
            }
        }
        public async Task<Product> GetProductByNameAsync(string productName)
        {
            return await _context.Products.FirstOrDefaultAsync(p => p.ProductName.ToLower() == productName.ToLower());
        }
    }
}