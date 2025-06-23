using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FoodServiceInventoryApp.Models;
using FoodServiceInventoryApp.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

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

        private readonly IProductSupplyHistoryService _supplyHistoryService;
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;
        private readonly ISupplierService _supplierService;

        [ObservableProperty]
        private ObservableCollection<string> _months;
        [ObservableProperty]
        private string _selectedMonth;

        [ObservableProperty]
        private ObservableCollection<int> _years;
        [ObservableProperty]
        private int _selectedYear;

        [ObservableProperty]
        private ObservableCollection<Product> _productsFilter;
        [ObservableProperty]
        private Product _selectedProductFilter;

        [ObservableProperty]
        private ObservableCollection<Supplier> _suppliersFilter;
        [ObservableProperty]
        private Supplier _selectedSupplierFilter;

        [ObservableProperty]
        private ObservableCollection<Category> _categoriesFilter;
        [ObservableProperty]
        private Category _selectedCategoryFilter;

        [ObservableProperty]
        private decimal _totalCost;

        [ObservableProperty]
        private ObservableCollection<ReportDetailItem> _reportDetails;

        [ObservableProperty]
        private string _errorMessage;

        public ICommand GenerateReportCommand { get; }
        public IAsyncRelayCommand LoadFiltersCommand { get; }

        public PurchaseCostReportVM(IProductSupplyHistoryService supplyHistoryService, IProductService productService, ICategoryService categoryService, ISupplierService supplierService)
        {
            _supplyHistoryService = supplyHistoryService;
            _productService = productService;
            _categoryService = categoryService;
            _supplierService = supplierService;

            Months = new ObservableCollection<string>(Enumerable.Range(1, 12).Select(m => new DateTime(DateTime.Now.Year, m, 1).ToString("MMMM")));
            SelectedMonth = Months[DateTime.Now.Month - 1];

            Years = new ObservableCollection<int>(Enumerable.Range(DateTime.Now.Year - 2, 5).OrderByDescending(y => y));
            SelectedYear = DateTime.Now.Year;

            ProductsFilter = new ObservableCollection<Product>();
            SuppliersFilter = new ObservableCollection<Supplier>();
            CategoriesFilter = new ObservableCollection<Category>();

            ReportDetails = new ObservableCollection<ReportDetailItem>();
            ErrorMessage = string.Empty;

            GenerateReportCommand = new AsyncRelayCommand(ExecuteGenerateReportAsync);
            LoadFiltersCommand = new AsyncRelayCommand(LoadFiltersAsync);

            LoadFiltersCommand.Execute(null);
        }

        private async Task LoadFiltersAsync()
        {
            ProductsFilter.Clear();
            ProductsFilter.Add(new Product { ProductId = 0, ProductName = "Все" });
            var allProducts = await _productService.GetAllProductsAsync();
            foreach (var p in allProducts.OrderBy(p => p.ProductName))
            {
                ProductsFilter.Add(p);
            }
            SelectedProductFilter = ProductsFilter.FirstOrDefault();


            SuppliersFilter.Clear();
            SuppliersFilter.Add(new Supplier { SupplierId = 0, CompanyName = "Все" });
            var allSuppliers = await _supplierService.GetAllSuppliersAsync();
            foreach (var s in allSuppliers.OrderBy(s => s.CompanyName))
            {
                SuppliersFilter.Add(s);
            }
            SelectedSupplierFilter = SuppliersFilter.FirstOrDefault();

            CategoriesFilter.Clear();
            CategoriesFilter.Add(new Category { CategoryId = 0, CategoryName = "Все" });
            var allCategories = await _categoryService.GetAllCategoriesAsync();
            foreach (var c in allCategories.OrderBy(c => c.CategoryName))
            {
                CategoriesFilter.Add(c);
            }
            SelectedCategoryFilter = CategoriesFilter.FirstOrDefault();
        }

        private async Task ExecuteGenerateReportAsync()
        {
            ErrorMessage = string.Empty;
            TotalCost = 0;
            ReportDetails.Clear();

            int monthIndex = Months.IndexOf(SelectedMonth) + 1;
            DateTime startDate = new DateTime(SelectedYear, monthIndex, 1);
            DateTime endDate = new DateTime(SelectedYear, monthIndex, DateTime.DaysInMonth(SelectedYear, monthIndex), 23, 59, 59, 999);


            var supplies = await _supplyHistoryService.GetSupplyRecordsFilteredAsync(
                startDate: startDate,
                endDate: endDate,
                productId: SelectedProductFilter?.ProductId == 0 ? (int?)null : SelectedProductFilter?.ProductId,
                supplierId: SelectedSupplierFilter?.SupplierId == 0 ? (int?)null : SelectedSupplierFilter?.SupplierId,
                categoryId: SelectedCategoryFilter?.CategoryId == 0 ? (int?)null : SelectedCategoryFilter?.CategoryId
            );

            foreach (var supply in supplies)
            {
                var product = await _productService.GetProductByIdAsync(supply.ProductId);
                var supplier = await _supplierService.GetSupplierByIdAsync(supply.SupplierId);

                ReportDetails.Add(new ReportDetailItem
                {
                    SupplyDate = supply.SupplyDate,
                    ProductName = product?.ProductName ?? "Неизвестный продукт",
                    SupplierName = supplier?.CompanyName ?? "Неизвестный поставщик",
                    SuppliedQuantity = supply.SuppliedQuantity,
                    SupplyUnitPrice = supply.SupplyUnitPrice
                });
            }

            TotalCost = ReportDetails.Sum(item => item.TotalItemCost);

            if (ReportDetails.Count == 0)
            {
                ErrorMessage = "Данные для выбранных критериев не найдены.";
            }
        }
    }
}