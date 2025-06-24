using FoodServiceInventoryApp.Models;
using FoodServiceInventoryApp.Services;
using FoodServiceInventoryApp.ViewModels;
using Moq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using CommunityToolkit.Mvvm.Input;
using static FoodServiceInventoryApp.ViewModels.SupplierReportVM;

namespace FoodServiceInventoryApp.Tests
{
    public class SupplierReportVMTests
    {
        private readonly Mock<IProductSupplyHistoryService> _mockSupplyHistoryService;
        private readonly Mock<ISupplierService> _mockSupplierService;
        private readonly SupplierReportVM _sut;

        public SupplierReportVMTests()
        {
            _mockSupplyHistoryService = new Mock<IProductSupplyHistoryService>();
            _mockSupplierService = new Mock<ISupplierService>();
            _mockSupplyHistoryService.Setup(s => s.GetSupplyRecordsFilteredAsync(
                It.IsAny<DateTime?>(),
                It.IsAny<DateTime?>(),
                It.IsAny<int?>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .ReturnsAsync(new List<ProductSupplyHistory>());

            _mockSupplierService.Setup(s => s.GetSupplierByIdAsync(It.IsAny<int>()))
                .ReturnsAsync((Supplier)null);

            _sut = new SupplierReportVM(_mockSupplyHistoryService.Object, _mockSupplierService.Object);

            _sut.StartDate = new DateTime(2023, 1, 1);
            _sut.EndDate = new DateTime(2023, 1, 31);
        }

        [Fact]
        public async Task GenerateReportCommand_ShouldSetErrorMessage_WhenStartDateIsNull()
        {
            _sut.StartDate = null;
            _sut.EndDate = DateTime.Now;

            _sut.GenerateReportCommand.Execute(null);

            Assert.Equal("Пожалуйста, выберите корректный диапазон дат.", _sut.ErrorMessage);
            Assert.Empty(_sut.SupplierReportItems);

            _mockSupplyHistoryService.Verify(s => s.GetSupplyRecordsFilteredAsync(
                It.IsAny<DateTime?>(), It.IsAny<DateTime?>(), It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<int?>()),
                Times.Never);
        }

        [Fact]
        public async Task GenerateReportCommand_ShouldSetErrorMessage_WhenEndDateIsNull()
        {
            _sut.StartDate = DateTime.Now.AddMonths(-1);
            _sut.EndDate = null;

            _sut.GenerateReportCommand.Execute(null);

            Assert.Equal("Пожалуйста, выберите корректный диапазон дат.", _sut.ErrorMessage);
            Assert.Empty(_sut.SupplierReportItems);
            _mockSupplyHistoryService.Verify(s => s.GetSupplyRecordsFilteredAsync(
                It.IsAny<DateTime?>(), It.IsAny<DateTime?>(), It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<int?>()),
                Times.Never);
        }

        [Fact]
        public async Task GenerateReportCommand_ShouldSetErrorMessage_WhenStartDateIsAfterEndDate()
        {
            _sut.StartDate = new DateTime(2023, 2, 1);
            _sut.EndDate = new DateTime(2023, 1, 31);

            _sut.GenerateReportCommand.Execute(null);

            Assert.Equal("Пожалуйста, выберите корректный диапазон дат.", _sut.ErrorMessage);
            Assert.Empty(_sut.SupplierReportItems);
            _mockSupplyHistoryService.Verify(s => s.GetSupplyRecordsFilteredAsync(
                It.IsAny<DateTime?>(), It.IsAny<DateTime?>(), It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<int?>()),
                Times.Never);
        }

        [Fact]
        public async Task GenerateReportCommand_ShouldSetErrorMessage_WhenNoSuppliesFound()
        {
            _sut.StartDate = new DateTime(2023, 1, 1);
            _sut.EndDate = new DateTime(2023, 1, 31);

            _mockSupplyHistoryService.Setup(s => s.GetSupplyRecordsFilteredAsync(
                It.Is<DateTime?>(dt => dt.HasValue && dt.Value.Date == _sut.StartDate.Value.Date),
                It.Is<DateTime?>(dt => dt.HasValue && dt.Value.Date == _sut.EndDate.Value.Date),
                It.IsAny<int?>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .ReturnsAsync(new List<ProductSupplyHistory>());

            await ((AsyncRelayCommand)_sut.GenerateReportCommand).ExecuteAsync(null);

            Assert.Equal("Данные о поставках за выбранный период не найдены.", _sut.ErrorMessage);
            Assert.Empty(_sut.SupplierReportItems);

            _mockSupplyHistoryService.Verify(s => s.GetSupplyRecordsFilteredAsync(
                It.Is<DateTime?>(dt => dt.HasValue && dt.Value.Date == _sut.StartDate.Value.Date),
                It.Is<DateTime?>(dt => dt.HasValue && dt.Value.Date == _sut.EndDate.Value.Date),
                It.IsAny<int?>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()),
                Times.Once);
        }

        [Fact]
        public async Task GenerateReportCommand_ShouldSetErrorMessage_WhenSupplyServiceReturnsNull()
        {
            _sut.StartDate = new DateTime(2023, 1, 1);
            _sut.EndDate = new DateTime(2023, 1, 31);

            _mockSupplyHistoryService.Setup(s => s.GetSupplyRecordsFilteredAsync(
                It.Is<DateTime?>(dt => dt.HasValue && dt.Value.Date == _sut.StartDate.Value.Date),
                It.Is<DateTime?>(dt => dt.HasValue && dt.Value.Date == _sut.EndDate.Value.Date),
                It.IsAny<int?>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .ReturnsAsync((List<ProductSupplyHistory>)null);

            await ((AsyncRelayCommand)_sut.GenerateReportCommand).ExecuteAsync(null);

            Assert.Equal("Данные о поставках за выбранный период не найдены.", _sut.ErrorMessage);
            Assert.Empty(_sut.SupplierReportItems);
            _mockSupplyHistoryService.Verify(s => s.GetSupplyRecordsFilteredAsync(
                It.Is<DateTime?>(dt => dt.HasValue && dt.Value.Date == _sut.StartDate.Value.Date),
                It.Is<DateTime?>(dt => dt.HasValue && dt.Value.Date == _sut.EndDate.Value.Date),
                It.IsAny<int?>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()),
                Times.Once);
        }

        [Fact]
        public async Task GenerateReportCommand_ShouldGenerateReportCorrectly_SingleSupplier()
        {
            _sut.StartDate = new DateTime(2023, 1, 1);
            _sut.EndDate = new DateTime(2023, 1, 31);

            var supplier1 = new Supplier { SupplierId = 1, CompanyName = "Поставщик А" };
            var supplies = new List<ProductSupplyHistory>
            {
                new ProductSupplyHistory { SupplierId = 1, SuppliedQuantity = 10, SupplyUnitPrice = 5.0m },
                new ProductSupplyHistory { SupplierId = 1, SuppliedQuantity = 20, SupplyUnitPrice = 2.5m }
            };

            _mockSupplyHistoryService.Setup(s => s.GetSupplyRecordsFilteredAsync(
                It.Is<DateTime?>(dt => dt.HasValue && dt.Value.Date == _sut.StartDate.Value.Date),
                It.Is<DateTime?>(dt => dt.HasValue && dt.Value.Date == _sut.EndDate.Value.Date),
                It.IsAny<int?>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .ReturnsAsync(supplies);
            _mockSupplierService.Setup(s => s.GetSupplierByIdAsync(1)).ReturnsAsync(supplier1);

            await ((AsyncRelayCommand)_sut.GenerateReportCommand).ExecuteAsync(null);

            Assert.Empty(_sut.ErrorMessage);
            Assert.Single(_sut.SupplierReportItems);
            var reportItem = _sut.SupplierReportItems.First();
            Assert.Equal("Поставщик А", reportItem.SupplierName);
            Assert.Equal(10 * 5.0m + 20 * 2.5m, reportItem.TotalPaid);
            _mockSupplyHistoryService.Verify(s => s.GetSupplyRecordsFilteredAsync(
                It.Is<DateTime?>(dt => dt.HasValue && dt.Value.Date == _sut.StartDate.Value.Date),
                It.Is<DateTime?>(dt => dt.HasValue && dt.Value.Date == _sut.EndDate.Value.Date),
                It.IsAny<int?>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()),
                Times.Once);
            _mockSupplierService.Verify(s => s.GetSupplierByIdAsync(1), Times.Once);
        }

        [Fact]
        public async Task GenerateReportCommand_ShouldGenerateReportCorrectly_MultipleSuppliers()
        {
            _sut.StartDate = new DateTime(2023, 1, 1);
            _sut.EndDate = new DateTime(2023, 1, 31);

            var supplier1 = new Supplier { SupplierId = 1, CompanyName = "Поставщик Б" };
            var supplier2 = new Supplier { SupplierId = 2, CompanyName = "Поставщик А" };

            var supplies = new List<ProductSupplyHistory>
            {
                new ProductSupplyHistory { SupplierId = 1, SuppliedQuantity = 10, SupplyUnitPrice = 5.0m },
                new ProductSupplyHistory { SupplierId = 2, SuppliedQuantity = 5, SupplyUnitPrice = 10.0m },
                new ProductSupplyHistory { SupplierId = 1, SuppliedQuantity = 20, SupplyUnitPrice = 2.0m },
                new ProductSupplyHistory { SupplierId = 2, SuppliedQuantity = 1, SupplyUnitPrice = 25.0m }
            };
            _mockSupplyHistoryService.Setup(s => s.GetSupplyRecordsFilteredAsync(
                It.Is<DateTime?>(dt => dt.HasValue && dt.Value.Date == _sut.StartDate.Value.Date),
                It.Is<DateTime?>(dt => dt.HasValue && dt.Value.Date == _sut.EndDate.Value.Date),
                It.IsAny<int?>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .ReturnsAsync(supplies);
            _mockSupplierService.Setup(s => s.GetSupplierByIdAsync(1)).ReturnsAsync(supplier1);
            _mockSupplierService.Setup(s => s.GetSupplierByIdAsync(2)).ReturnsAsync(supplier2);

            await ((AsyncRelayCommand)_sut.GenerateReportCommand).ExecuteAsync(null);

            Assert.Empty(_sut.ErrorMessage);
            Assert.Equal(2, _sut.SupplierReportItems.Count);

            Assert.Equal("Поставщик А", _sut.SupplierReportItems[0].SupplierName);
            Assert.Equal(75.0m, _sut.SupplierReportItems[0].TotalPaid);

            Assert.Equal("Поставщик Б", _sut.SupplierReportItems[1].SupplierName);
            Assert.Equal(90.0m, _sut.SupplierReportItems[1].TotalPaid);

            _mockSupplyHistoryService.Verify(s => s.GetSupplyRecordsFilteredAsync(
                It.Is<DateTime?>(dt => dt.HasValue && dt.Value.Date == _sut.StartDate.Value.Date),
                It.Is<DateTime?>(dt => dt.HasValue && dt.Value.Date == _sut.EndDate.Value.Date),
                It.IsAny<int?>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()),
                Times.Once);
            _mockSupplierService.Verify(s => s.GetSupplierByIdAsync(1), Times.Once);
            _mockSupplierService.Verify(s => s.GetSupplierByIdAsync(2), Times.Once);
        }

        [Fact]
        public async Task GenerateReportCommand_ShouldHandleSupplierNotFound()
        {
            // Arrange
            _sut.StartDate = new DateTime(2023, 1, 1);
            _sut.EndDate = new DateTime(2023, 1, 31);

            var supplies = new List<ProductSupplyHistory>
            {
                new ProductSupplyHistory { SupplierId = 999, SuppliedQuantity = 10, SupplyUnitPrice = 5.0m }
            };

            _mockSupplyHistoryService.Setup(s => s.GetSupplyRecordsFilteredAsync(
                It.Is<DateTime?>(dt => dt.HasValue && dt.Value.Date == _sut.StartDate.Value.Date),
                It.Is<DateTime?>(dt => dt.HasValue && dt.Value.Date == _sut.EndDate.Value.Date),
                It.IsAny<int?>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .ReturnsAsync(supplies);
            _mockSupplierService.Setup(s => s.GetSupplierByIdAsync(999)).ReturnsAsync((Supplier)null);

            await ((AsyncRelayCommand)_sut.GenerateReportCommand).ExecuteAsync(null);

            Assert.Empty(_sut.ErrorMessage);
            Assert.Single(_sut.SupplierReportItems);
            var reportItem = _sut.SupplierReportItems.First();
            Assert.Equal("Неизвестный поставщик", reportItem.SupplierName);
            Assert.Equal(50.0m, reportItem.TotalPaid);
            _mockSupplyHistoryService.Verify(s => s.GetSupplyRecordsFilteredAsync(
                It.Is<DateTime?>(dt => dt.HasValue && dt.Value.Date == _sut.StartDate.Value.Date),
                It.Is<DateTime?>(dt => dt.HasValue && dt.Value.Date == _sut.EndDate.Value.Date),
                It.IsAny<int?>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()),
                Times.Once);
            _mockSupplierService.Verify(s => s.GetSupplierByIdAsync(999), Times.Once);
        }

        [Fact]
        public async Task GenerateReportCommand_ShouldClearPreviousReportAndErrors()
        {
            _sut.ErrorMessage = "Old error message";
            _sut.SupplierReportItems.Add(new SupplierReportItem { SupplierName = "Old Supplier", TotalPaid = 100 });

            _sut.StartDate = new DateTime(2023, 1, 1);
            _sut.EndDate = new DateTime(2023, 1, 31);
            var supplies = new List<ProductSupplyHistory>
            {
                new ProductSupplyHistory { SupplierId = 1, SuppliedQuantity = 1, SupplyUnitPrice = 1.0m }
            };
            _mockSupplyHistoryService.Setup(s => s.GetSupplyRecordsFilteredAsync(
                It.Is<DateTime?>(dt => dt.HasValue && dt.Value.Date == _sut.StartDate.Value.Date),
                It.Is<DateTime?>(dt => dt.HasValue && dt.Value.Date == _sut.EndDate.Value.Date),
                It.IsAny<int?>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .ReturnsAsync(supplies);
            _mockSupplierService.Setup(s => s.GetSupplierByIdAsync(1)).ReturnsAsync(new Supplier { SupplierId = 1, CompanyName = "New Supplier" });

            await ((AsyncRelayCommand)_sut.GenerateReportCommand).ExecuteAsync(null);

            Assert.Empty(_sut.ErrorMessage);
            Assert.Single(_sut.SupplierReportItems);
            Assert.Equal("New Supplier", _sut.SupplierReportItems.First().SupplierName);
        }
    }
}