using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FoodServiceInventoryApp.Services;
using FoodServiceInventoryApp.Models;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace FoodServiceInventoryApp.ViewModels
{
    public partial class SupplierReportVM : ObservableObject
    {
        public class SupplierReportItem : ObservableObject
        {
            public string SupplierName { get; set; }
            public decimal TotalPaid { get; set; }
        }

        private readonly IProductSupplyHistoryService _supplyHistoryService;
        private readonly ISupplierService _supplierService;

        [ObservableProperty]
        private DateTime? _startDate = DateTime.Now.AddMonths(-1);

        [ObservableProperty]
        private DateTime? _endDate = DateTime.Now;

        [ObservableProperty]
        private ObservableCollection<SupplierReportItem> _supplierReportItems;

        [ObservableProperty]
        private string _errorMessage;

        public ICommand GenerateReportCommand { get; }

        public SupplierReportVM(IProductSupplyHistoryService supplyHistoryService, ISupplierService supplierService)
        {
            _supplyHistoryService = supplyHistoryService;
            _supplierService = supplierService;
            SupplierReportItems = new ObservableCollection<SupplierReportItem>();
            GenerateReportCommand = new AsyncRelayCommand(ExecuteGenerateReportAsync);
        }

        private async Task ExecuteGenerateReportAsync()
        {
            ErrorMessage = string.Empty;
            SupplierReportItems.Clear();

            if (!StartDate.HasValue || !EndDate.HasValue || StartDate.Value > EndDate.Value)
            {
                ErrorMessage = "Пожалуйста, выберите корректный диапазон дат.";
                return;
            }

            var supplies = await _supplyHistoryService.GetSupplyRecordsFilteredAsync(
                startDate: StartDate.Value,
                endDate: EndDate.Value
            );

            if (supplies == null || !supplies.Any())
            {
                ErrorMessage = "Данные о поставках за выбранный период не найдены.";
                return;
            }

            var groupedBySupplierId = supplies
                .GroupBy(s => s.SupplierId)
                .Select(g => new
                {
                    SupplierId = g.Key,
                    TotalPaid = g.Sum(item => item.SuppliedQuantity * item.SupplyUnitPrice)
                })
                .ToList();

            var reportItems = new ObservableCollection<SupplierReportItem>();
            foreach (var group in groupedBySupplierId.OrderBy(g => g.SupplierId))
            {
                var supplier = await _supplierService.GetSupplierByIdAsync(group.SupplierId);
                reportItems.Add(new SupplierReportItem
                {
                    SupplierName = supplier?.CompanyName ?? "Неизвестный поставщик",
                    TotalPaid = group.TotalPaid
                });
            }

            SupplierReportItems = new ObservableCollection<SupplierReportItem>(reportItems.OrderBy(s => s.SupplierName));


            if (SupplierReportItems.Count == 0)
            {
                ErrorMessage = "Данные для выбранного периода не найдены.";
            }
        }
    }
}