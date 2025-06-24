using Xunit;
using Microsoft.EntityFrameworkCore;
using FoodServiceInventoryApp.Services;
using FoodServiceInventoryApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FoodServiceInventoryApp.Tests
{
    public class ProductServiceTests : IDisposable
    {
        private TestFoodServiceDbContext _context;
        private ProductService _sut;

        private int _foodCategoryId;
        private int _drinkCategoryId;

        public ProductServiceTests()
        {
            var options = new DbContextOptionsBuilder<FoodServiceDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new TestFoodServiceDbContext(options);
            _context.Database.EnsureDeleted();
            _context.Database.EnsureCreated();

            var foodCategory = new Category { CategoryName = "Продукты питания" };
            var drinkCategory = new Category { CategoryName = "Напитки" };
            _context.Categories.AddRange(foodCategory, drinkCategory);
            _context.SaveChanges();

            _foodCategoryId = foodCategory.CategoryId;
            _drinkCategoryId = drinkCategory.CategoryId;

            _context.Products.AddRange(
                new Product { ProductId = 1, ProductName = "Яблоки", Quantity = 100, UnitOfMeasure = "кг", CategoryId = _foodCategoryId, UnitPrice = 1.5m, LastSupplyDate = DateTime.Now.AddDays(-7) },
                new Product { ProductId = 2, ProductName = "Молоко", Quantity = 50, UnitOfMeasure = "л", CategoryId = _drinkCategoryId, UnitPrice = 1.0m, LastSupplyDate = DateTime.Now.AddDays(-5) },
                new Product { ProductId = 3, ProductName = "Хлеб", Quantity = 20, UnitOfMeasure = "буханок", CategoryId = _foodCategoryId, UnitPrice = 2.0m, LastSupplyDate = DateTime.Now.AddDays(-1) }
            );
            _context.SaveChanges();

            _sut = new ProductService(_context);
        }

        public void Dispose()
        {
            _context.Dispose();
        }

        [Fact]
        public async Task GetAllProductsAsync_ReturnsAllProductsWithCategoriesOrderedByName()
        {
            var products = await _sut.GetAllProductsAsync();

            Assert.NotNull(products);
            Assert.Equal(3, products.Count());

            Assert.Equal("Молоко", products.ElementAt(0).ProductName);
            Assert.Equal("Хлеб", products.ElementAt(1).ProductName);
            Assert.Equal("Яблоки", products.ElementAt(2).ProductName);

            Assert.NotNull(products.ElementAt(0).Category);
            Assert.NotNull(products.ElementAt(1).Category);
            Assert.NotNull(products.ElementAt(2).Category);
            Assert.Equal("Напитки", products.ElementAt(0).Category.CategoryName);
            Assert.Equal("Продукты питания", products.ElementAt(1).Category.CategoryName);
            Assert.Equal("Продукты питания", products.ElementAt(2).Category.CategoryName);
        }

        [Fact]
        public async Task GetProductByIdAsync_ReturnsProductWithCategory_WhenFound()
        {
            var product = await _sut.GetProductByIdAsync(2);

            Assert.NotNull(product);
            Assert.Equal("Молоко", product.ProductName);
            Assert.Equal(2, product.ProductId);
            Assert.NotNull(product.Category);
            Assert.Equal("Напитки", product.Category.CategoryName);
        }

        [Fact]
        public async Task GetProductByIdAsync_ReturnsNull_WhenNotFound()
        {
            var product = await _sut.GetProductByIdAsync(99);

            Assert.Null(product);
        }

        [Fact]
        public async Task AddProductAsync_AddsNewProductToDatabase()
        {
            var newProduct = new Product
            {
                ProductName = "Сок",
                Quantity = 30,
                UnitOfMeasure = "л",
                CategoryId = _drinkCategoryId,
                UnitPrice = 0.8m,
                LastSupplyDate = DateTime.Now
            };

            var addedProduct = await _sut.AddProductAsync(newProduct);

            Assert.NotNull(addedProduct);
            Assert.True(addedProduct.ProductId > 0);
            Assert.Equal("Сок", addedProduct.ProductName);

            var productInDb = await _context.Products
                                            .Include(p => p.Category)
                                            .FirstOrDefaultAsync(p => p.ProductName == "Сок");
            Assert.NotNull(productInDb);
            Assert.Equal(addedProduct.ProductId, productInDb.ProductId);
            Assert.NotNull(productInDb.Category);
            Assert.Equal("Напитки", productInDb.Category.CategoryName);
        }

        [Fact]
        public async Task UpdateProductAsync_UpdatesExistingProduct()
        {
            var productToUpdate = await _sut.GetProductByIdAsync(1);
            Assert.NotNull(productToUpdate);
            productToUpdate.ProductName = "Красные Яблоки";
            productToUpdate.Quantity = 120;
            productToUpdate.CategoryId = _drinkCategoryId;

            await _sut.UpdateProductAsync(productToUpdate);

            var updatedProduct = await _sut.GetProductByIdAsync(1);
            Assert.NotNull(updatedProduct);
            Assert.Equal("Красные Яблоки", updatedProduct.ProductName);
            Assert.Equal(120, updatedProduct.Quantity);
            Assert.Equal(_drinkCategoryId, updatedProduct.CategoryId);
            Assert.Equal("Напитки", updatedProduct.Category.CategoryName);
        }

        [Fact]
        public async Task UpdateProductAsync_ThrowsException_ForNonExistentProduct()
        {
            var nonExistentProduct = new Product
            {
                ProductId = 999,
                ProductName = "Несуществующий",
                Quantity = 1,
                UnitOfMeasure = "шт",
                CategoryId = _foodCategoryId,
                UnitPrice = 0.1m, 
                LastSupplyDate = DateTime.Now
            };

            await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() =>
                _sut.UpdateProductAsync(nonExistentProduct)
            );

            var productAdded = await _context.Products.AnyAsync(p => p.ProductId == nonExistentProduct.ProductId);
            Assert.False(productAdded);
        }

        [Fact]
        public async Task DeleteProductAsync_RemovesProduct_WhenFound()
        {
            var initialCount = await _context.Products.CountAsync();

            await _sut.DeleteProductAsync(3);

            var productInDb = await _sut.GetProductByIdAsync(3);
            Assert.Null(productInDb);
            var finalCount = await _context.Products.CountAsync();
            Assert.Equal(initialCount - 1, finalCount);
        }

        [Fact]
        public async Task DeleteProductAsync_DoesNothing_WhenNotFound()
        {
            var initialCount = await _context.Products.CountAsync();

            await _sut.DeleteProductAsync(99);

            var finalCount = await _context.Products.CountAsync();
            Assert.Equal(initialCount, finalCount);
        }

        [Fact]
        public async Task ProductExistsAsync_ReturnsTrue_WhenProductExists()
        {
            bool exists = await _sut.ProductExistsAsync(1);

            Assert.True(exists);
        }

        [Fact]
        public async Task ProductExistsAsync_ReturnsFalse_WhenProductDoesNotExist()
        {
            bool exists = await _sut.ProductExistsAsync(99);

            Assert.False(exists);
        }

        [Fact]
        public async Task ProductExistsByNameAsync_ReturnsTrue_WhenProductExistsCaseInsensitive()
        {
            bool exists = await _sut.ProductExistsByNameAsync("Яблоки");
            bool existsLower = await _sut.ProductExistsByNameAsync("яблоки");
            bool existsUpper = await _sut.ProductExistsByNameAsync("ЯБЛОКИ");

            Assert.True(exists);
            Assert.True(existsLower);
            Assert.True(existsUpper);
        }

        [Fact]
        public async Task ProductExistsByNameAsync_ReturnsFalse_WhenProductDoesNotExist()
        {
            bool exists = await _sut.ProductExistsByNameAsync("Несуществующий Продукт");

            Assert.False(exists);
        }

        [Theory]
        [InlineData(1, 50, 150)]
        [InlineData(1, -20, 80)]
        [InlineData(1, -150, 0)]
        public async Task UpdateProductQuantityAsync_UpdatesQuantityCorrectly(int productId, decimal quantityChange, decimal expectedQuantity)
        {
            var initialProduct = await _context.Products.FindAsync(productId);
            Assert.NotNull(initialProduct);

            await _sut.UpdateProductQuantityAsync(productId, quantityChange);

            var updatedProduct = await _context.Products.FindAsync(productId);
            Assert.NotNull(updatedProduct);
            Assert.Equal(expectedQuantity, updatedProduct.Quantity);
        }

        [Fact]
        public async Task UpdateProductQuantityAsync_DoesNothing_WhenProductNotFound()
        {
            var initialCount = await _context.Products.CountAsync();

            await _sut.UpdateProductQuantityAsync(999, 10);

            var finalCount = await _context.Products.CountAsync();
            Assert.Equal(initialCount, finalCount);
        }
    }
}