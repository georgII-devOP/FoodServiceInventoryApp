using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FoodServiceInventoryApp.Models;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;

namespace FoodServiceInventoryApp.ViewModels
{
    public partial class PurchaseCostReportVM : ObservableObject
    {
        public class ReportDetailItem : ObservableObject
        {
            public DateTime SupplyDate { get; set; }
            public string ProductName { get; set; }
            public string SupplierName { get; set; }
            public decimal SuppliedQuantity { get; set; }
            public decimal SupplyUnitPrice { get; set; }
            public decimal TotalItemCost => SuppliedQuantity * SupplyUnitPrice;
        }

        [ObservableProperty]
        private ObservableCollection<string> _months;
        [ObservableProperty]
        private string _selectedMonth;

        [ObservableProperty]
        private ObservableCollection<int> _years;
        [ObservableProperty]
        private int _selectedYear;

        [ObservableProperty]
        private ObservableCollection<string> _products;
        [ObservableProperty]
        private string _selectedProductFilter;

        [ObservableProperty]
        private ObservableCollection<string> _suppliers;
        [ObservableProperty]
        private string _selectedSupplierFilter;

        [ObservableProperty]
        private ObservableCollection<string> _categories;
        [ObservableProperty]
        private string _selectedCategoryFilter;

        [ObservableProperty]
        private decimal _totalCost;

        [ObservableProperty]
        private ObservableCollection<ReportDetailItem> _reportDetails;

        [ObservableProperty]
        private string _errorMessage;

        public ICommand GenerateReportCommand { get; }

        public PurchaseCostReportVM()
        {
            Months = new ObservableCollection<string>(Enumerable.Range(1, 12).Select(m => new DateTime(DateTime.Now.Year, m, 1).ToString("MMMM")));
            SelectedMonth = Months[DateTime.Now.Month - 1];

            Years = new ObservableCollection<int>(Enumerable.Range(DateTime.Now.Year - 2, 5));
            SelectedYear = DateTime.Now.Year;

            Products = new ObservableCollection<string> { "Все", "Молоко", "Хлеб", "Мука" };
            SelectedProductFilter = "Все";

            Suppliers = new ObservableCollection<string> { "Все", "Поставщик А", "Поставщик Б" };
            SelectedSupplierFilter = "Все";

            Categories = new ObservableCollection<string> { "Все", "Продукты", "Принадлежности" };
            SelectedCategoryFilter = "Все";

            ReportDetails = new ObservableCollection<ReportDetailItem>();

            GenerateReportCommand = new AsyncRelayCommand(ExecuteGenerateReportAsync);
        }

        private async Task ExecuteGenerateReportAsync()
        {
            ErrorMessage = string.Empty;
            TotalCost = 0;
            ReportDetails.Clear();

            var dummySupplyHistory = new ObservableCollection<ReportDetailItem>
            {
                new ReportDetailItem { SupplyDate = new DateTime(2025, 6, 5), ProductName = "Молоко", SupplierName = "Поставщик А", SuppliedQuantity = 5.0M, SupplyUnitPrice = 1.2M },
                new ReportDetailItem { SupplyDate = new DateTime(2025, 6, 10), ProductName = "Хлеб", SupplierName = "Поставщик Б", SuppliedQuantity = 10.0M, SupplyUnitPrice = 0.8M },
                new ReportDetailItem { SupplyDate = new DateTime(2025, 5, 15), ProductName = "Мука", SupplierName = "Поставщик А", SuppliedQuantity = 20.0M, SupplyUnitPrice = 0.9M },
                new ReportDetailItem { SupplyDate = new DateTime(2025, 6, 20), ProductName = "Салфетки", SupplierName = "Поставщик Б", SuppliedQuantity = 50.0M, SupplyUnitPrice = 0.03M }
            };

            var filteredDetails = dummySupplyHistory.Where(item =>
            {
                bool monthMatch = (item.SupplyDate.Month == (Months.IndexOf(SelectedMonth) + 1));
                bool yearMatch = (item.SupplyDate.Year == SelectedYear);
                bool productMatch = (SelectedProductFilter == "Все" || item.ProductName == SelectedProductFilter);
                bool supplierMatch = (SelectedSupplierFilter == "Все" || item.SupplierName == SelectedSupplierFilter);
                return monthMatch && yearMatch && productMatch && supplierMatch;
            }).ToList();

            ReportDetails = new ObservableCollection<ReportDetailItem>(filteredDetails);
            TotalCost = ReportDetails.Sum(item => item.TotalItemCost);

            if (ReportDetails.Count == 0)
            {
                ErrorMessage = "Данные для выбранных критериев не найдены.";
            }
        }
    }
}