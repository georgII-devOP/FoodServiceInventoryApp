﻿using Xunit;
using Moq;
using FoodServiceInventoryApp.ViewModels;
using FoodServiceInventoryApp.Services;
using FoodServiceInventoryApp.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using CommunityToolkit.Mvvm.Input;
using System;
using Microsoft.Extensions.DependencyInjection;

namespace FoodServiceInventoryApp.Tests
{
    public class StockReportVMTests
    {
        private readonly Mock<IProductService> _mockProductService;
        private readonly Mock<ICategoryService> _mockCategoryService;
        private readonly Mock<ISupplierService> _mockSupplierService;
        private readonly Mock<IProductSupplyHistoryService> _mockProductSupplyHistoryService;
        private readonly Mock<IServiceProvider> _mockServiceProvider;
        private readonly Mock<IMessageService> _mockMessageService;

        private readonly Mock<ProductInputVM> _mockProductInputVM;
        private readonly Mock<ProductRemovalVM> _mockProductRemovalVM;
        private readonly Mock<PurchaseCostReportVM> _mockPurchaseCostReportVM;
        private readonly Mock<SupplierReportVM> _mockSupplierReportVM;
        private readonly Mock<PurchasePlanVM> _mockPurchasePlanVM;
        private readonly Mock<MainViewModel> _mockMainViewModel;

        private readonly Mock<StockReportVM> _sutMock;
        private readonly StockReportVM _sut;

        public StockReportVMTests()
        {
            _mockProductService = new Mock<IProductService>();
            _mockCategoryService = new Mock<ICategoryService>();
            _mockSupplierService = new Mock<ISupplierService>();
            _mockProductSupplyHistoryService = new Mock<IProductSupplyHistoryService>();
            _mockServiceProvider = new Mock<IServiceProvider>();
            _mockMessageService = new Mock<IMessageService>();

            _mockMessageService.Setup(m => m.ShowMessage(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<MessageType>()
            )).Verifiable();
            _mockMessageService.Setup(m => m.ShowConfirmation(
                It.IsAny<string>(),
                It.IsAny<string>()
            )).Returns(true);

            _mockProductService.Setup(s => s.GetAllProductsAsync()).ReturnsAsync(new List<Product>());
            _mockCategoryService.Setup(s => s.GetAllCategoriesAsync()).ReturnsAsync(new List<Category>());
            _mockSupplierService.Setup(s => s.GetAllSuppliersAsync()).ReturnsAsync(new List<Supplier>());
            _mockProductService.Setup(s => s.GetProductByIdAsync(It.IsAny<int>())).ReturnsAsync((int id) => new Product { ProductId = id, ProductName = $"Product {id}" });
            _mockProductService.Setup(s => s.ProductExistsByNameAsync(It.IsAny<string>())).ReturnsAsync(false);

            _mockProductInputVM = new Mock<ProductInputVM>(
                _mockProductService.Object,
                _mockCategoryService.Object,
                _mockSupplierService.Object,
                _mockProductSupplyHistoryService.Object,
                _mockMessageService.Object
            )
            { CallBase = true };

            Mock.Get(_mockProductInputVM.Object).Setup(x => x.LoadCategoriesAsync()).Returns(Task.CompletedTask);
            Mock.Get(_mockProductInputVM.Object).Setup(x => x.LoadSuppliersAsync()).Returns(Task.CompletedTask);
            Mock.Get(_mockProductInputVM.Object).Setup(x => x.ResetForm()).Callback(() => { });
            Mock.Get(_mockProductInputVM.Object).Setup(x => x.LoadProductForEdit(It.IsAny<int>())).Callback(() => { });


            _mockProductRemovalVM = new Mock<ProductRemovalVM>(_mockProductService.Object, _mockMessageService.Object) { CallBase = true };
            _mockPurchaseCostReportVM = new Mock<PurchaseCostReportVM>(
                _mockProductSupplyHistoryService.Object, _mockProductService.Object, _mockCategoryService.Object, _mockSupplierService.Object
            )
            { CallBase = true };
            _mockSupplierReportVM = new Mock<SupplierReportVM>(
                _mockProductSupplyHistoryService.Object, _mockSupplierService.Object
            )
            { CallBase = true };

            _mockPurchasePlanVM = new Mock<PurchasePlanVM>(
                _mockSupplierService.Object,
                _mockProductSupplyHistoryService.Object,
                _mockProductService.Object
            )
            { CallBase = true };
            Mock.Get(_mockPurchasePlanVM.Object).Setup(x => x.LoadSuppliersAsync()).Returns(Task.CompletedTask);


            _mockMainViewModel = new Mock<MainViewModel>(_mockServiceProvider.Object)
            {
                CallBase = true
            };

            _mockServiceProvider.Setup(s => s.GetService(typeof(ProductInputVM))).Returns(_mockProductInputVM.Object);
            _mockServiceProvider.Setup(s => s.GetService(typeof(ProductRemovalVM))).Returns(_mockProductRemovalVM.Object);
            _mockServiceProvider.Setup(s => s.GetService(typeof(PurchaseCostReportVM))).Returns(_mockPurchaseCostReportVM.Object);
            _mockServiceProvider.Setup(s => s.GetService(typeof(SupplierReportVM))).Returns(_mockSupplierReportVM.Object);
            _mockServiceProvider.Setup(s => s.GetService(typeof(PurchasePlanVM))).Returns(_mockPurchasePlanVM.Object);
            _mockServiceProvider.Setup(s => s.GetService(typeof(MainViewModel))).Returns(_mockMainViewModel.Object);
            _mockServiceProvider.Setup(s => s.GetService(typeof(IMessageService))).Returns(_mockMessageService.Object);


            _sutMock = new Mock<StockReportVM>(
                _mockProductService.Object,
                _mockServiceProvider.Object,
                _mockMainViewModel.Object,
                _mockMessageService.Object
            );
            _sut = _sutMock.Object;

            _sutMock.Setup(x => x.LoadProductsAsync()).Returns(Task.CompletedTask);

            _sut.Products.Clear();
        }

        [Fact]
        public void EditProductCommand_CanExecute_ShouldBeFalseWhenNoProductSelected()
        {
            _sut.SelectedProduct = null;
            Assert.False(_sut.EditProductCommand.CanExecute(null));
        }

        [Fact]
        public void EditProductCommand_CanExecute_ShouldBeTrueWhenProductIsSelected()
        {
            _sut.SelectedProduct = new Product { ProductId = 1, ProductName = "Тестовый Продукт" };
            Assert.True(_sut.EditProductCommand.CanExecute(null));
        }

        [Fact]
        public async Task EditProductCommand_ShouldNavigateToProductInputForEdit_WhenProductSelected()
        {
            var selectedProduct = new Product { ProductId = 5, ProductName = "Выбранный Продукт" };
            _sut.SelectedProduct = selectedProduct;

            Mock.Get(_mockProductInputVM.Object).Invocations.Clear();

            await _sut.EditProductCommand.ExecuteAsync(null);

            _mockMessageService.Verify(m => m.ShowMessage(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<MessageType>()
            ), Times.Never);
        }

        [Fact]
        public async Task EditProductCommand_ShouldDisplayWarning_WhenNoProductSelected()
        {
            _sut.SelectedProduct = null;

            await _sut.EditProductCommand.ExecuteAsync(null);

            _mockMessageService.Verify(m => m.ShowMessage(
                "Пожалуйста, выберите продукт для редактирования.",
                "Ошибка",
                MessageType.Warning
            ), Times.Once);
        }
    }
}