using Xunit;
using Moq;
using FoodServiceInventoryApp.ViewModels;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using FoodServiceInventoryApp.Services;
using System.Collections.Generic;
using System.Threading.Tasks;
using FoodServiceInventoryApp.Models;

namespace FoodServiceInventoryApp.Tests
{
    public class MainViewModelTests
    {
        private Mock<IServiceProvider> _mockServiceProvider;
        private MainViewModel _sut;

        private Mock<IMessageService> _mockMessageService;

        private Mock<ProductInputVM> _mockProductInputVM;
        private Mock<ProductRemovalVM> _mockProductRemovalVM;
        private Mock<StockReportVM> _mockStockReportVM;
        private Mock<PurchaseCostReportVM> _mockPurchaseCostReportVM;
        private Mock<SupplierReportVM> _mockSupplierReportVM;
        private Mock<PurchasePlanVM> _mockPurchasePlanVM;

        private Mock<IProductService> _mockProductService;
        private Mock<ICategoryService> _mockCategoryService;
        private Mock<ISupplierService> _mockSupplierService;
        private Mock<IProductSupplyHistoryService> _mockProductSupplyHistoryService;

        public MainViewModelTests()
        {
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

            _mockProductService = new Mock<IProductService>();
            _mockCategoryService = new Mock<ICategoryService>();
            _mockSupplierService = new Mock<ISupplierService>();
            _mockProductSupplyHistoryService = new Mock<IProductSupplyHistoryService>();

            _mockCategoryService.Setup(s => s.GetAllCategoriesAsync())
                                 .ReturnsAsync(new List<Category>());
            _mockSupplierService.Setup(s => s.GetAllSuppliersAsync())
                                 .ReturnsAsync(new List<Supplier>());
            _mockProductService.Setup(s => s.GetAllProductsAsync())
                                 .ReturnsAsync(new List<Product>());
            _mockProductService.Setup(s => s.GetProductByIdAsync(It.IsAny<int>()))
                                 .ReturnsAsync((int id) => new Product { ProductId = id, ProductName = "Test Product", Quantity = 10, UnitOfMeasure = "шт.", UnitPrice = 5.0m, CategoryId = 1, LastSupplyDate = DateTime.Now });
            _mockSupplierService.Setup(s => s.GetSupplierByIdAsync(It.IsAny<int>()))
                                 .ReturnsAsync((int id) => new Supplier { SupplierId = id, CompanyName = "Test Supplier" });
            _mockProductSupplyHistoryService.Setup(s => s.GetSupplyRecordsFilteredAsync(
                It.IsAny<DateTime?>(), It.IsAny<DateTime?>(), It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<int?>()))
                .ReturnsAsync(new List<ProductSupplyHistory>());

            _mockProductInputVM = new Mock<ProductInputVM>(
                _mockProductService.Object,
                _mockCategoryService.Object,
                _mockSupplierService.Object,
                _mockProductSupplyHistoryService.Object,
                _mockMessageService.Object
            );
            _mockProductInputVM.Setup(vm => vm.ResetForm()).Verifiable();
            _mockProductInputVM.Setup(vm => vm.LoadProductForEdit(It.IsAny<int>())).Returns(Task.CompletedTask).Verifiable();


            _mockProductRemovalVM = new Mock<ProductRemovalVM>(_mockProductService.Object, _mockMessageService.Object);
            _mockProductRemovalVM.Setup(vm => vm.LoadProductsAsync()).Returns(Task.CompletedTask).Verifiable();

            _mockStockReportVM = new Mock<StockReportVM>(
                _mockProductService.Object,
                _mockServiceProvider.Object,
                null,
                _mockMessageService.Object
            );
            _mockStockReportVM.Setup(vm => vm.LoadProductsAsync()).Returns(Task.CompletedTask).Verifiable();


            _mockPurchaseCostReportVM = new Mock<PurchaseCostReportVM>(
                _mockProductSupplyHistoryService.Object,
                _mockProductService.Object,
                _mockCategoryService.Object,
                _mockSupplierService.Object
            );
            _mockPurchaseCostReportVM.Setup(vm => vm.LoadFiltersAsync()).Returns(Task.CompletedTask).Verifiable();


            _mockSupplierReportVM = new Mock<SupplierReportVM>(
                _mockProductSupplyHistoryService.Object,
                _mockSupplierService.Object
            );


            _mockPurchasePlanVM = new Mock<PurchasePlanVM>(
                _mockSupplierService.Object,
                _mockProductSupplyHistoryService.Object,
                _mockProductService.Object
            );
            _mockPurchasePlanVM.Setup(vm => vm.LoadSuppliersAsync()).Returns(Task.CompletedTask).Verifiable();

            _mockServiceProvider.Setup(sp => sp.GetService(typeof(ProductInputVM)))
                                .Returns(_mockProductInputVM.Object);
            _mockServiceProvider.Setup(sp => sp.GetService(typeof(ProductRemovalVM)))
                                .Returns(_mockProductRemovalVM.Object);
            _mockServiceProvider.Setup(sp => sp.GetService(typeof(StockReportVM)))
                                .Returns(_mockStockReportVM.Object);
            _mockServiceProvider.Setup(sp => sp.GetService(typeof(PurchaseCostReportVM)))
                                .Returns(_mockPurchaseCostReportVM.Object);
            _mockServiceProvider.Setup(sp => sp.GetService(typeof(SupplierReportVM)))
                                .Returns(_mockSupplierReportVM.Object);
            _mockServiceProvider.Setup(sp => sp.GetService(typeof(PurchasePlanVM)))
                                .Returns(_mockPurchasePlanVM.Object);
            _mockServiceProvider.Setup(sp => sp.GetService(typeof(IMessageService)))
                                .Returns(_mockMessageService.Object);

            _sut = new MainViewModel(_mockServiceProvider.Object);
        }

        [Fact]
        public void Constructor_InitializesProductInputVMAsCurrentViewModel()
        {
            Assert.NotNull(_sut.CurrentViewModel);
            Assert.IsAssignableFrom<ProductInputVM>(_sut.CurrentViewModel);
            Assert.Equal(_mockProductInputVM.Object, _sut.CurrentViewModel);
        }

        [Fact]
        public void NavigateToProductInputViewCommand_SetsCurrentViewModelToProductInputVM()
        {
            _sut.CurrentViewModel = _mockProductRemovalVM.Object;

            _sut.NavigateToProductInputViewCommand.Execute(null);

            Assert.NotNull(_sut.CurrentViewModel);
            Assert.IsAssignableFrom<ProductInputVM>(_sut.CurrentViewModel);
            Assert.Equal(_mockProductInputVM.Object, _sut.CurrentViewModel);
        }

        [Fact]
        public void NavigateToProductRemovalViewCommand_SetsCurrentViewModelToProductRemovalVM()
        {
            _sut.NavigateToProductRemovalViewCommand.Execute(null);

            Assert.NotNull(_sut.CurrentViewModel);
            Assert.IsAssignableFrom<ProductRemovalVM>(_sut.CurrentViewModel);
            Assert.Equal(_mockProductRemovalVM.Object, _sut.CurrentViewModel);
        }

        [Fact]
        public void NavigateToStockReportViewCommand_SetsCurrentViewModelToStockReportVM()
        {
            _sut.NavigateToStockReportViewCommand.Execute(null);

            Assert.NotNull(_sut.CurrentViewModel);
            Assert.IsAssignableFrom<StockReportVM>(_sut.CurrentViewModel);
            Assert.Equal(_mockStockReportVM.Object, _sut.CurrentViewModel);
        }

        [Fact]
        public void NavigateToPurchaseCostReportViewCommand_SetsCurrentViewModelToPurchaseCostReportVM()
        {
            _sut.NavigateToPurchaseCostReportViewCommand.Execute(null);

            Assert.NotNull(_sut.CurrentViewModel);
            Assert.IsAssignableFrom<PurchaseCostReportVM>(_sut.CurrentViewModel);
            Assert.Equal(_mockPurchaseCostReportVM.Object, _sut.CurrentViewModel);
        }

        [Fact]
        public void NavigateToSupplierReportViewCommand_SetsCurrentViewModelToSupplierReportVM()
        {
            _sut.NavigateToSupplierReportViewCommand.Execute(null);

            Assert.NotNull(_sut.CurrentViewModel);
            Assert.IsAssignableFrom<SupplierReportVM>(_sut.CurrentViewModel);
            Assert.Equal(_mockSupplierReportVM.Object, _sut.CurrentViewModel);
        }

        [Fact]
        public void NavigateToPurchasePlanViewCommand_SetsCurrentViewModelToPurchasePlanVM()
        {
            _sut.NavigateToPurchasePlanViewCommand.Execute(null);

            Assert.NotNull(_sut.CurrentViewModel);
            Assert.IsAssignableFrom<PurchasePlanVM>(_sut.CurrentViewModel);
            Assert.Equal(_mockPurchasePlanVM.Object, _sut.CurrentViewModel);
        }

        [Fact]
        public async Task NavigateToProductInputForEdit_SetsCurrentViewModelToProductInputVM()
        {
            int testProductId = 123;

            await _sut.NavigateToProductInputForEdit(testProductId);

            Assert.NotNull(_sut.CurrentViewModel);
            Assert.IsAssignableFrom<ProductInputVM>(_sut.CurrentViewModel);
            Assert.Equal(_mockProductInputVM.Object, _sut.CurrentViewModel);
        }

        [Fact]
        public void ExitApplicationCommand_IsNotNull()
        {
            Assert.NotNull(_sut.ExitApplicationCommand);
            Assert.True(_sut.ExitApplicationCommand.CanExecute(null));
        }

        [Fact]
        public void AllResetFormVerifications()
        {
            // Arrange
            _mockProductInputVM.Invocations.Clear();

            // Act
            var localSut = new MainViewModel(_mockServiceProvider.Object);
            localSut.NavigateToProductInputViewCommand.Execute(null);

            // Assert
            _mockProductInputVM.Verify(vm => vm.ResetForm(), Times.Exactly(2));
        }

        [Fact]
        public async Task LoadVerifications()
        {
            // Arrange
            _mockProductRemovalVM.Invocations.Clear();
            _mockStockReportVM.Invocations.Clear();
            _mockPurchaseCostReportVM.Invocations.Clear();
            _mockPurchasePlanVM.Invocations.Clear();
            _mockProductInputVM.Invocations.Clear();

            int testProductIdForEdit = 789;

            // Act
            _sut.NavigateToProductRemovalViewCommand.Execute(null);
            _sut.NavigateToStockReportViewCommand.Execute(null);
            _sut.NavigateToPurchaseCostReportViewCommand.Execute(null);
            _sut.NavigateToPurchasePlanViewCommand.Execute(null);
            await _sut.NavigateToProductInputForEdit(testProductIdForEdit);

            // Assert
            _mockProductInputVM.Verify(vm => vm.LoadProductForEdit(testProductIdForEdit), Times.Once);
        }
    }
}