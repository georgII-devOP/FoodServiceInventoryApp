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
    public class ProductSupplyHistoryServiceTests : IDisposable
    {
        private TestFoodServiceDbContext _context;
        private ProductSupplyHistoryService _sut;

        private int _foodCategoryId;
        private int _drinkCategoryId;
        private int _supplierAId;
        private int _supplierBId;
        private int _productMilkId;
        private int _productBreadId;

        public ProductSupplyHistoryServiceTests()
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

            var supplierA = new Supplier { CompanyName = "Поставщик А", ContactPerson = "Иван", Phone = "111-222", Email = "a@example.com" };
            var supplierB = new Supplier { CompanyName = "Поставщик Б", ContactPerson = "Петр", Phone = "333-444", Email = "b@example.com" };
            _context.Suppliers.AddRange(supplierA, supplierB);
            _context.SaveChanges();
            _supplierAId = supplierA.SupplierId;
            _supplierBId = supplierB.SupplierId;

            var productMilk = new Product
            {
                ProductId = 1,
                ProductName = "Молоко",
                Quantity = 100,
                UnitOfMeasure = "л",
                CategoryId = _drinkCategoryId,
                UnitPrice = 1.0m,
                LastSupplyDate = DateTime.Now.AddDays(-10)
            };
            var productBread = new Product
            {
                ProductId = 2,
                ProductName = "Хлеб",
                Quantity = 50,
                UnitOfMeasure = "буханок",
                CategoryId = _foodCategoryId,
                UnitPrice = 0.8m,
                LastSupplyDate = DateTime.Now.AddDays(-5)
            };
            _context.Products.AddRange(productMilk, productBread);
            _context.SaveChanges();
            _productMilkId = productMilk.ProductId;
            _productBreadId = productBread.ProductId;

            _context.ProductSupplyHistories.AddRange(
                new ProductSupplyHistory
                {
                    SupplyRecordId = 1,
                    ProductId = _productMilkId,
                    SupplierId = _supplierAId,
                    SuppliedQuantity = 20,
                    SupplyUnitPrice = 0.9m,
                    SupplyDate = DateTime.Parse("2024-06-01")
                },
                new ProductSupplyHistory
                {
                    SupplyRecordId = 2,
                    ProductId = _productBreadId,
                    SupplierId = _supplierBId,
                    SuppliedQuantity = 10,
                    SupplyUnitPrice = 0.7m,
                    SupplyDate = DateTime.Parse("2024-06-05")
                },
                new ProductSupplyHistory
                {
                    SupplyRecordId = 3,
                    ProductId = _productMilkId,
                    SupplierId = _supplierAId,
                    SuppliedQuantity = 30,
                    SupplyUnitPrice = 0.95m,
                    SupplyDate = DateTime.Parse("2024-06-10")
                },
                 new ProductSupplyHistory
                 {
                     SupplyRecordId = 4,
                     ProductId = _productBreadId,
                     SupplierId = _supplierAId,
                     SuppliedQuantity = 15,
                     SupplyUnitPrice = 0.75m,
                     SupplyDate = DateTime.Parse("2024-06-15")
                 }
            );
            _context.SaveChanges();

            _sut = new ProductSupplyHistoryService(_context);
        }

        public void Dispose()
        {
            _context.Dispose();
        }

        [Fact]
        public async Task GetAllSupplyRecordsAsync_ReturnsAllRecordsWithProductAndSupplier()
        {
            var records = await _sut.GetAllSupplyRecordsAsync();

            Assert.NotNull(records);
            Assert.Equal(4, records.Count());

            foreach (var record in records)
            {
                Assert.NotNull(record.Product);
                Assert.NotNull(record.Supplier);
            }

            var record1 = records.FirstOrDefault(r => r.SupplyRecordId == 1);
            Assert.NotNull(record1);
            Assert.Equal("Молоко", record1.Product.ProductName);
            Assert.Equal("Поставщик А", record1.Supplier.CompanyName);
        }

        [Fact]
        public async Task GetSupplyRecordByIdAsync_ReturnsRecordWithProductAndSupplier_WhenFound()
        {
            var record = await _sut.GetSupplyRecordByIdAsync(2);

            Assert.NotNull(record);
            Assert.Equal(2, record.SupplyRecordId);
            Assert.Equal("Хлеб", record.Product.ProductName);
            Assert.Equal("Поставщик Б", record.Supplier.CompanyName);
        }

        [Fact]
        public async Task GetSupplyRecordByIdAsync_ReturnsNull_WhenNotFound()
        {
            var record = await _sut.GetSupplyRecordByIdAsync(999);

            Assert.Null(record);
        }

        [Fact]
        public async Task AddSupplyRecordAsync_AddsNewRecordToDatabase()
        {
            var newRecord = new ProductSupplyHistory
            {
                ProductId = _productMilkId,
                SupplierId = _supplierBId,
                SuppliedQuantity = 15,
                SupplyUnitPrice = 1.1m,
                SupplyDate = DateTime.Parse("2024-06-20")
            };
            var initialCount = await _context.ProductSupplyHistories.CountAsync();

            await _sut.AddSupplyRecordAsync(newRecord);

            Assert.True(newRecord.SupplyRecordId > 0);
            var recordInDb = await _context.ProductSupplyHistories
                                           .Include(psh => psh.Product)
                                           .Include(psh => psh.Supplier)
                                           .FirstOrDefaultAsync(psh => psh.SupplyRecordId == newRecord.SupplyRecordId);
            Assert.NotNull(recordInDb);
            Assert.Equal(initialCount + 1, await _context.ProductSupplyHistories.CountAsync());
            Assert.Equal("Молоко", recordInDb.Product.ProductName);
            Assert.Equal("Поставщик Б", recordInDb.Supplier.CompanyName);
            Assert.Equal(15, recordInDb.SuppliedQuantity);
        }

        [Fact]
        public async Task UpdateSupplyRecordAsync_UpdatesExistingRecord()
        {
            var recordToUpdate = await _sut.GetSupplyRecordByIdAsync(1);
            Assert.NotNull(recordToUpdate);
            recordToUpdate.SuppliedQuantity = 25;
            recordToUpdate.SupplyUnitPrice = 0.92m;
            recordToUpdate.SupplyDate = DateTime.Parse("2024-06-02");
            recordToUpdate.SupplierId = _supplierBId;

            await _sut.UpdateSupplyRecordAsync(recordToUpdate);

            var updatedRecord = await _sut.GetSupplyRecordByIdAsync(1);
            Assert.NotNull(updatedRecord);
            Assert.Equal(25, updatedRecord.SuppliedQuantity);
            Assert.Equal(0.92m, updatedRecord.SupplyUnitPrice);
            Assert.Equal(DateTime.Parse("2024-06-02"), updatedRecord.SupplyDate);
            Assert.Equal("Поставщик Б", updatedRecord.Supplier.CompanyName);
        }

        [Fact]
        public async Task UpdateSupplyRecordAsync_ThrowsException_ForNonExistentRecord()
        {
            var nonExistentRecord = new ProductSupplyHistory
            {
                SupplyRecordId = 999,
                ProductId = _productMilkId,
                SupplierId = _supplierAId,
                SuppliedQuantity = 10,
                SupplyUnitPrice = 1.0m,
                SupplyDate = DateTime.Now
            };

            await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() =>
                _sut.UpdateSupplyRecordAsync(nonExistentRecord)
            );

            var recordAdded = await _context.ProductSupplyHistories.AnyAsync(r => r.SupplyRecordId == nonExistentRecord.SupplyRecordId);
            Assert.False(recordAdded);
        }

        [Fact]
        public async Task DeleteSupplyRecordAsync_RemovesRecord_WhenFound()
        {
            var initialCount = await _context.ProductSupplyHistories.CountAsync();

            await _sut.DeleteSupplyRecordAsync(3);

            var recordInDb = await _sut.GetSupplyRecordByIdAsync(3);
            Assert.Null(recordInDb);
            var finalCount = await _context.ProductSupplyHistories.CountAsync();
            Assert.Equal(initialCount - 1, finalCount);
        }

        [Fact]
        public async Task DeleteSupplyRecordAsync_DoesNothing_WhenNotFound()
        {
            var initialCount = await _context.ProductSupplyHistories.CountAsync();

            await _sut.DeleteSupplyRecordAsync(999);

            var finalCount = await _context.ProductSupplyHistories.CountAsync();
            Assert.Equal(initialCount, finalCount);
        }

        [Fact]
        public async Task GetSupplyRecordsByDateRangeAsync_ReturnsRecordsWithinRange()
        {
            var startDate = DateTime.Parse("2024-06-04");
            var endDate = DateTime.Parse("2024-06-11");

            var records = await _sut.GetSupplyRecordsByDateRangeAsync(startDate, endDate);

            Assert.NotNull(records);
            Assert.Equal(2, records.Count());

            Assert.Contains(records, r => r.SupplyRecordId == 2);
            Assert.Contains(records, r => r.SupplyRecordId == 3);
            Assert.DoesNotContain(records, r => r.SupplyRecordId == 1);
            Assert.DoesNotContain(records, r => r.SupplyRecordId == 4);
        }

        [Fact]
        public async Task GetSupplyRecordsByDateRangeAsync_ReturnsEmptyList_WhenNoRecordsInDateRange()
        {
            var startDate = DateTime.Parse("2025-01-01");
            var endDate = DateTime.Parse("2025-01-31");

            var records = await _sut.GetSupplyRecordsByDateRangeAsync(startDate, endDate);

            Assert.NotNull(records);
            Assert.Empty(records);
        }

        [Fact]
        public async Task GetSupplyRecordsFilteredAsync_FiltersByStartDate()
        {
            var startDate = DateTime.Parse("2024-06-10");

            var records = await _sut.GetSupplyRecordsFilteredAsync(startDate: startDate);

            Assert.NotNull(records);
            Assert.Equal(2, records.Count());
            Assert.Contains(records, r => r.SupplyRecordId == 3);
            Assert.Contains(records, r => r.SupplyRecordId == 4);
            Assert.Equal(4, records.First().SupplyRecordId);
        }

        [Fact]
        public async Task GetSupplyRecordsFilteredAsync_FiltersByEndDate()
        {
            var endDate = DateTime.Parse("2024-06-05");

            var records = await _sut.GetSupplyRecordsFilteredAsync(endDate: endDate);

            Assert.NotNull(records);
            Assert.Equal(2, records.Count());
            Assert.Contains(records, r => r.SupplyRecordId == 1);
            Assert.Contains(records, r => r.SupplyRecordId == 2);
            Assert.Equal(2, records.First().SupplyRecordId);
        }

        [Fact]
        public async Task GetSupplyRecordsFilteredAsync_FiltersByDateRange()
        {
            var startDate = DateTime.Parse("2024-06-05");
            var endDate = DateTime.Parse("2024-06-10");

            var records = await _sut.GetSupplyRecordsFilteredAsync(startDate: startDate, endDate: endDate);

            Assert.NotNull(records);
            Assert.Equal(2, records.Count());
            Assert.Contains(records, r => r.SupplyRecordId == 2);
            Assert.Contains(records, r => r.SupplyRecordId == 3);
            Assert.Equal(3, records.First().SupplyRecordId);
        }

        [Fact]
        public async Task GetSupplyRecordsFilteredAsync_FiltersByProductId()
        {
            var records = await _sut.GetSupplyRecordsFilteredAsync(productId: _productMilkId);

            Assert.NotNull(records);
            Assert.Equal(2, records.Count());
            Assert.Contains(records, r => r.SupplyRecordId == 1);
            Assert.Contains(records, r => r.SupplyRecordId == 3);
            Assert.Equal(3, records.First().SupplyRecordId);
        }

        [Fact]
        public async Task GetSupplyRecordsFilteredAsync_FiltersBySupplierId()
        {
            var records = await _sut.GetSupplyRecordsFilteredAsync(supplierId: _supplierAId);

            Assert.NotNull(records);
            Assert.Equal(3, records.Count());
            Assert.Contains(records, r => r.SupplyRecordId == 1);
            Assert.Contains(records, r => r.SupplyRecordId == 3);
            Assert.Contains(records, r => r.SupplyRecordId == 4);
            Assert.Equal(4, records.First().SupplyRecordId);
        }

        [Fact]
        public async Task GetSupplyRecordsFilteredAsync_FiltersByCategoryId()
        {
            var records = await _sut.GetSupplyRecordsFilteredAsync(categoryId: _foodCategoryId);

            Assert.NotNull(records);
            Assert.Equal(2, records.Count());
            Assert.Contains(records, r => r.SupplyRecordId == 2);
            Assert.Contains(records, r => r.SupplyRecordId == 4);
            Assert.Equal(4, records.First().SupplyRecordId);
        }

        [Fact]
        public async Task GetSupplyRecordsFilteredAsync_FiltersByMultipleCriteria()
        {
            var startDate = DateTime.Parse("2024-06-01");
            var endDate = DateTime.Parse("2024-06-10");

            var records = await _sut.GetSupplyRecordsFilteredAsync(
                startDate: startDate,
                endDate: endDate,
                productId: _productMilkId,
                supplierId: _supplierAId,
                categoryId: _drinkCategoryId
            );

            Assert.NotNull(records);
            Assert.Equal(2, records.Count());
            Assert.Contains(records, r => r.SupplyRecordId == 1);
            Assert.Contains(records, r => r.SupplyRecordId == 3);
            Assert.Equal(3, records.First().SupplyRecordId);
        }

        [Fact]
        public async Task GetSupplyRecordsFilteredAsync_ReturnsEmptyList_WhenNoRecordsMatchCriteria()
        {
            var records = await _sut.GetSupplyRecordsFilteredAsync(
                startDate: DateTime.Parse("2025-01-01"),
                productId: _productMilkId,
                supplierId: _supplierAId
            );

            Assert.NotNull(records);
            Assert.Empty(records);
        }

        [Fact]
        public async Task GetSupplyRecordsFilteredAsync_HandlesZeroProductIdAndSupplierId()
        {
            var expectedCount = await _context.ProductSupplyHistories.CountAsync();

            var records = await _sut.GetSupplyRecordsFilteredAsync(
                productId: 0,
                supplierId: 0,
                categoryId: 0
            );

            Assert.NotNull(records);
            Assert.Equal(expectedCount, records.Count());
        }
    }
}