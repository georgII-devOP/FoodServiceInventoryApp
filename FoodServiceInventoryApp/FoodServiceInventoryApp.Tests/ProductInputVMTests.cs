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
        private readonly Mock<IMessageService> _mockMessageService;

        private readonly ProductInputVM _sut;

        public ProductInputVMTests()
        {
            _mockProductService = new Mock<IProductService>();
            _mockCategoryService = new Mock<ICategoryService>();
            _mockSupplierService = new Mock<ISupplierService>();
            _mockProductSupplyHistoryService = new Mock<IProductSupplyHistoryService>();
            _mockMessageService = new Mock<IMessageService>();

            _mockMessageService.Setup(m => m.ShowMessage(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<MessageType>()
            )).Verifiable();

            _mockCategoryService.Setup(s => s.GetAllCategoriesAsync())
                                .ReturnsAsync(new List<Category> { new Category { CategoryId = 1, CategoryName = "Напитки" } });
            _mockSupplierService.Setup(s => s.GetAllSuppliersAsync())
                                .ReturnsAsync(new List<Supplier> { new Supplier { SupplierId = 1, CompanyName = "Поставщик А" } });

            _sut = new ProductInputVM(
                _mockProductService.Object,
                _mockCategoryService.Object,
                _mockSupplierService.Object,
                _mockProductSupplyHistoryService.Object,
                _mockMessageService.Object
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

            await _sut.LoadCategoriesCommand.ExecuteAsync(null);


            _mockProductService.Setup(s => s.GetProductByIdAsync(1)).ReturnsAsync(testProduct);

            await _sut.LoadProductForEdit(1);

            Assert.True(_sut.IsEditMode);
            Assert.Equal(testProduct.ProductId, _sut.ProductId);
            Assert.Equal(testProduct.ProductName, _sut.ProductName);
            Assert.Equal(testProduct.Quantity, _sut.Quantity);
            Assert.Equal(testProduct.UnitOfMeasure, _sut.UnitOfMeasure);
            Assert.Equal(testProduct.UnitPrice, _sut.UnitPrice);
            Assert.Equal(testProduct.LastSupplyDate, _sut.LastSupplyDate);
            Assert.Equal(testProduct.CategoryId, _sut.SelectedCategory.CategoryId);

            _mockMessageService.Verify(m => m.ShowMessage(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<MessageType>()
            ), Times.Never);
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

            Assert.Equal(DateTime.Now.Date, _sut.LastSupplyDate.Date);

            Assert.Equal(_sut.Categories.FirstOrDefault(), _sut.SelectedCategory);
            Assert.Equal(_sut.Suppliers.FirstOrDefault(), _sut.SelectedSupplier);

            _mockMessageService.Verify(m => m.ShowMessage(
                "Продукт не найден для редактирования.",
                "Ошибка",
                MessageType.Error
            ), Times.Once);
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
            await _sut.LoadCategoriesCommand.ExecuteAsync(null);

            _sut.IsEditMode = true;
            _sut.ProductId = 5;
            _sut.ProductName = "Обновленный Продукт";
            _sut.Quantity = 150;
            _sut.UnitOfMeasure = "кг";
            _sut.UnitPrice = 20.0m;
            _sut.LastSupplyDate = new DateTime(2024, 6, 22);
            _sut.SelectedCategory = new Category { CategoryId = 1, CategoryName = "Напитки" };

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
            _mockProductService.Setup(s => s.ProductExistsByNameAsync("Обновленный Продукт")).ReturnsAsync(false);
            _mockProductService.Setup(s => s.UpdateProductAsync(It.IsAny<Product>())).Returns(Task.CompletedTask);

            await _sut.SaveProductCommand.ExecuteAsync(null);

            _mockProductService.Verify(s => s.UpdateProductAsync(It.Is<Product>(p =>
                p.ProductId == 5 &&
                p.ProductName == "Обновленный Продукт" &&
                p.Quantity == 150 &&
                p.UnitOfMeasure == "кг" &&
                p.UnitPrice == 20.0m &&
                p.LastSupplyDate == new DateTime(2024, 6, 22) &&
                p.CategoryId == 1)), Times.Once);

            _mockProductService.Verify(s => s.AddProductAsync(It.IsAny<Product>()), Times.Never);
            _mockProductSupplyHistoryService.Verify(s => s.AddSupplyRecordAsync(It.IsAny<ProductSupplyHistory>()), Times.Never);

            _mockMessageService.Verify(m => m.ShowMessage(
                "Продукт успешно обновлен!",
                "Успех",
                MessageType.Information
            ), Times.Once);

            _mockMessageService.Verify(m => m.ShowMessage(
                It.Is<string>(s => s.Contains("Ошибка")),
                It.IsAny<string>(),
                It.IsAny<MessageType>()
            ), Times.Never);
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

            _mockMessageService.Verify(m => m.ShowMessage(
                $"Продукт с названием '{_sut.ProductName}' уже существует.",
                "Ошибка",
                MessageType.Error
            ), Times.Once);
        }

        [Fact]
        public async Task SaveProductCommand_ShouldNotUpdateProduct_WhenEditModeButProductNameConflictsWithAnotherProduct()
        {
            await _sut.LoadCategoriesCommand.ExecuteAsync(null);

            _sut.IsEditMode = true;
            _sut.ProductId = 1;
            _sut.ProductName = "Конфликтное Название";
            _sut.Quantity = 50;
            _sut.UnitOfMeasure = "шт.";
            _sut.UnitPrice = 10.0m;
            _sut.LastSupplyDate = DateTime.Now;
            _sut.SelectedCategory = new Category { CategoryId = 1, CategoryName = "Напитки" };

            var originalProduct = new Product { ProductId = 1, ProductName = "Оригинальное Название", CategoryId = 1 };

            _mockProductService.Setup(s => s.GetProductByIdAsync(1)).ReturnsAsync(originalProduct);
            _mockProductService.Setup(s => s.ProductExistsByNameAsync("Конфликтное Название")).ReturnsAsync(true);

            await _sut.SaveProductCommand.ExecuteAsync(null);

            _mockProductService.Verify(s => s.UpdateProductAsync(It.IsAny<Product>()), Times.Never);
            _mockProductService.Verify(s => s.AddProductAsync(It.IsAny<Product>()), Times.Never);
            _mockProductSupplyHistoryService.Verify(s => s.AddSupplyRecordAsync(It.IsAny<ProductSupplyHistory>()), Times.Never);

            _mockMessageService.Verify(m => m.ShowMessage(
                $"Продукт с названием '{_sut.ProductName}' уже существует.",
                "Ошибка",
                MessageType.Error
            ), Times.Once);
        }

        [Fact]
        public void ResetForm_ShouldClearAllProperties()
        {
            _sut.LoadCategoriesCommand.ExecuteAsync(null).Wait();
            _sut.LoadSuppliersCommand.ExecuteAsync(null).Wait();

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

        [Fact]
        public async Task SaveProductCommand_ShouldAddProductAndSupplyHistory_WhenInAddModeAndValid()
        {
            await _sut.LoadCategoriesCommand.ExecuteAsync(null);
            await _sut.LoadSuppliersCommand.ExecuteAsync(null);

            _sut.IsEditMode = false;
            _sut.ProductName = "Новый Уникальный Продукт";
            _sut.Quantity = 10;
            _sut.UnitOfMeasure = "шт.";
            _sut.UnitPrice = 5.50m;
            _sut.LastSupplyDate = new DateTime(2024, 6, 24);
            _sut.SelectedCategory = _sut.Categories.First();
            _sut.SelectedSupplier = _sut.Suppliers.First();

            _mockProductService.Setup(s => s.ProductExistsByNameAsync(It.IsAny<string>())).ReturnsAsync(false);

            _mockProductService.Setup(s => s.AddProductAsync(It.IsAny<Product>()))
                   .Callback<Product>(product => product.ProductId = 100)
                   .Returns((Product product) => Task.FromResult(product));

            _mockProductSupplyHistoryService.Setup(s => s.AddSupplyRecordAsync(It.IsAny<ProductSupplyHistory>())).Returns(Task.CompletedTask);

            await _sut.SaveProductCommand.ExecuteAsync(null);

            _mockProductService.Verify(s => s.AddProductAsync(It.Is<Product>(p =>
                p.ProductName == "Новый Уникальный Продукт" &&
                p.Quantity == 10 &&
                p.UnitOfMeasure == "шт." &&
                p.UnitPrice == 5.50m &&
                p.LastSupplyDate == new DateTime(2024, 6, 24) &&
                p.CategoryId == _sut.SelectedCategory.CategoryId
            )), Times.Once);

            _mockProductSupplyHistoryService.Verify(s => s.AddSupplyRecordAsync(It.Is<ProductSupplyHistory>(psh =>
                psh.ProductId == 100 &&
                psh.SupplierId == _sut.SelectedSupplier.SupplierId &&
                psh.SuppliedQuantity == 10 &&
                psh.SupplyUnitPrice == 5.50m &&
                psh.SupplyDate == new DateTime(2024, 6, 24)
            )), Times.Once);

            _mockProductService.Verify(s => s.UpdateProductAsync(It.IsAny<Product>()), Times.Never);

            _mockMessageService.Verify(m => m.ShowMessage(
                $"Продукт 'Новый Уникальный Продукт' и его первая поставка успешно добавлены!",
                "Успех",
                MessageType.Information
            ), Times.Once);

            Assert.False(_sut.IsEditMode);
            Assert.Equal(0, _sut.ProductId);
        }

        [Fact]
        public async Task SaveProductCommand_ShouldDisplayError_WhenAddProductThrowsException()
        {
            await _sut.LoadCategoriesCommand.ExecuteAsync(null);
            await _sut.LoadSuppliersCommand.ExecuteAsync(null);

            _sut.IsEditMode = false;
            _sut.ProductName = "Продукт с ошибкой";
            _sut.Quantity = 1;
            _sut.UnitOfMeasure = "шт.";
            _sut.UnitPrice = 1;
            _sut.LastSupplyDate = DateTime.Now;
            _sut.SelectedCategory = _sut.Categories.First();
            _sut.SelectedSupplier = _sut.Suppliers.First();

            _mockProductService.Setup(s => s.ProductExistsByNameAsync(It.IsAny<string>())).ReturnsAsync(false);
            _mockProductService.Setup(s => s.AddProductAsync(It.IsAny<Product>())).ThrowsAsync(new InvalidOperationException("Тестовая ошибка добавления продукта."));

            await _sut.SaveProductCommand.ExecuteAsync(null);

            _mockMessageService.Verify(m => m.ShowMessage(
                It.Is<string>(s => s.Contains("Ошибка при добавлении продукта")),
                "Ошибка",
                MessageType.Error
            ), Times.Once);

            _mockProductService.Verify(s => s.AddProductAsync(It.IsAny<Product>()), Times.Once);
            _mockProductSupplyHistoryService.Verify(s => s.AddSupplyRecordAsync(It.IsAny<ProductSupplyHistory>()), Times.Never);
        }
    }
}