using CommunityToolkit.Mvvm.Input;
using FoodServiceInventoryApp.Models;
using FoodServiceInventoryApp.Services;
using FoodServiceInventoryApp.ViewModels;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace FoodServiceInventoryApp.Tests
{
    public class PurchasePlanVMTests
    {
        private readonly Mock<ISupplierService> _mockSupplierService;
        private readonly Mock<IProductSupplyHistoryService> _mockProductSupplyHistoryService;
        private readonly Mock<IProductService> _mockProductService;

        private readonly PurchasePlanVM _sut;

        public PurchasePlanVMTests()
        {
            _mockSupplierService = new Mock<ISupplierService>();
            _mockProductSupplyHistoryService = new Mock<IProductSupplyHistoryService>();
            _mockProductService = new Mock<IProductService>();

            _mockSupplierService.Setup(s => s.GetAllSuppliersAsync())
                                .ReturnsAsync(new List<Supplier>
                                {
                                    new Supplier { SupplierId = 1, CompanyName = "Поставщик А" },
                                    new Supplier { SupplierId = 2, CompanyName = "Поставщик Б" }
                                });

            _sut = new PurchasePlanVM(
                _mockSupplierService.Object,
                _mockProductSupplyHistoryService.Object,
                _mockProductService.Object
            );
        }

        [Fact]
        public async Task LoadSuppliersCommand_ShouldPopulateSuppliersCorrectly()
        {
            _mockSupplierService.Invocations.Clear();

            var testSuppliers = new List<Supplier>
            {
                new Supplier { SupplierId = 10, CompanyName = "Тестовый Поставщик 1" },
                new Supplier { SupplierId = 20, CompanyName = "Тестовый Поставщик 2" }
            };
            _mockSupplierService.Setup(s => s.GetAllSuppliersAsync()).ReturnsAsync(testSuppliers);

            await _sut.LoadSuppliersCommand.ExecuteAsync(null);

            _mockSupplierService.Verify(s => s.GetAllSuppliersAsync(), Times.Once);
            Assert.Equal(testSuppliers.Count + 1, _sut.Suppliers.Count);
            Assert.Equal("Все", _sut.Suppliers.First().CompanyName);
            Assert.Contains(_sut.Suppliers, s => s.CompanyName == "Тестовый Поставщик 1");
            Assert.Contains(_sut.Suppliers, s => s.CompanyName == "Тестовый Поставщик 2");
            Assert.Equal(_sut.Suppliers.First(), _sut.SelectedSupplierFilter);
        }

        [Fact]
        public async Task LoadSuppliersCommand_ShouldHandleEmptySuppliersList()
        {
            _mockSupplierService.Invocations.Clear();
            _mockSupplierService.Setup(s => s.GetAllSuppliersAsync()).ReturnsAsync(new List<Supplier>());

            await _sut.LoadSuppliersCommand.ExecuteAsync(null);

            _mockSupplierService.Verify(s => s.GetAllSuppliersAsync(), Times.Once);
            Assert.Single(_sut.Suppliers);
            Assert.Equal("Все", _sut.Suppliers.First().CompanyName);
            Assert.Equal(_sut.Suppliers.First(), _sut.SelectedSupplierFilter);
        }

        [Fact]
        public async Task LoadSuppliersCommand_ShouldHandleNullSuppliersResult()
        {
            _mockSupplierService.Invocations.Clear();
            _mockSupplierService.Setup(s => s.GetAllSuppliersAsync()).ReturnsAsync((List<Supplier>)null);

            await _sut.LoadSuppliersCommand.ExecuteAsync(null);

            _mockSupplierService.Verify(s => s.GetAllSuppliersAsync(), Times.Once);
            Assert.Single(_sut.Suppliers);
            Assert.Equal("Все", _sut.Suppliers.First().CompanyName);
            Assert.Equal(_sut.Suppliers.First(), _sut.SelectedSupplierFilter);
        }

        [Fact]
        public async Task GeneratePlanCommand_ShouldGeneratePlanWithMultipleItems()
        {
            var now = DateTime.Now;
            var startDate = now.Date.AddMonths(-1);
            var endDate = now.Date.AddDays(1).AddTicks(-1);

            _mockProductSupplyHistoryService.Setup(s => s.GetSupplyRecordsFilteredAsync(
                It.Is<DateTime>(d => d.Date == startDate.Date),
                It.Is<DateTime>(d => d.Date == endDate.Date),
                null, null, null))
                .ReturnsAsync(new List<ProductSupplyHistory>
                {
                    new ProductSupplyHistory { SupplyRecordId = 1, ProductId = 1, SupplierId = 1, SuppliedQuantity = 10, SupplyUnitPrice = 5m, SupplyDate = now.AddDays(-10) },
                    new ProductSupplyHistory { SupplyRecordId = 2, ProductId = 2, SupplierId = 1, SuppliedQuantity = 20, SupplyUnitPrice = 2.5m, SupplyDate = now.AddDays(-5) },
                    new ProductSupplyHistory { SupplyRecordId = 3, ProductId = 1, SupplierId = 2, SuppliedQuantity = 5, SupplyUnitPrice = 6m, SupplyDate = now.AddDays(-20) }
                });

            _mockProductService.Setup(s => s.GetAllProductsAsync())
                .ReturnsAsync(new List<Product>
                {
                    new Product { ProductId = 1, ProductName = "Мука", Quantity = 3, UnitOfMeasure = "кг" },
                    new Product { ProductId = 2, ProductName = "Сахар", Quantity = 15, UnitOfMeasure = "кг" }
                });

            _mockProductService.Setup(s => s.GetProductByIdAsync(1)).ReturnsAsync(new Product { ProductId = 1, ProductName = "Мука", Quantity = 3, UnitOfMeasure = "кг" });
            _mockProductService.Setup(s => s.GetProductByIdAsync(2)).ReturnsAsync(new Product { ProductId = 2, ProductName = "Сахар", Quantity = 15, UnitOfMeasure = "кг" });
            _mockSupplierService.Setup(s => s.GetSupplierByIdAsync(1)).ReturnsAsync(new Supplier { SupplierId = 1, CompanyName = "Поставщик А" });
            _mockSupplierService.Setup(s => s.GetSupplierByIdAsync(2)).ReturnsAsync(new Supplier { SupplierId = 2, CompanyName = "Поставщик Б" });

            _sut.SelectedSupplierFilter = _sut.Suppliers.First(s => s.SupplierId == 0);

            await ((IAsyncRelayCommand)_sut.GeneratePlanCommand).ExecuteAsync(null);

            Assert.Equal(3, _sut.PurchasePlanItems.Count);

            var flourFromA = _sut.PurchasePlanItems.FirstOrDefault(p => p.ProductName == "Мука" && p.SupplierName == "Поставщик А");
            Assert.NotNull(flourFromA);
            Assert.Equal(12m, flourFromA.RecommendedQuantity);
            Assert.Equal(12m * 5m, flourFromA.EstimatedCost);

            var sugarFromA = _sut.PurchasePlanItems.FirstOrDefault(p => p.ProductName == "Сахар" && p.SupplierName == "Поставщик А");
            Assert.NotNull(sugarFromA);
            Assert.Equal(15m, sugarFromA.RecommendedQuantity);
            Assert.Equal(15m * 2.5m, sugarFromA.EstimatedCost);

            var flourFromB = _sut.PurchasePlanItems.FirstOrDefault(p => p.ProductName == "Мука" && p.SupplierName == "Поставщик Б");
            Assert.NotNull(flourFromB);
            Assert.Equal(4.5m, flourFromB.RecommendedQuantity);
            Assert.Equal(4.5m * 6m, flourFromB.EstimatedCost);


            Assert.Empty(_sut.ErrorMessage);
        }

        [Fact]
        public async Task GeneratePlanCommand_ShouldSetErrorMessage_WhenNoSupplyHistoryFound()
        {
            _mockProductSupplyHistoryService.Setup(s => s.GetSupplyRecordsFilteredAsync(
                It.IsAny<DateTime>(), It.IsAny<DateTime>(), null, null, null))
                .ReturnsAsync(new List<ProductSupplyHistory>()); // Пустая история

            await ((IAsyncRelayCommand)_sut.GeneratePlanCommand).ExecuteAsync(null);

            Assert.Empty(_sut.PurchasePlanItems);
            Assert.False(string.IsNullOrEmpty(_sut.ErrorMessage));
            Assert.Contains("Данные о прошлых поставках для расчета плана не найдены.", _sut.ErrorMessage);
            _mockProductSupplyHistoryService.Verify(s => s.GetSupplyRecordsFilteredAsync(
                It.IsAny<DateTime>(), It.IsAny<DateTime>(), null, null, null), Times.Once);
            _mockProductService.Verify(s => s.GetAllProductsAsync(), Times.Never);
        }

        [Fact]
        public async Task GeneratePlanCommand_ShouldSetErrorMessage_WhenProductStockNotFound()
        {
            _mockProductSupplyHistoryService.Setup(s => s.GetSupplyRecordsFilteredAsync(
                It.IsAny<DateTime>(), It.IsAny<DateTime>(), null, null, null))
                .ReturnsAsync(new List<ProductSupplyHistory>
                {
                    new ProductSupplyHistory { SupplyRecordId = 1, ProductId = 1, SupplierId = 1, SuppliedQuantity = 10, SupplyUnitPrice = 5m, SupplyDate = DateTime.Now }
                });

            _mockProductService.Setup(s => s.GetAllProductsAsync())
                .ReturnsAsync(new List<Product>());

            await ((IAsyncRelayCommand)_sut.GeneratePlanCommand).ExecuteAsync(null);

            Assert.Empty(_sut.PurchasePlanItems);
            Assert.False(string.IsNullOrEmpty(_sut.ErrorMessage));
            Assert.Contains("Данные о текущих остатках продуктов не найдены.", _sut.ErrorMessage);
            _mockProductSupplyHistoryService.Verify(s => s.GetSupplyRecordsFilteredAsync(
                It.IsAny<DateTime>(), It.IsAny<DateTime>(), null, null, null), Times.Once);
            _mockProductService.Verify(s => s.GetAllProductsAsync(), Times.Once);
        }

        [Fact]
        public async Task GeneratePlanCommand_ShouldFilterBySelectedSupplier()
        {
            var now = DateTime.Now;
            var startDate = now.Date.AddMonths(-1);
            var endDate = now.Date.AddDays(1).AddTicks(-1);

            _mockProductSupplyHistoryService.Setup(s => s.GetSupplyRecordsFilteredAsync(
                It.Is<DateTime>(d => d.Date == startDate.Date),
                It.Is<DateTime>(d => d.Date == endDate.Date),
                null, null, null))
                .ReturnsAsync(new List<ProductSupplyHistory>
                {
                    new ProductSupplyHistory { SupplyRecordId = 1, ProductId = 1, SupplierId = 1, SuppliedQuantity = 10, SupplyUnitPrice = 5m, SupplyDate = now.AddDays(-10) },
                    new ProductSupplyHistory { SupplyRecordId = 2, ProductId = 2, SupplierId = 2, SuppliedQuantity = 20, SupplyUnitPrice = 2.5m, SupplyDate = now.AddDays(-5) }
                });

            _mockProductService.Setup(s => s.GetAllProductsAsync())
                .ReturnsAsync(new List<Product>
                {
                    new Product { ProductId = 1, ProductName = "Мука", Quantity = 3, UnitOfMeasure = "кг" },
                    new Product { ProductId = 2, ProductName = "Сахар", Quantity = 15, UnitOfMeasure = "кг" }
                });

            _mockProductService.Setup(s => s.GetProductByIdAsync(1)).ReturnsAsync(new Product { ProductId = 1, ProductName = "Мука", Quantity = 3, UnitOfMeasure = "кг" });
            _mockProductService.Setup(s => s.GetProductByIdAsync(2)).ReturnsAsync(new Product { ProductId = 2, ProductName = "Сахар", Quantity = 15, UnitOfMeasure = "кг" });
            _mockSupplierService.Setup(s => s.GetSupplierByIdAsync(1)).ReturnsAsync(new Supplier { SupplierId = 1, CompanyName = "Поставщик А" });
            _mockSupplierService.Setup(s => s.GetSupplierByIdAsync(2)).ReturnsAsync(new Supplier { SupplierId = 2, CompanyName = "Поставщик Б" });

            _sut.Suppliers.Clear();
            _sut.Suppliers.Add(new Supplier { SupplierId = 0, CompanyName = "Все" });
            _sut.Suppliers.Add(new Supplier { SupplierId = 1, CompanyName = "Поставщик А" });
            _sut.Suppliers.Add(new Supplier { SupplierId = 2, CompanyName = "Поставщик Б" });
            _sut.SelectedSupplierFilter = _sut.Suppliers.First(s => s.SupplierId == 1);

            await ((IAsyncRelayCommand)_sut.GeneratePlanCommand).ExecuteAsync(null);

            Assert.Equal(1, _sut.PurchasePlanItems.Count);
            Assert.Equal("Поставщик А", _sut.PurchasePlanItems.First().SupplierName);
            Assert.Equal("Мука", _sut.PurchasePlanItems.First().ProductName);
            Assert.Equal(12m, _sut.PurchasePlanItems.First().RecommendedQuantity);
            Assert.Empty(_sut.ErrorMessage);
        }

        [Theory]
        [InlineData(10, 3, 12.0)]
        [InlineData(10, 10, 10.0)]
        [InlineData(10, 15, 10.0)]
        [InlineData(0.5, 0.1, 1.0)]
        [InlineData(0.0, 5, 0.0)]
        [InlineData(0.0, 0.0, 0.0)]
        public async Task GeneratePlanCommand_ShouldCalculateRecommendedQuantityCorrectly(decimal totalSuppliedQuantity, decimal currentProductQuantity, decimal expectedRecommendedQuantity)
        {
            var now = DateTime.Now;
            var startDate = now.Date.AddMonths(-1);
            var endDate = now.Date.AddDays(1).AddTicks(-1);

            _mockProductSupplyHistoryService.Setup(s => s.GetSupplyRecordsFilteredAsync(
                It.IsAny<DateTime>(), It.IsAny<DateTime>(), null, null, null))
                .ReturnsAsync(new List<ProductSupplyHistory>
                {
                    new ProductSupplyHistory { SupplyRecordId = 1, ProductId = 1, SupplierId = 1, SuppliedQuantity = totalSuppliedQuantity, SupplyUnitPrice = 10m, SupplyDate = now.AddDays(-10) }
                });

            _mockProductService.Setup(s => s.GetAllProductsAsync())
                .ReturnsAsync(new List<Product>
                {
                    new Product { ProductId = 1, ProductName = "Тестовый Продукт", Quantity = currentProductQuantity, UnitOfMeasure = "шт." }
                });

            _mockProductService.Setup(s => s.GetProductByIdAsync(1)).ReturnsAsync(new Product { ProductId = 1, ProductName = "Тестовый Продукт", Quantity = currentProductQuantity, UnitOfMeasure = "шт." });
            _mockSupplierService.Setup(s => s.GetSupplierByIdAsync(1)).ReturnsAsync(new Supplier { SupplierId = 1, CompanyName = "Тестовый Поставщик" });

            _sut.SelectedSupplierFilter = _sut.Suppliers.First(s => s.SupplierId == 0);

            await ((IAsyncRelayCommand)_sut.GeneratePlanCommand).ExecuteAsync(null);

            if (expectedRecommendedQuantity > 0)
            {
                Assert.Single(_sut.PurchasePlanItems);
                Assert.Equal(expectedRecommendedQuantity, _sut.PurchasePlanItems.First().RecommendedQuantity);
            }
            else
            {
                Assert.Empty(_sut.PurchasePlanItems);
                Assert.Contains("План закупок для выбранных критериев не сформирован.", _sut.ErrorMessage);
            }
        }

        [Fact]
        public async Task GeneratePlanCommand_ShouldHandleMissingProductOrSupplierDetails()
        {
            var now = DateTime.Now;
            var startDate = now.Date.AddMonths(-1);
            var endDate = now.Date.AddDays(1).AddTicks(-1);

            _mockProductSupplyHistoryService.Setup(s => s.GetSupplyRecordsFilteredAsync(
                It.IsAny<DateTime>(), It.IsAny<DateTime>(), null, null, null))
                .ReturnsAsync(new List<ProductSupplyHistory>
                {
                    new ProductSupplyHistory { SupplyRecordId = 1, ProductId = 1, SupplierId = 1, SuppliedQuantity = 10, SupplyUnitPrice = 5m, SupplyDate = now.AddDays(-10) },
                    new ProductSupplyHistory { SupplyRecordId = 2, ProductId = 99, SupplierId = 2, SuppliedQuantity = 5, SupplyUnitPrice = 6m, SupplyDate = now.AddDays(-5) },
                    new ProductSupplyHistory { SupplyRecordId = 3, ProductId = 1, SupplierId = 99, SuppliedQuantity = 2, SupplyUnitPrice = 7m, SupplyDate = now.AddDays(-15) }
                });

            _mockProductService.Setup(s => s.GetAllProductsAsync())
                .ReturnsAsync(new List<Product>
                {
                    new Product { ProductId = 1, ProductName = "Существующий Продукт", Quantity = 3, UnitOfMeasure = "кг" },
                });

            _mockProductService.Setup(s => s.GetProductByIdAsync(1)).ReturnsAsync(new Product { ProductId = 1, ProductName = "Существующий Продукт", Quantity = 3, UnitOfMeasure = "кг" });
            _mockProductService.Setup(s => s.GetProductByIdAsync(99)).ReturnsAsync((Product)null);
            _mockSupplierService.Setup(s => s.GetSupplierByIdAsync(1)).ReturnsAsync(new Supplier { SupplierId = 1, CompanyName = "Существующий Поставщик" });
            _mockSupplierService.Setup(s => s.GetSupplierByIdAsync(99)).ReturnsAsync((Supplier)null);

            _sut.SelectedSupplierFilter = _sut.Suppliers.First(s => s.SupplierId == 0);

            await ((IAsyncRelayCommand)_sut.GeneratePlanCommand).ExecuteAsync(null);

            Assert.Single(_sut.PurchasePlanItems);
            Assert.Equal("Существующий Продукт", _sut.PurchasePlanItems.First().ProductName);
            Assert.Equal("Существующий Поставщик", _sut.PurchasePlanItems.First().SupplierName);

            Assert.Equal(12m, _sut.PurchasePlanItems.First().RecommendedQuantity);
            Assert.Empty(_sut.ErrorMessage);
        }
    }
}