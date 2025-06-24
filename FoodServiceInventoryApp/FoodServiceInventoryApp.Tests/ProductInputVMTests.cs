using FoodServiceInventoryApp.Models;
using FoodServiceInventoryApp.Services;
using FoodServiceInventoryApp.ViewModels;
using Moq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Xunit;

namespace FoodServiceInventoryApp.Tests
{
    public class ProductInputVMTests
    {
        private readonly Mock<IProductService> _mockProductService;
        private readonly Mock<ICategoryService> _mockCategoryService;
        private readonly Mock<ISupplierService> _mockSupplierService;
        private readonly Mock<IProductSupplyHistoryService> _mockProductSupplyHistoryService;

        private readonly ProductInputVM _sut;

        public ProductInputVMTests()
        {
            _mockProductService = new Mock<IProductService>();
            _mockCategoryService = new Mock<ICategoryService>();
            _mockSupplierService = new Mock<ISupplierService>();
            _mockProductSupplyHistoryService = new Mock<IProductSupplyHistoryService>();

            _mockCategoryService.Setup(s => s.GetAllCategoriesAsync())
                                .ReturnsAsync(new List<Category> { new Category { CategoryId = 1, CategoryName = "Напитки" } });
            _mockSupplierService.Setup(s => s.GetAllSuppliersAsync())
                                .ReturnsAsync(new List<Supplier> { new Supplier { SupplierId = 1, CompanyName = "Поставщик А" } });

            _sut = new ProductInputVM(
                _mockProductService.Object,
                _mockCategoryService.Object,
                _mockSupplierService.Object,
                _mockProductSupplyHistoryService.Object
            );
        }

        [Fact]
        public async Task LoadProductForEdit_ShouldSetProductProperties_WhenProductExists()
        {
            var testCategory = new Category { CategoryId = 1, CategoryName = "Напитки" };
            var testProduct = new Product
            {
                ProductId = 1,
                ProductName = "Тестовый Продукт",
                Quantity = 50,
                UnitOfMeasure = "шт.",
                UnitPrice = 10.50m,
                LastSupplyDate = new DateTime(2023, 1, 15),
                CategoryId = testCategory.CategoryId,
                Category = testCategory
            };

            _mockCategoryService.Setup(s => s.GetAllCategoriesAsync())
                                .ReturnsAsync(new List<Category> { testCategory });
            var sutWithCorrectCategories = new ProductInputVM(
                _mockProductService.Object,
                _mockCategoryService.Object,
                _mockSupplierService.Object,
                _mockProductSupplyHistoryService.Object
            );
            await sutWithCorrectCategories.LoadCategoriesCommand.ExecuteAsync(null);


            _mockProductService.Setup(s => s.GetProductByIdAsync(1)).ReturnsAsync(testProduct);

            await sutWithCorrectCategories.LoadProductForEdit(1);

            Assert.True(sutWithCorrectCategories.IsEditMode);
            Assert.Equal(testProduct.ProductId, sutWithCorrectCategories.ProductId);
            Assert.Equal(testProduct.ProductName, sutWithCorrectCategories.ProductName);
            Assert.Equal(testProduct.Quantity, sutWithCorrectCategories.Quantity);
            Assert.Equal(testProduct.UnitOfMeasure, sutWithCorrectCategories.UnitOfMeasure);
            Assert.Equal(testProduct.UnitPrice, sutWithCorrectCategories.UnitPrice);
            Assert.Equal(testProduct.LastSupplyDate, sutWithCorrectCategories.LastSupplyDate);
            Assert.Equal(testProduct.CategoryId, sutWithCorrectCategories.SelectedCategory.CategoryId);
        }

        [Fact]
        public async Task LoadProductForEdit_ShouldNotSetProductProperties_WhenProductDoesNotExist()
        {
            _mockProductService.Setup(s => s.GetProductByIdAsync(It.IsAny<int>())).ReturnsAsync((Product)null);

            await _sut.LoadProductForEdit(99);

            Assert.False(_sut.IsEditMode);
            Assert.Equal(0, _sut.ProductId);
            Assert.Equal(string.Empty, _sut.ProductName);
            Assert.Equal(0, _sut.Quantity);
            Assert.Equal(string.Empty, _sut.UnitOfMeasure);
            Assert.Equal(0m, _sut.UnitPrice);
        }


        [Fact]
        public async Task SaveProductCommand_CanExecute_ReturnsTrue_WhenProductIsValid()
        {
            _sut.ProductName = "Новый Продукт";
            _sut.Quantity = 100;
            _sut.UnitOfMeasure = "кг";
            _sut.UnitPrice = 10.0m;
            _sut.LastSupplyDate = DateTime.Now;
            _sut.SelectedCategory = new Category { CategoryId = 1, CategoryName = "Тест" };
            _sut.SelectedSupplier = new Supplier { SupplierId = 1, CompanyName = "Поставщик" };
            _sut.IsEditMode = false;

            Assert.True(((ICommand)_sut.SaveProductCommand).CanExecute(null));
        }

        [Fact]
        public async Task SaveProductCommand_CanExecute_ReturnsFalse_WhenProductNameIsEmpty()
        {
            _sut.ProductName = "";
            _sut.Quantity = 100;
            _sut.UnitOfMeasure = "кг";
            _sut.UnitPrice = 10.0m;
            _sut.LastSupplyDate = DateTime.Now;
            _sut.SelectedCategory = new Category { CategoryId = 1, CategoryName = "Тест" };
            _sut.SelectedSupplier = new Supplier { SupplierId = 1, CompanyName = "Поставщик" };

            Assert.False(((ICommand)_sut.SaveProductCommand).CanExecute(null));
        }

        [Fact]
        public async Task SaveProductCommand_CanExecute_ReturnsFalse_WhenUnitPriceIsZeroAndAddingNew()
        {
            _sut.IsEditMode = false;
            _sut.ProductName = "Новый Продукт";
            _sut.Quantity = 100;
            _sut.UnitOfMeasure = "кг";
            _sut.UnitPrice = 0.0m;
            _sut.LastSupplyDate = DateTime.Now;
            _sut.SelectedCategory = new Category { CategoryId = 1, CategoryName = "Тест" };
            _sut.SelectedSupplier = new Supplier { SupplierId = 1, CompanyName = "Поставщик" };

            Assert.False(((ICommand)_sut.SaveProductCommand).CanExecute(null));
        }

        [Fact]
        public async Task SaveProductCommand_CanExecute_ReturnsFalse_WhenQuantityIsZeroAndAddingNew()
        {
            _sut.IsEditMode = false;
            _sut.ProductName = "Новый Продукт";
            _sut.Quantity = 0;
            _sut.UnitOfMeasure = "кг";
            _sut.UnitPrice = 10.0m;
            _sut.LastSupplyDate = DateTime.Now;
            _sut.SelectedCategory = new Category { CategoryId = 1, CategoryName = "Тест" };
            _sut.SelectedSupplier = new Supplier { SupplierId = 1, CompanyName = "Поставщик" };

            Assert.False(((ICommand)_sut.SaveProductCommand).CanExecute(null));
        }

        [Fact]
        public async Task SaveProductCommand_CanExecute_ReturnsFalse_WhenSelectedCategoryIsNull()
        {
            _sut.ProductName = "Новый Продукт";
            _sut.Quantity = 100;
            _sut.UnitOfMeasure = "кг";
            _sut.UnitPrice = 10.0m;
            _sut.LastSupplyDate = DateTime.Now;
            _sut.SelectedCategory = null;
            _sut.SelectedSupplier = new Supplier { SupplierId = 1, CompanyName = "Поставщик" };

            Assert.False(((ICommand)_sut.SaveProductCommand).CanExecute(null));
        }

        [Fact]
        public async Task SaveProductCommand_ShouldCallUpdateProduct_WhenInEditMode()
        {
            _sut.IsEditMode = true;
            _sut.ProductId = 5;
            _sut.ProductName = "Обновленный Продукт";
            _sut.Quantity = 150;
            _sut.UnitOfMeasure = "кг";
            _sut.UnitPrice = 20.0m;
            _sut.LastSupplyDate = new DateTime(2024, 6, 22);
            _sut.SelectedCategory = new Category { CategoryId = 2, CategoryName = "Молочные продукты" };

            var existingProduct = new Product
            {
                ProductId = 5,
                ProductName = "Старый Продукт",
                Quantity = 100,
                UnitOfMeasure = "кг",
                UnitPrice = 15.0m,
                LastSupplyDate = new DateTime(2024, 5, 1),
                CategoryId = 1
            };

            _mockProductService.Setup(s => s.GetProductByIdAsync(5)).ReturnsAsync(existingProduct);
            _mockProductService.Setup(s => s.ProductExistsByNameAsync(It.IsAny<string>())).ReturnsAsync(false);
            _mockProductService.Setup(s => s.UpdateProductAsync(It.IsAny<Product>())).Returns(Task.CompletedTask);

            await _sut.SaveProductCommand.ExecuteAsync(null);

            _mockProductService.Verify(s => s.UpdateProductAsync(It.Is<Product>(p =>
                p.ProductId == 5 &&
                p.ProductName == "Обновленный Продукт" &&
                p.Quantity == 150 &&
                p.UnitOfMeasure == "кг" &&
                p.UnitPrice == 20.0m &&
                p.LastSupplyDate == new DateTime(2024, 6, 22) &&
                p.CategoryId == 2)), Times.Once);

            _mockProductService.Verify(s => s.AddProductAsync(It.IsAny<Product>()), Times.Never);
            _mockProductSupplyHistoryService.Verify(s => s.AddSupplyRecordAsync(It.IsAny<ProductSupplyHistory>()), Times.Never);
        }

        [Fact]
        public async Task SaveProductCommand_ShouldNotAddProduct_WhenProductNameAlreadyExistsInAddMode()
        {
            _sut.IsEditMode = false;
            _sut.ProductName = "Существующий Продукт";
            _sut.Quantity = 50;
            _sut.UnitOfMeasure = "уп.";
            _sut.UnitPrice = 10.0m;
            _sut.LastSupplyDate = DateTime.Now;
            _sut.SelectedCategory = new Category { CategoryId = 1, CategoryName = "Тест" };
            _sut.SelectedSupplier = new Supplier { SupplierId = 1, CompanyName = "Поставщик" };

            _mockProductService.Setup(s => s.ProductExistsByNameAsync("Существующий Продукт")).ReturnsAsync(true);

            await _sut.SaveProductCommand.ExecuteAsync(null);

            _mockProductService.Verify(s => s.AddProductAsync(It.IsAny<Product>()), Times.Never);
            _mockProductService.Verify(s => s.UpdateProductAsync(It.IsAny<Product>()), Times.Never);
            _mockProductSupplyHistoryService.Verify(s => s.AddSupplyRecordAsync(It.IsAny<ProductSupplyHistory>()), Times.Never);
        }

        [Fact]
        public async Task SaveProductCommand_ShouldNotUpdateProduct_WhenEditModeButProductNameConflictsWithAnotherProduct()
        {
            _sut.IsEditMode = true;
            _sut.ProductId = 1;
            _sut.ProductName = "Конфликтное Название";
            _sut.Quantity = 50;
            _sut.UnitOfMeasure = "шт.";
            _sut.UnitPrice = 10.0m;
            _sut.LastSupplyDate = DateTime.Now;
            _sut.SelectedCategory = new Category { CategoryId = 1, CategoryName = "Тест" };

            var originalProduct = new Product { ProductId = 1, ProductName = "Оригинальное Название" };
            var conflictingProduct = new Product { ProductId = 2, ProductName = "Конфликтное Название" };

            _mockProductService.Setup(s => s.GetProductByIdAsync(1)).ReturnsAsync(originalProduct);
            _mockProductService.Setup(s => s.ProductExistsByNameAsync("Конфликтное Название")).ReturnsAsync(true);

            await _sut.SaveProductCommand.ExecuteAsync(null);

            _mockProductService.Verify(s => s.UpdateProductAsync(It.IsAny<Product>()), Times.Never);
            _mockProductService.Verify(s => s.AddProductAsync(It.IsAny<Product>()), Times.Never);
            _mockProductSupplyHistoryService.Verify(s => s.AddSupplyRecordAsync(It.IsAny<ProductSupplyHistory>()), Times.Never);
        }

        [Fact]
        public void ResetForm_ShouldClearAllProperties()
        {
            _sut.IsEditMode = true;
            _sut.ProductId = 1;
            _sut.ProductName = "Какой-то продукт";
            _sut.Quantity = 100;
            _sut.UnitOfMeasure = "кг";
            _sut.UnitPrice = 99.99m;
            _sut.LastSupplyDate = new DateTime(2024, 1, 1);
            _sut.SelectedCategory = new Category { CategoryId = 5, CategoryName = "Фрукты" };
            _sut.SelectedSupplier = new Supplier { SupplierId = 3, CompanyName = "Овощи и Ко" };

            _sut.ResetForm();

            Assert.False(_sut.IsEditMode);
            Assert.Equal(0, _sut.ProductId);
            Assert.Equal(string.Empty, _sut.ProductName);
            Assert.Equal(0, _sut.Quantity);
            Assert.Equal(string.Empty, _sut.UnitOfMeasure);
            Assert.Equal(0m, _sut.UnitPrice);
            Assert.Equal(DateTime.Now.Date, _sut.LastSupplyDate.Date);

            Assert.NotNull(_sut.Categories);
            Assert.NotNull(_sut.Suppliers);
           
            Assert.Equal(_sut.Categories.FirstOrDefault(), _sut.SelectedCategory);
            Assert.Equal(_sut.Suppliers.FirstOrDefault(), _sut.SelectedSupplier);
        }

        [Fact]
        public void ProductName_Set_RaisesPropertyChanged()
        {
            string changedPropertyName = null;
            _sut.PropertyChanged += (sender, e) =>
            {
                changedPropertyName = e.PropertyName;
            };

            _sut.ProductName = "Новое Имя";

            Assert.Equal("ProductName", changedPropertyName);
        }
    }
}