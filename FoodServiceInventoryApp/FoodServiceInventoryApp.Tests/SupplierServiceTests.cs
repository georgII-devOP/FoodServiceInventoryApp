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
    public class SupplierServiceTests : IDisposable
    {
        private TestFoodServiceDbContext _context;
        private SupplierService _sut;

        public SupplierServiceTests()
        {
            var options = new DbContextOptionsBuilder<FoodServiceDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new TestFoodServiceDbContext(options);
            _context.Database.EnsureDeleted();
            _context.Database.EnsureCreated();

            _context.Suppliers.AddRange(
                new Supplier { SupplierId = 1, CompanyName = "ABC Supplies", ContactPerson = "Джон Доу", Phone = "123-456-7890", Email = "john@abc.com" },
                new Supplier { SupplierId = 2, CompanyName = "Global Distributors", ContactPerson = "Джейн Смит", Phone = "987-654-3210", Email = "jane@global.com" },
                new Supplier { SupplierId = 3, CompanyName = "Local Foods Inc.", ContactPerson = "Мария Иванова", Phone = "555-111-2222", Email = "maria@local.com" }
            );
            _context.SaveChanges();

            _sut = new SupplierService(_context);
        }

        public void Dispose()
        {
            _context.Dispose();
        }

        [Fact]
        public async Task GetAllSuppliersAsync_ReturnsAllSuppliersOrderedByName()
        {
            var suppliers = await _sut.GetAllSuppliersAsync();

            Assert.NotNull(suppliers);
            Assert.Equal(3, suppliers.Count());

            Assert.Equal("ABC Supplies", suppliers.ElementAt(0).CompanyName);
            Assert.Equal("Global Distributors", suppliers.ElementAt(1).CompanyName);
            Assert.Equal("Local Foods Inc.", suppliers.ElementAt(2).CompanyName);
        }

        [Fact]
        public async Task GetSupplierByIdAsync_ReturnsSupplier_WhenFound()
        {
            var supplier = await _sut.GetSupplierByIdAsync(2);

            Assert.NotNull(supplier);
            Assert.Equal(2, supplier.SupplierId);
            Assert.Equal("Global Distributors", supplier.CompanyName);
            Assert.Equal("Джейн Смит", supplier.ContactPerson);
        }

        [Fact]
        public async Task GetSupplierByIdAsync_ReturnsNull_WhenNotFound()
        {
            var supplier = await _sut.GetSupplierByIdAsync(99);

            Assert.Null(supplier);
        }

        [Fact]
        public async Task AddSupplierAsync_AddsNewSupplierToDatabase()
        {
            var newSupplier = new Supplier
            {
                CompanyName = "New Fresh Co.",
                ContactPerson = "Елена Петрова",
                Phone = "111-222-3333",
                Email = "elena@newfresh.com"
            };
            var initialCount = await _context.Suppliers.CountAsync();

            var addedSupplier = await _sut.AddSupplierAsync(newSupplier);

            Assert.NotNull(addedSupplier);
            Assert.True(addedSupplier.SupplierId > 0);
            Assert.Equal("New Fresh Co.", addedSupplier.CompanyName);

            var supplierInDb = await _context.Suppliers.FirstOrDefaultAsync(s => s.CompanyName == "New Fresh Co.");
            Assert.NotNull(supplierInDb);
            Assert.Equal(addedSupplier.SupplierId, supplierInDb.SupplierId);
            Assert.Equal(initialCount + 1, await _context.Suppliers.CountAsync());
        }

        [Fact]
        public async Task UpdateSupplierAsync_UpdatesExistingSupplier()
        {
            var supplierToUpdate = await _sut.GetSupplierByIdAsync(1);
            Assert.NotNull(supplierToUpdate);
            supplierToUpdate.CompanyName = "ABC Premium Supplies";
            supplierToUpdate.Email = "support@abcpremium.com";

            await _sut.UpdateSupplierAsync(supplierToUpdate);

            var updatedSupplier = await _sut.GetSupplierByIdAsync(1);
            Assert.NotNull(updatedSupplier);
            Assert.Equal("ABC Premium Supplies", updatedSupplier.CompanyName);
            Assert.Equal("support@abcpremium.com", updatedSupplier.Email);
        }

        [Fact]
        public async Task UpdateSupplierAsync_ThrowsException_ForNonExistentSupplier()
        {
            var nonExistentSupplier = new Supplier
            {
                SupplierId = 999,
                CompanyName = "Несуществующий Поставщик",
                ContactPerson = "Тест",
                Phone = "123",
                Email = "test@test.com"
            };

            await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() =>
                _sut.UpdateSupplierAsync(nonExistentSupplier)
            );

            var supplierAdded = await _context.Suppliers.AnyAsync(s => s.SupplierId == nonExistentSupplier.SupplierId);
            Assert.False(supplierAdded);
        }

        [Fact]
        public async Task DeleteSupplierAsync_RemovesSupplier_WhenFound()
        {
            var initialCount = await _context.Suppliers.CountAsync();

            await _sut.DeleteSupplierAsync(3);

            var supplierInDb = await _sut.GetSupplierByIdAsync(3);
            Assert.Null(supplierInDb);
            var finalCount = await _context.Suppliers.CountAsync();
            Assert.Equal(initialCount - 1, finalCount);
        }

        [Fact]
        public async Task DeleteSupplierAsync_DoesNothing_WhenNotFound()
        {
            var initialCount = await _context.Suppliers.CountAsync();

            await _sut.DeleteSupplierAsync(99);

            var finalCount = await _context.Suppliers.CountAsync();
            Assert.Equal(initialCount, finalCount);
        }

        [Fact]
        public async Task SupplierExistsAsync_ReturnsTrue_WhenSupplierExists()
        {
            bool exists = await _sut.SupplierExistsAsync(1);

            Assert.True(exists);
        }

        [Fact]
        public async Task SupplierExistsAsync_ReturnsFalse_WhenSupplierDoesNotExist()
        {
            bool exists = await _sut.SupplierExistsAsync(99);

            Assert.False(exists);
        }

        [Fact]
        public async Task GetSupplierByNameAsync_ReturnsSupplier_WhenFoundCaseInsensitive()
        {
            var supplier = await _sut.GetSupplierByNameAsync("Abc Supplies");

            Assert.NotNull(supplier);
            Assert.Equal("ABC Supplies", supplier.CompanyName);
            Assert.Equal(1, supplier.SupplierId);
        }

        [Fact]
        public async Task GetSupplierByNameAsync_ReturnsNull_WhenNotFound()
        {
            var supplier = await _sut.GetSupplierByNameAsync("NonExistent Supplier");

            Assert.Null(supplier);
        }
    }
}