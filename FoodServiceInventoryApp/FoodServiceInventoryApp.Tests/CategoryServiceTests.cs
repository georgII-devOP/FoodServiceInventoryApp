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
    public class CategoryServiceTests : IDisposable
    {
        private TestFoodServiceDbContext _context;
        private CategoryService _sut;

        public CategoryServiceTests()
        {
            var options = new DbContextOptionsBuilder<FoodServiceDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new TestFoodServiceDbContext(options);
            _context.Database.EnsureDeleted();
            _context.Database.EnsureCreated();

            _context.Categories.AddRange(
                new Category { CategoryName = "Напитки" },
                new Category { CategoryName = "Закуски" },
                new Category { CategoryName = "Десерты" }
            );
            _context.SaveChanges();

            _sut = new CategoryService(_context);
        }

        public void Dispose()
        {
            _context.Dispose();
        }

        [Fact]
        public async Task GetAllCategoriesAsync_ReturnsAllCategoriesOrderedByName()
        {
            var categories = await _sut.GetAllCategoriesAsync();

            Assert.NotNull(categories);
            Assert.Equal(3, categories.Count());
            Assert.Equal("Десерты", categories.ElementAt(0).CategoryName);
            Assert.Equal("Закуски", categories.ElementAt(1).CategoryName);
            Assert.Equal("Напитки", categories.ElementAt(2).CategoryName);
        }

        [Fact]
        public async Task GetCategoryByIdAsync_ReturnsCategory_WhenFound()
        {
            var expectedCategory = await _context.Categories.FirstOrDefaultAsync(c => c.CategoryName == "Закуски");
            Assert.NotNull(expectedCategory);

            var category = await _sut.GetCategoryByIdAsync(expectedCategory.CategoryId);

            Assert.NotNull(category);
            Assert.Equal("Закуски", category.CategoryName);
            Assert.Equal(expectedCategory.CategoryId, category.CategoryId);
        }

        [Fact]
        public async Task GetCategoryByIdAsync_ReturnsNull_WhenNotFound()
        {
            var category = await _sut.GetCategoryByIdAsync(9999);
            Assert.Null(category);
        }

        [Fact]
        public async Task AddCategoryAsync_AddsNewCategoryToDatabase()
        {
            var newCategory = new Category { CategoryName = "Новая Категория" };

            var addedCategory = await _sut.AddCategoryAsync(newCategory);

            Assert.NotNull(addedCategory);
            Assert.True(addedCategory.CategoryId > 0);
            Assert.Equal("Новая Категория", addedCategory.CategoryName);

            var categoryInDb = await _context.Categories.FirstOrDefaultAsync(c => c.CategoryName == "Новая Категория");
            Assert.NotNull(categoryInDb);
            Assert.Equal(addedCategory.CategoryId, categoryInDb.CategoryId);
        }

        [Fact]
        public async Task UpdateCategoryAsync_UpdatesExistingCategory()
        {
            var categoryToUpdate = await _context.Categories.FirstOrDefaultAsync(c => c.CategoryName == "Напитки");
            Assert.NotNull(categoryToUpdate);

            categoryToUpdate.CategoryName = "Обновленные Напитки";

            await _sut.UpdateCategoryAsync(categoryToUpdate);

            var updatedCategory = await _sut.GetCategoryByIdAsync(categoryToUpdate.CategoryId);
            Assert.NotNull(updatedCategory);
            Assert.Equal("Обновленные Напитки", updatedCategory.CategoryName);
        }

        [Fact]
        public async Task UpdateCategoryAsync_ThrowsException_ForNonExistentCategory()
        {
            var nonExistentCategory = new Category { CategoryId = 9999, CategoryName = "Несуществующая" };

            await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() =>
                _sut.UpdateCategoryAsync(nonExistentCategory)
            );

            var categoryAdded = await _context.Categories.AnyAsync(c => c.CategoryId == nonExistentCategory.CategoryId);
            Assert.False(categoryAdded);
        }

        [Fact]
        public async Task DeleteCategoryAsync_RemovesCategory_WhenFound()
        {
            var categoryToDelete = await _context.Categories.FirstOrDefaultAsync(c => c.CategoryName == "Десерты");
            Assert.NotNull(categoryToDelete);

            var initialCount = await _context.Categories.CountAsync();

            await _sut.DeleteCategoryAsync(categoryToDelete.CategoryId);

            var categoryInDb = await _sut.GetCategoryByIdAsync(categoryToDelete.CategoryId);
            Assert.Null(categoryInDb);
            var finalCount = await _context.Categories.CountAsync();
            Assert.Equal(initialCount - 1, finalCount);
        }

        [Fact]
        public async Task DeleteCategoryAsync_DoesNothing_WhenNotFound()
        {
            var initialCount = await _context.Categories.CountAsync();

            await _sut.DeleteCategoryAsync(9999);

            var finalCount = await _context.Categories.CountAsync();
            Assert.Equal(initialCount, finalCount);
        }

        [Fact]
        public async Task CategoryExistsAsync_ReturnsTrue_WhenCategoryExists()
        {
            var categoryToCheck = await _context.Categories.FirstOrDefaultAsync(c => c.CategoryName == "Напитки");
            Assert.NotNull(categoryToCheck);

            bool exists = await _sut.CategoryExistsAsync(categoryToCheck.CategoryId);

            Assert.True(exists);
        }

        [Fact]
        public async Task CategoryExistsAsync_ReturnsFalse_WhenCategoryDoesNotExist()
        {
            bool exists = await _sut.CategoryExistsAsync(9999);

            Assert.False(exists);
        }

        [Fact]
        public async Task GetCategoryByNameAsync_ReturnsCategory_WhenFoundCaseInsensitive()
        {
            var category = await _sut.GetCategoryByNameAsync("Напитки");
            var categoryLower = await _sut.GetCategoryByNameAsync("напитки");
            var categoryUpper = await _sut.GetCategoryByNameAsync("НАПИТКИ");

            Assert.NotNull(category);
            Assert.Equal("Напитки", category.CategoryName);
            Assert.NotNull(categoryLower);
            Assert.Equal("Напитки", categoryLower.CategoryName);
            Assert.NotNull(categoryUpper);
            Assert.Equal("Напитки", categoryUpper.CategoryName);
        }

        [Fact]
        public async Task GetCategoryByNameAsync_ReturnsNull_WhenNotFound()
        {
            var category = await _sut.GetCategoryByNameAsync("Несуществующая Категория");

            Assert.Null(category);
        }
    }
}