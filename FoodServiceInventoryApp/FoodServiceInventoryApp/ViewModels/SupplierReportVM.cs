using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
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

        [ObservableProperty]
        private DateTime? _startDate = DateTime.Now.AddMonths(-1);

        [ObservableProperty]
        private DateTime? _endDate = DateTime.Now;

        [ObservableProperty]
        private ObservableCollection<SupplierReportItem> _supplierReportItems;

        [ObservableProperty]
        private string _errorMessage;

        public ICommand GenerateReportCommand { get; }

        public SupplierReportVM()
        {
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

            var dummySupplyHistory = new ObservableCollection<PurchaseCostReportVM.ReportDetailItem>
            {
                new PurchaseCostReportVM.ReportDetailItem { SupplyDate = new DateTime(2025, 6, 5), ProductName = "Молоко", SupplierName = "Поставщик А", SuppliedQuantity = 5.0M, SupplyUnitPrice = 1.2M },
                new PurchaseCostReportVM.ReportDetailItem { SupplyDate = new DateTime(2025, 6, 10), ProductName = "Хлеб", SupplierName = "Поставщик Б", SuppliedQuantity = 10.0M, SupplyUnitPrice = 0.8M },
                new PurchaseCostReportVM.ReportDetailItem { SupplyDate = new DateTime(2025, 5, 15), ProductName = "Мука", SupplierName = "Поставщик А", SuppliedQuantity = 20.0M, SupplyUnitPrice = 0.9M },
                new PurchaseCostReportVM.ReportDetailItem { SupplyDate = new DateTime(2025, 6, 20), ProductName = "Салфетки", SupplierName = "Поставщик Б", SuppliedQuantity = 50.0M, SupplyUnitPrice = 0.03M },
                new PurchaseCostReportVM.ReportDetailItem { SupplyDate = new DateTime(2025, 5, 25), ProductName = "Масло", SupplierName = "Поставщик А", SuppliedQuantity = 8.0M, SupplyUnitPrice = 2.5M }
            };

            var filteredSupplies = dummySupplyHistory.Where(s => s.SupplyDate >= StartDate.Value && s.SupplyDate <= EndDate.Value);

            var groupedBySupplier = filteredSupplies
                .GroupBy(s => s.SupplierName)
                .Select(g => new SupplierReportItem
                {
                    SupplierName = g.Key,
                    TotalPaid = g.Sum(item => item.TotalItemCost)
                })
                .OrderBy(s => s.SupplierName)
                .ToList();

            SupplierReportItems = new ObservableCollection<SupplierReportItem>(groupedBySupplier);

            if (SupplierReportItems.Count == 0)
            {
                ErrorMessage = "Данные для выбранного периода не найдены.";
            }
        }
    }
}