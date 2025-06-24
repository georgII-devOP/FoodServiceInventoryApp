using Xunit;
using Microsoft.EntityFrameworkCore;
using FoodServiceInventoryApp.Models;
using FoodServiceInventoryApp.Services;
using System.Linq;
using System.Threading.Tasks;

namespace FoodServiceInventoryApp.Tests
{
    public class FoodServiceDbContextTests : IDisposable
    {
        private readonly FoodServiceDbContext _context;

        public FoodServiceDbContextTests()
        {
            var options = new DbContextOptionsBuilder<FoodServiceDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new FoodServiceDbContext(options);

            _context.Database.EnsureCreated();
        }

        [Fact]
        public async Task Database_CanBeCreated()
        {
            Assert.NotNull(_context);

            var product = new Product { ProductName = "Test Product", Quantity = 10, UnitOfMeasure = "кг", UnitPrice = 100.0m, CategoryId = 1 };
            _context.Products.Add(product);
            await _context.SaveChangesAsync();
            Assert.True(product.ProductId > 0);
        }

        [Fact]
        public void OnModelCreating_SeedsCategories()
        {
            var categories = _context.Categories.ToList();

            Assert.NotNull(categories);
            Assert.Equal(5, categories.Count);

            Assert.Contains(categories, c => c.CategoryName == "Продукты питания" && c.CategoryId == 1);
            Assert.Contains(categories, c => c.CategoryName == "Посуда и однораз. упаковка" && c.CategoryId == 2);
            Assert.Contains(categories, c => c.CategoryName == "Оборудование и инвентарь" && c.CategoryId == 3);
            Assert.Contains(categories, c => c.CategoryName == "Расходные материалы для бизнеса" && c.CategoryId == 4);
            Assert.Contains(categories, c => c.CategoryName == "Чистящие и дезинфицирующие средства" && c.CategoryId == 5);
        }

        [Fact]
        public void EntityMappings_AreCorrectlyConfigured()
        {
            Assert.NotNull(_context.Users);
            Assert.NotNull(_context.Categories);
            Assert.NotNull(_context.Suppliers);
            Assert.NotNull(_context.Products);
            Assert.NotNull(_context.ProductSupplyHistories);
        }

        [Fact]
        public async Task Product_CanBeAddedAndRetrieved()
        {
            var newProduct = new Product
            {
                ProductName = "Сок",
                Quantity = 50,
                UnitOfMeasure = "л",
                UnitPrice = 120.0m,
                CategoryId = 1,
                LastSupplyDate = DateTime.Now
            };

            _context.Products.Add(newProduct);
            await _context.SaveChangesAsync();

            Assert.True(newProduct.ProductId > 0);

            var retrievedProduct = await _context.Products.FindAsync(newProduct.ProductId);
            Assert.NotNull(retrievedProduct);
            Assert.Equal("Сок", retrievedProduct.ProductName);
            Assert.Equal(50, retrievedProduct.Quantity);
        }

        [Fact]
        public async Task User_CanBeAdded()
        {
            var newUser = new User
            {
                Username = "testuser",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
                FirstName = "Test",
                LastName = "User",
                Patronymic = "Testovich",
                Position = "Employee"
            };

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            Assert.True(newUser.UserId > 0);
            var retrievedUser = await _context.Users.FindAsync(newUser.UserId);
            Assert.NotNull(retrievedUser);
            Assert.Equal("testuser", retrievedUser.Username);
            Assert.Equal("Test", retrievedUser.FirstName);
            Assert.True(BCrypt.Net.BCrypt.Verify("password123", retrievedUser.PasswordHash));
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }
}