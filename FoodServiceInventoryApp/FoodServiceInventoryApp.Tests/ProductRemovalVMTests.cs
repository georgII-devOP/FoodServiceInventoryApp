using CommunityToolkit.Mvvm.Input;
using FoodServiceInventoryApp.Models;
using FoodServiceInventoryApp.Services;
using FoodServiceInventoryApp.ViewModels;
using Moq;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Xunit;

namespace FoodServiceInventoryApp.Tests
{
    public class ProductRemovalVMTests
    {
        private readonly Mock<IProductService> _mockProductService;
        private readonly ProductRemovalVM _sut;

        public ProductRemovalVMTests()
        {
            _mockProductService = new Mock<IProductService>();

            _mockProductService.Setup(s => s.GetAllProductsAsync())
                               .ReturnsAsync(new List<Product>
                               {
                                   new Product { ProductId = 1, ProductName = "Яблоки", Quantity = 100, UnitOfMeasure = "кг" },
                                   new Product { ProductId = 2, ProductName = "Молоко", Quantity = 50, UnitOfMeasure = "л" }
                               });

            _sut = new ProductRemovalVM(_mockProductService.Object);
        }

        [Fact]
        public async Task LoadProductsCommand_ShouldLoadProductsIntoCollection()
        {
            var productsToReturn = new List<Product>
            {
                new Product { ProductId = 3, ProductName = "Тестовый Продукт 3", Quantity = 10, UnitOfMeasure = "шт." },
                new Product { ProductId = 4, ProductName = "Тестовый Продукт 4", Quantity = 20, UnitOfMeasure = "шт." }
            };

            _mockProductService.Invocations.Clear();

            _mockProductService.Setup(s => s.GetAllProductsAsync()).ReturnsAsync(productsToReturn);

            await _sut.LoadProductsCommand.ExecuteAsync(null);

            _mockProductService.Verify(s => s.GetAllProductsAsync(), Times.Once);

            Assert.Equal(productsToReturn.Count, _sut.Products.Count);
            Assert.Contains(_sut.Products, p => p.ProductName == "Тестовый Продукт 3");
            Assert.Contains(_sut.Products, p => p.ProductName == "Тестовый Продукт 4");
        }

        [Theory]
        [InlineData(null, 10, false, "Нет выбранного продукта")]
        [InlineData("Яблоки", 0, false, "Количество для списания 0")]
        [InlineData("Яблоки", -5, false, "Количество для списания отрицательное")]
        [InlineData("Яблоки", 101, false, "Количество для списания превышает остаток")]
        [InlineData("Яблоки", 50, true, "Все условия соблюдены")]
        public void DeductProductQuantityCommand_CanExecute(string productName, decimal quantityToDeduct, bool expectedCanExecute, string reason)
        {
            if (productName != null)
            {
                _sut.SelectedProduct = _sut.Products.FirstOrDefault(p => p.ProductName == productName) ??
                                      new Product { ProductId = 3, ProductName = productName, Quantity = 100, UnitOfMeasure = "шт." };
            }
            else
            {
                _sut.SelectedProduct = null;
            }
            _sut.QuantityToDeduct = quantityToDeduct;

            var canExecute = ((IAsyncRelayCommand)_sut.DeductProductQuantityCommand).CanExecute(null);

            Assert.Equal(expectedCanExecute, canExecute);
        }

        [Fact]
        public async Task DeductProductQuantityCommand_ShouldDeductQuantity_WhenConfirmedAndValid()
        {
            var initialProduct = new Product { ProductId = 1, ProductName = "Яблоки", Quantity = 100, UnitOfMeasure = "кг" };
            _sut.Products.Clear();
            _sut.Products.Add(initialProduct);

            _sut.SelectedProduct = initialProduct;
            _sut.QuantityToDeduct = 25;

            _mockProductService.Setup(s => s.UpdateProductQuantityAsync(initialProduct.ProductId, -_sut.QuantityToDeduct))
                               .Returns(Task.CompletedTask);

            await _sut.DeductProductQuantityCommand.ExecuteAsync(null);

            _mockProductService.Verify(s => s.UpdateProductQuantityAsync(initialProduct.ProductId, -25), Times.Once);

            Assert.Equal(0, _sut.QuantityToDeduct);

            _mockProductService.Verify(s => s.GetAllProductsAsync(), Times.Exactly(2));
        }

        [Fact]
        public async Task DeductProductQuantityCommand_ShouldNotDeductQuantity_WhenQuantityToDeductIsInvalid()
        {
            var initialProduct = new Product { ProductId = 1, ProductName = "Яблоки", Quantity = 100, UnitOfMeasure = "кг" };
            _sut.Products.Clear();
            _sut.Products.Add(initialProduct);

            _sut.SelectedProduct = initialProduct;
            _sut.QuantityToDeduct = 150;

            await _sut.DeductProductQuantityCommand.ExecuteAsync(null);

            _mockProductService.Verify(s => s.UpdateProductQuantityAsync(It.IsAny<int>(), It.IsAny<decimal>()), Times.Never);
            Assert.Equal(150, _sut.QuantityToDeduct);
            _mockProductService.Verify(s => s.GetAllProductsAsync(), Times.Once);
        }

        [Fact]
        public async Task DeductProductQuantityCommand_ShouldHandleServiceErrorDuringDeduction()
        {
            var initialProduct = new Product { ProductId = 1, ProductName = "Яблоки", Quantity = 100, UnitOfMeasure = "кг" };
            _sut.Products.Clear();
            _sut.Products.Add(initialProduct);

            _sut.SelectedProduct = initialProduct;
            _sut.QuantityToDeduct = 10;

            _mockProductService.Setup(s => s.UpdateProductQuantityAsync(initialProduct.ProductId, -_sut.QuantityToDeduct))
                               .ThrowsAsync(new System.Exception("Ошибка базы данных"));

            await _sut.DeductProductQuantityCommand.ExecuteAsync(null);

            _mockProductService.Verify(s => s.UpdateProductQuantityAsync(initialProduct.ProductId, -10), Times.Once);

            Assert.Equal(10, _sut.QuantityToDeduct);

            _mockProductService.Verify(s => s.GetAllProductsAsync(), Times.Once);
        }
    }
}