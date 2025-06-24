using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FoodServiceInventoryApp.Models;
using FoodServiceInventoryApp.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
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

        private readonly ISupplierService _supplierService;
        private readonly IProductSupplyHistoryService _productSupplyHistoryService;
        private readonly IProductService _productService;

        [ObservableProperty]
        private ObservableCollection<Supplier> _suppliers;
        [ObservableProperty]
        private Supplier _selectedSupplierFilter;

        [ObservableProperty]
        private ObservableCollection<PurchasePlanItem> _purchasePlanItems;

        [ObservableProperty]
        private string _errorMessage;

        public ICommand GeneratePlanCommand { get; }
        public IAsyncRelayCommand LoadSuppliersCommand { get; }

        public PurchasePlanVM(ISupplierService supplierService, IProductSupplyHistoryService productSupplyHistoryService, IProductService productService)
        {
            _supplierService = supplierService;
            _productSupplyHistoryService = productSupplyHistoryService;
            _productService = productService;

            Suppliers = new ObservableCollection<Supplier>();
            PurchasePlanItems = new ObservableCollection<PurchasePlanItem>();
            ErrorMessage = string.Empty;

            GeneratePlanCommand = new AsyncRelayCommand(ExecuteGeneratePlanAsync);
            LoadSuppliersCommand = new AsyncRelayCommand(LoadSuppliersAsync);

            LoadSuppliersCommand.Execute(null);
        }

        protected internal virtual async Task LoadSuppliersAsync()
        {
            Suppliers.Clear();
            Suppliers.Add(new Supplier { SupplierId = 0, CompanyName = "Все" });

            var allSuppliers = await _supplierService.GetAllSuppliersAsync();

            if (allSuppliers != null)
            {
                foreach (var s in allSuppliers.OrderBy(s => s.CompanyName))
                {
                    Suppliers.Add(s);
                }
            }
            else
            {
                Debug.WriteLine("Предупреждение: _supplierService.GetAllSuppliersAsync() вернул null.");
            }

            SelectedSupplierFilter = Suppliers.FirstOrDefault();
        }

        private async Task ExecuteGeneratePlanAsync()
        {
            ErrorMessage = string.Empty;
            PurchasePlanItems.Clear();

            Debug.WriteLine("--- Начинаем формирование плана закупок ---");

            DateTime endDate = DateTime.Now.Date.AddDays(1).AddTicks(-1);
            DateTime startDate = endDate.AddMonths(-1).Date;

            Debug.WriteLine($"Период анализа: с {startDate.ToShortDateString()} по {endDate.ToShortDateString()}");

            var allSupplyHistory = await _productSupplyHistoryService.GetSupplyRecordsFilteredAsync(
                startDate: startDate,
                endDate: endDate
            );

            if (allSupplyHistory == null || !allSupplyHistory.Any())
            {
                ErrorMessage = "Данные о прошлых поставках для расчета плана не найдены. Убедитесь, что в БД есть записи о поставках за последний месяц.";
                Debug.WriteLine("Ошибка: Нет данных о прошлых поставках.");
                return;
            }
            Debug.WriteLine($"Найдено {allSupplyHistory.Count()} записей истории поставок.");

            var monthlyUsage = allSupplyHistory
                .GroupBy(s => new { s.ProductId, s.SupplierId })
                .Select(g => new
                {
                    ProductId = g.Key.ProductId,
                    SupplierId = g.Key.SupplierId,
                    TotalSuppliedQuantity = g.Sum(x => x.SuppliedQuantity),
                    AverageUnitPrice = g.Average(x => x.SupplyUnitPrice)
                }).ToList();

            Debug.WriteLine($"Сгруппировано {monthlyUsage.Count} уникальных комбинаций Продукт/Поставщик.");

            var currentStock = (await _productService.GetAllProductsAsync())?.ToList();

            if (currentStock == null || !currentStock.Any())
            {
                ErrorMessage = "Данные о текущих остатках продуктов не найдены. Убедитесь, что в БД есть продукты.";
                Debug.WriteLine("Ошибка: Нет данных о текущих остатках продуктов.");
                return;
            }
            Debug.WriteLine($"Найдено {currentStock.Count} продуктов в текущем остатке.");

            var planItems = new List<PurchasePlanItem>();

            foreach (var usage in monthlyUsage)
            {
                var supplier = await _supplierService.GetSupplierByIdAsync(usage.SupplierId);
                var product = await _productService.GetProductByIdAsync(usage.ProductId);

                if (supplier == null || product == null)
                {
                    Debug.WriteLine($"Пропуск записи: не удалось найти поставщика (ID: {usage.SupplierId}) или продукт (ID: {usage.ProductId}).");
                    continue;
                }

                if (SelectedSupplierFilter?.SupplierId != 0 && usage.SupplierId != SelectedSupplierFilter?.SupplierId)
                {
                    Debug.WriteLine($"Пропуск записи для {product.ProductName} от {supplier.CompanyName}: не соответствует выбранному фильтру поставщика.");
                    continue;
                }

                var currentProductStock = currentStock.FirstOrDefault(s => s.ProductId == product.ProductId);

                if (currentProductStock != null)
                {
                    decimal recommendedQuantity = usage.TotalSuppliedQuantity;

                    if (currentProductStock.Quantity < usage.TotalSuppliedQuantity)
                    {
                        recommendedQuantity = (usage.TotalSuppliedQuantity * 1.5M) - currentProductStock.Quantity;
                    }

                    if (recommendedQuantity < 1M && usage.TotalSuppliedQuantity > 0)
                    {
                        recommendedQuantity = 1M;
                    }
                    else if (recommendedQuantity < 0.1M)
                    {
                        recommendedQuantity = 0;
                    }

                    if (recommendedQuantity > 0)
                    {
                        planItems.Add(new PurchasePlanItem
                        {
                            SupplierName = supplier.CompanyName,
                            ProductName = product.ProductName,
                            RecommendedQuantity = recommendedQuantity,
                            UnitOfMeasure = product.UnitOfMeasure,
                            EstimatedCost = recommendedQuantity * usage.AverageUnitPrice
                        });
                        Debug.WriteLine($"Добавлен в план: {product.ProductName} ({recommendedQuantity:N2} {product.UnitOfMeasure}) от {supplier.CompanyName}. Расчетная стоимость: {recommendedQuantity * usage.AverageUnitPrice:C}");
                    }
                    else
                    {
                        Debug.WriteLine($"Пропуск {product.ProductName} от {supplier.CompanyName}: рекомендованное количество <= 0 (текущий запас {currentProductStock.Quantity:N2}, потребление {usage.TotalSuppliedQuantity:N2})");
                    }
                }
                else
                {
                    Debug.WriteLine($"Пропуск {product.ProductName}: не найдено текущих данных по остаткам (ProductId: {product.ProductId}).");
                }
            }

            PurchasePlanItems = new ObservableCollection<PurchasePlanItem>(planItems.OrderBy(p => p.SupplierName).ThenBy(p => p.ProductName));

            if (PurchasePlanItems.Count == 0)
            {
                ErrorMessage = "План закупок для выбранных критериев не сформирован. Возможно, запасы достаточны, нет данных о потреблении или не удалось найти связанные продукты/поставщиков.";
                Debug.WriteLine("Завершено: План закупок пуст.");
            }
            else
            {
                Debug.WriteLine($"Завершено: Сформировано {PurchasePlanItems.Count} позиций в плане закупок.");
            }
        }
    }
}