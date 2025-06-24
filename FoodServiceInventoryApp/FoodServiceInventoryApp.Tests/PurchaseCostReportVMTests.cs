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
    public class PurchaseCostReportVMTests
    {
        private readonly Mock<IProductSupplyHistoryService> _mockSupplyHistoryService;
        private readonly Mock<IProductService> _mockProductService;
        private readonly Mock<ICategoryService> _mockCategoryService;
        private readonly Mock<ISupplierService> _mockSupplierService;

        private readonly PurchaseCostReportVM _sut;

        public PurchaseCostReportVMTests()
        {
            _mockSupplyHistoryService = new Mock<IProductSupplyHistoryService>();
            _mockProductService = new Mock<IProductService>();
            _mockCategoryService = new Mock<ICategoryService>();
            _mockSupplierService = new Mock<ISupplierService>();

            _mockProductService.Setup(s => s.GetAllProductsAsync())
                               .ReturnsAsync(new List<Product>
                               {
                                   new Product { ProductId = 1, ProductName = "Продукт А" },
                                   new Product { ProductId = 2, ProductName = "Продукт Б" }
                               });
            _mockCategoryService.Setup(s => s.GetAllCategoriesAsync())
                                .ReturnsAsync(new List<Category>
                                {
                                    new Category { CategoryId = 1, CategoryName = "Категория X" },
                                    new Category { CategoryId = 2, CategoryName = "Категория Y" }
                                });
            _mockSupplierService.Setup(s => s.GetAllSuppliersAsync())
                                .ReturnsAsync(new List<Supplier>
                                {
                                    new Supplier { SupplierId = 1, CompanyName = "Поставщик 1" },
                                    new Supplier { SupplierId = 2, CompanyName = "Поставщик 2" }
                                });

            _sut = new PurchaseCostReportVM(
                _mockSupplyHistoryService.Object,
                _mockProductService.Object,
                _mockCategoryService.Object,
                _mockSupplierService.Object
            );
        }

        [Fact]
        public async Task LoadFiltersCommand_ShouldPopulateFiltersCorrectly()
        {
            _mockProductService.Invocations.Clear();
            _mockCategoryService.Invocations.Clear();
            _mockSupplierService.Invocations.Clear();

            var testProducts = new List<Product> { new Product { ProductId = 10, ProductName = "Тестовый Продукт" } };
            var testCategories = new List<Category> { new Category { CategoryId = 20, CategoryName = "Тестовая Категория" } };
            var testSuppliers = new List<Supplier> { new Supplier { SupplierId = 30, CompanyName = "Тестовый Поставщик" } };

            _mockProductService.Setup(s => s.GetAllProductsAsync()).ReturnsAsync(testProducts);
            _mockCategoryService.Setup(s => s.GetAllCategoriesAsync()).ReturnsAsync(testCategories);
            _mockSupplierService.Setup(s => s.GetAllSuppliersAsync()).ReturnsAsync(testSuppliers);

            await _sut.LoadFiltersCommand.ExecuteAsync(null);

            _mockProductService.Verify(s => s.GetAllProductsAsync(), Times.Once);
            _mockCategoryService.Verify(s => s.GetAllCategoriesAsync(), Times.Once);
            _mockSupplierService.Verify(s => s.GetAllSuppliersAsync(), Times.Once);

            Assert.Equal(testProducts.Count + 1, _sut.ProductsFilter.Count);
            Assert.Equal("Все", _sut.ProductsFilter.First().ProductName);
            Assert.Contains(_sut.ProductsFilter, p => p.ProductName == "Тестовый Продукт");
            Assert.Equal(_sut.ProductsFilter.First(), _sut.SelectedProductFilter);

            Assert.Equal(testSuppliers.Count + 1, _sut.SuppliersFilter.Count);
            Assert.Equal("Все", _sut.SuppliersFilter.First().CompanyName);
            Assert.Contains(_sut.SuppliersFilter, s => s.CompanyName == "Тестовый Поставщик");
            Assert.Equal(_sut.SuppliersFilter.First(), _sut.SelectedSupplierFilter);

            Assert.Equal(testCategories.Count + 1, _sut.CategoriesFilter.Count);
            Assert.Equal("Все", _sut.CategoriesFilter.First().CategoryName);
            Assert.Contains(_sut.CategoriesFilter, c => c.CategoryName == "Тестовая Категория");
            Assert.Equal(_sut.CategoriesFilter.First(), _sut.SelectedCategoryFilter);
        }

        [Fact]
        public async Task GenerateReportCommand_ShouldGenerateReportWithMultipleItemsAndCalculateTotalCost()
        {
            _sut.SelectedMonth = _sut.Months[DateTime.Now.Month - 1];
            _sut.SelectedYear = DateTime.Now.Year;

            var supplyRecords = new List<ProductSupplyHistory>
            {
                new ProductSupplyHistory { SupplyRecordId = 1, ProductId = 1, SupplierId = 1, SuppliedQuantity = 10, SupplyUnitPrice = 5m, SupplyDate = new DateTime(_sut.SelectedYear, DateTime.Now.Month, 15) },
                new ProductSupplyHistory { SupplyRecordId = 2, ProductId = 2, SupplierId = 2, SuppliedQuantity = 20, SupplyUnitPrice = 2.5m, SupplyDate = new DateTime(_sut.SelectedYear, DateTime.Now.Month, 20) }
            };
            _mockSupplyHistoryService.Setup(s => s.GetSupplyRecordsFilteredAsync(
                It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<int?>()))
                .ReturnsAsync(supplyRecords);

            _mockProductService.Setup(s => s.GetProductByIdAsync(1)).ReturnsAsync(new Product { ProductId = 1, ProductName = "Продукт Альфа" });
            _mockProductService.Setup(s => s.GetProductByIdAsync(2)).ReturnsAsync(new Product { ProductId = 2, ProductName = "Продукт Бета" });
            _mockSupplierService.Setup(s => s.GetSupplierByIdAsync(1)).ReturnsAsync(new Supplier { SupplierId = 1, CompanyName = "Поставщик Один" });
            _mockSupplierService.Setup(s => s.GetSupplierByIdAsync(2)).ReturnsAsync(new Supplier { SupplierId = 2, CompanyName = "Поставщик Два" });

            await _sut.GenerateReportCommand.ExecuteAsync(null);

            Assert.Equal(2, _sut.ReportDetails.Count);

            var item1 = _sut.ReportDetails.FirstOrDefault(item => item.ProductName == "Продукт Альфа");
            Assert.NotNull(item1);
            Assert.Equal(50m, item1.TotalItemCost);
            Assert.Equal("Поставщик Один", item1.SupplierName);

            var item2 = _sut.ReportDetails.FirstOrDefault(item => item.ProductName == "Продукт Бета");
            Assert.NotNull(item2);
            Assert.Equal(50m, item2.TotalItemCost);
            Assert.Equal("Поставщик Два", item2.SupplierName);

            Assert.Equal(100m, _sut.TotalCost);
            Assert.Equal(string.Empty, _sut.ErrorMessage);
        }

        [Fact]
        public async Task GenerateReportCommand_ShouldSetErrorMessage_WhenNoRecordsFound()
        {
            _sut.SelectedMonth = _sut.Months.Last();
            _sut.SelectedYear = DateTime.Now.Year;

            _mockSupplyHistoryService.Setup(s => s.GetSupplyRecordsFilteredAsync(
                It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<int?>()))
                .ReturnsAsync(new List<ProductSupplyHistory>());

            await _sut.GenerateReportCommand.ExecuteAsync(null);

            Assert.Empty(_sut.ReportDetails);
            Assert.Equal(0m, _sut.TotalCost);
            Assert.False(string.IsNullOrEmpty(_sut.ErrorMessage));
            Assert.Equal("Данные для выбранных критериев не найдены.", _sut.ErrorMessage);
        }

        [Fact]
        public async Task GenerateReportCommand_ShouldFilterBySelectedProduct()
        {
            _sut.SelectedMonth = _sut.Months[DateTime.Now.Month - 1];
            _sut.SelectedYear = DateTime.Now.Year;

            _sut.ProductsFilter.Clear();
            _sut.ProductsFilter.Add(new Product { ProductId = 0, ProductName = "Все" });
            _sut.ProductsFilter.Add(new Product { ProductId = 1, ProductName = "Продукт А" });
            _sut.SelectedProductFilter = _sut.ProductsFilter.First(p => p.ProductId == 1);

            var supplyRecords = new List<ProductSupplyHistory>
            {
                new ProductSupplyHistory { SupplyRecordId = 1, ProductId = 1, SupplierId = 1, SuppliedQuantity = 10, SupplyUnitPrice = 5m, SupplyDate = new DateTime(_sut.SelectedYear, DateTime.Now.Month, 15) },
                new ProductSupplyHistory { SupplyRecordId = 2, ProductId = 2, SupplierId = 2, SuppliedQuantity = 20, SupplyUnitPrice = 2.5m, SupplyDate = new DateTime(_sut.SelectedYear, DateTime.Now.Month, 20) }
            };
            _mockSupplyHistoryService.Setup(s => s.GetSupplyRecordsFilteredAsync(
                It.IsAny<DateTime>(), It.IsAny<DateTime>(), 1, It.IsAny<int?>(), It.IsAny<int?>()))
                .ReturnsAsync(supplyRecords.Where(s => s.ProductId == 1).ToList());

            _mockProductService.Setup(s => s.GetProductByIdAsync(1)).ReturnsAsync(new Product { ProductId = 1, ProductName = "Продукт А" });
            _mockSupplierService.Setup(s => s.GetSupplierByIdAsync(1)).ReturnsAsync(new Supplier { SupplierId = 1, CompanyName = "Поставщик Один" });

            await _sut.GenerateReportCommand.ExecuteAsync(null);

            Assert.Single(_sut.ReportDetails);
            Assert.Equal("Продукт А", _sut.ReportDetails.First().ProductName);
            Assert.Equal(50m, _sut.TotalCost);

            _mockSupplyHistoryService.Verify(s => s.GetSupplyRecordsFilteredAsync(
                It.IsAny<DateTime>(), It.IsAny<DateTime>(), 1, null, null), Times.Once);
        }

        [Fact]
        public async Task GenerateReportCommand_ShouldFilterBySelectedSupplier()
        {
            _sut.SelectedMonth = _sut.Months[DateTime.Now.Month - 1];
            _sut.SelectedYear = DateTime.Now.Year;

            _sut.SuppliersFilter.Clear();
            _sut.SuppliersFilter.Add(new Supplier { SupplierId = 0, CompanyName = "Все" });
            _sut.SuppliersFilter.Add(new Supplier { SupplierId = 1, CompanyName = "Поставщик Один" });
            _sut.SelectedSupplierFilter = _sut.SuppliersFilter.First(sup => sup.SupplierId == 1);

            var supplyRecords = new List<ProductSupplyHistory>
            {
                new ProductSupplyHistory { SupplyRecordId = 1, ProductId = 1, SupplierId = 1, SuppliedQuantity = 10, SupplyUnitPrice = 5m, SupplyDate = new DateTime(_sut.SelectedYear, DateTime.Now.Month, 15) },
                new ProductSupplyHistory { SupplyRecordId = 2, ProductId = 2, SupplierId = 2, SuppliedQuantity = 20, SupplyUnitPrice = 2.5m, SupplyDate = new DateTime(_sut.SelectedYear, DateTime.Now.Month, 20) }
            };
            _mockSupplyHistoryService.Setup(s => s.GetSupplyRecordsFilteredAsync(
                It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<int?>(), 1, It.IsAny<int?>()))
                .ReturnsAsync(supplyRecords.Where(s => s.SupplierId == 1).ToList());

            _mockProductService.Setup(s => s.GetProductByIdAsync(1)).ReturnsAsync(new Product { ProductId = 1, ProductName = "Продукт А" });
            _mockSupplierService.Setup(s => s.GetSupplierByIdAsync(1)).ReturnsAsync(new Supplier { SupplierId = 1, CompanyName = "Поставщик Один" });

            await _sut.GenerateReportCommand.ExecuteAsync(null);

            Assert.Single(_sut.ReportDetails);
            Assert.Equal("Поставщик Один", _sut.ReportDetails.First().SupplierName);
            Assert.Equal(50m, _sut.TotalCost);

            _mockSupplyHistoryService.Verify(s => s.GetSupplyRecordsFilteredAsync(
                It.IsAny<DateTime>(), It.IsAny<DateTime>(), null, 1, null), Times.Once);
        }

        [Fact]
        public async Task GenerateReportCommand_ShouldFilterBySelectedCategory()
        {
            _sut.SelectedMonth = _sut.Months[DateTime.Now.Month - 1];
            _sut.SelectedYear = DateTime.Now.Year;

            _sut.CategoriesFilter.Clear();
            _sut.CategoriesFilter.Add(new Category { CategoryId = 0, CategoryName = "Все" });
            _sut.CategoriesFilter.Add(new Category { CategoryId = 1, CategoryName = "Категория X" });
            _sut.SelectedCategoryFilter = _sut.CategoriesFilter.First(cat => cat.CategoryId == 1);

            _mockProductService.Setup(s => s.GetProductByIdAsync(1)).ReturnsAsync(new Product { ProductId = 1, ProductName = "Продукт А", CategoryId = 1 });
            _mockProductService.Setup(s => s.GetProductByIdAsync(2)).ReturnsAsync(new Product { ProductId = 2, ProductName = "Продукт Б", CategoryId = 2 });

            var supplyRecords = new List<ProductSupplyHistory>
            {
                new ProductSupplyHistory { SupplyRecordId = 1, ProductId = 1, SupplierId = 1, SuppliedQuantity = 10, SupplyUnitPrice = 5m, SupplyDate = new DateTime(_sut.SelectedYear, DateTime.Now.Month, 15) },
                new ProductSupplyHistory { SupplyRecordId = 2, ProductId = 2, SupplierId = 2, SuppliedQuantity = 20, SupplyUnitPrice = 2.5m, SupplyDate = new DateTime(_sut.SelectedYear, DateTime.Now.Month, 20) }
            };

            _mockSupplyHistoryService.Setup(s => s.GetSupplyRecordsFilteredAsync(
                It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<int?>(), It.IsAny<int?>(), 1))
                .ReturnsAsync(supplyRecords.Where(s =>
                {
                    var product = _mockProductService.Object.GetProductByIdAsync(s.ProductId).Result;
                    return product != null && product.CategoryId == 1;
                }).ToList());

            _mockSupplierService.Setup(s => s.GetSupplierByIdAsync(1)).ReturnsAsync(new Supplier { SupplierId = 1, CompanyName = "Поставщик Один" });

            await _sut.GenerateReportCommand.ExecuteAsync(null);

            Assert.Single(_sut.ReportDetails);
            Assert.Equal("Продукт А", _sut.ReportDetails.First().ProductName);
            Assert.Equal(50m, _sut.TotalCost);

            _mockSupplyHistoryService.Verify(s => s.GetSupplyRecordsFilteredAsync(
                It.IsAny<DateTime>(), It.IsAny<DateTime>(), null, null, 1), Times.Once);
        }

        [Fact]
        public async Task GenerateReportCommand_ShouldHandleNullProductAndSupplierNamesGracefully()
        {
            _sut.SelectedMonth = _sut.Months[DateTime.Now.Month - 1];
            _sut.SelectedYear = DateTime.Now.Year;

            var supplyRecords = new List<ProductSupplyHistory>
            {
                new ProductSupplyHistory { SupplyRecordId = 1, ProductId = 99, SupplierId = 99, SuppliedQuantity = 10, SupplyUnitPrice = 5m, SupplyDate = new DateTime(_sut.SelectedYear, DateTime.Now.Month, 15) }
            };
            _mockSupplyHistoryService.Setup(s => s.GetSupplyRecordsFilteredAsync(
                It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<int?>()))
                .ReturnsAsync(supplyRecords);

            _mockProductService.Setup(s => s.GetProductByIdAsync(99)).ReturnsAsync((Product)null);
            _mockSupplierService.Setup(s => s.GetSupplierByIdAsync(99)).ReturnsAsync((Supplier)null);

            await _sut.GenerateReportCommand.ExecuteAsync(null);

            Assert.Single(_sut.ReportDetails);
            var item = _sut.ReportDetails.First();
            Assert.Equal("Неизвестный продукт", item.ProductName);
            Assert.Equal("Неизвестный поставщик", item.SupplierName);
            Assert.Equal(50m, item.TotalItemCost);
            Assert.Equal(50m, _sut.TotalCost);
        }
    }
}