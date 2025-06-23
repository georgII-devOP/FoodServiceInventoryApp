using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FoodServiceInventoryApp.Models;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace FoodServiceInventoryApp.ViewModels
{
    public partial class PurchasePlanVM : ObservableObject
    {
        public class PurchasePlanItem : ObservableObject
        {
            public string SupplierName { get; set; }
            public string ProductName { get; set; }
            public decimal RecommendedQuantity { get; set; }
            public string UnitOfMeasure { get; set; }
            public decimal EstimatedCost { get; set; }
        }

        [ObservableProperty]
        private ObservableCollection<string> _suppliers;
        [ObservableProperty]
        private string _selectedSupplierFilter;

        [ObservableProperty]
        private ObservableCollection<PurchasePlanItem> _purchasePlanItems;

        [ObservableProperty]
        private string _errorMessage;

        public ICommand GeneratePlanCommand { get; }

        public PurchasePlanVM()
        {
            Suppliers = new ObservableCollection<string> { "Все", "Поставщик А", "Поставщик Б" };
            SelectedSupplierFilter = "Все";
            PurchasePlanItems = new ObservableCollection<PurchasePlanItem>();
            GeneratePlanCommand = new AsyncRelayCommand(ExecuteGeneratePlanAsync);
        }

        private async Task ExecuteGeneratePlanAsync()
        {
            ErrorMessage = string.Empty;
            PurchasePlanItems.Clear();

            var productUsageData = new List<PurchaseCostReportVM.ReportDetailItem>
            {
                new PurchaseCostReportVM.ReportDetailItem { SupplyDate = DateTime.Now.AddMonths(-1).AddDays(5), ProductName = "Молоко", SupplierName = "Поставщик А", SuppliedQuantity = 5.0M, SupplyUnitPrice = 1.2M },
                new PurchaseCostReportVM.ReportDetailItem { SupplyDate = DateTime.Now.AddMonths(-1).AddDays(10), ProductName = "Молоко", SupplierName = "Поставщик А", SuppliedQuantity = 7.0M, SupplyUnitPrice = 1.2M },
                new PurchaseCostReportVM.ReportDetailItem { SupplyDate = DateTime.Now.AddMonths(-1).AddDays(15), ProductName = "Хлеб", SupplierName = "Поставщик Б", SuppliedQuantity = 10.0M, SupplyUnitPrice = 0.8M },
                new PurchaseCostReportVM.ReportDetailItem { SupplyDate = DateTime.Now.AddMonths(-1).AddDays(20), ProductName = "Хлеб", SupplierName = "Поставщик Б", SuppliedQuantity = 8.0M, SupplyUnitPrice = 0.8M },
                new PurchaseCostReportVM.ReportDetailItem { SupplyDate = DateTime.Now.AddMonths(-1).AddDays(25), ProductName = "Мука", SupplierName = "Поставщик А", SuppliedQuantity = 20.0M, SupplyUnitPrice = 0.9M },
                new PurchaseCostReportVM.ReportDetailItem { SupplyDate = DateTime.Now.AddMonths(-1).AddDays(28), ProductName = "Салфетки", SupplierName = "Поставщик Б", SuppliedQuantity = 50.0M, SupplyUnitPrice = 0.03M }
            };

            var currentStock = new List<Product>
            {
                new Product { ProductName = "Молоко", Quantity = 2.0M, UnitOfMeasure = "литр", UnitPrice = 1.2M, LastSupplyDate = DateTime.Now, Category = new Models.Category{CategoryName="Продукты"}},
                new Product { ProductName = "Хлеб", Quantity = 3.0M, UnitOfMeasure = "буханка", UnitPrice = 0.8M, LastSupplyDate = DateTime.Now, Category = new Models.Category{CategoryName="Продукты"}},
                new Product { ProductName = "Мука", Quantity = 5.0M, UnitOfMeasure = "кг", UnitPrice = 0.9M, LastSupplyDate = DateTime.Now, Category = new Models.Category{CategoryName="Продукты"}},
                new Product { ProductName = "Салфетки", Quantity = 10.0M, UnitOfMeasure = "шт.", UnitPrice = 0.03M, LastSupplyDate = DateTime.Now, Category = new Models.Category{CategoryName="Принадлежности"}}
            };

            var monthlyUsage = productUsageData
                .Where(u => u.SupplyDate >= DateTime.Now.AddMonths(-1) && u.SupplyDate <= DateTime.Now)
                .GroupBy(u => new { u.ProductName, u.SupplierName })
                .Select(g => new
                {
                    g.Key.ProductName,
                    g.Key.SupplierName,
                    TotalSuppliedQuantity = g.Sum(x => x.SuppliedQuantity),
                    AverageUnitPrice = g.Average(x => x.SupplyUnitPrice)
                }).ToList();

            var planItems = new List<PurchasePlanItem>();

            foreach (var usage in monthlyUsage)
            {
                if (SelectedSupplierFilter != "Все" && usage.SupplierName != SelectedSupplierFilter)
                {
                    continue;
                }

                var currentProductStock = currentStock.FirstOrDefault(s => s.ProductName == usage.ProductName);
                if (currentProductStock != null)
                {
                    decimal recommendedQuantity = 0;
                    if (currentProductStock.Quantity < usage.TotalSuppliedQuantity)
                    {
                        recommendedQuantity = (usage.TotalSuppliedQuantity * 1.5M) - currentProductStock.Quantity;
                    }

                    if (recommendedQuantity > 0)
                    {
                        planItems.Add(new PurchasePlanItem
                        {
                            SupplierName = usage.SupplierName,
                            ProductName = usage.ProductName,
                            RecommendedQuantity = recommendedQuantity,
                            UnitOfMeasure = currentProductStock.UnitOfMeasure,
                            EstimatedCost = recommendedQuantity * usage.AverageUnitPrice
                        });
                    }
                }
            }

            PurchasePlanItems = new ObservableCollection<PurchasePlanItem>(planItems.OrderBy(p => p.SupplierName).ThenBy(p => p.ProductName));

            if (PurchasePlanItems.Count == 0)
            {
                ErrorMessage = "План закупок для выбранных критериев не сформирован. Возможно, запасы достаточны или нет данных о потреблении.";
            }
        }
    }
}