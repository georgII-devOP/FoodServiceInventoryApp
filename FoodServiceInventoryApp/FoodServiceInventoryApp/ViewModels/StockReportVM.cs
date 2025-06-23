using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FoodServiceInventoryApp.Models;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Threading.Tasks;
using System;
using System.Linq;

namespace FoodServiceInventoryApp.ViewModels
{
    public partial class StockReportVM : ObservableObject
    {

        [ObservableProperty]
        private ObservableCollection<Product> _products;

        [ObservableProperty]
        private ObservableCollection<string> _categories;
        [ObservableProperty]
        private string _selectedCategoryFilter;

        [ObservableProperty]
        private string _productNameFilter;

        [ObservableProperty]
        private string _errorMessage;

        public ICommand ApplyFilterCommand { get; }
        public ICommand ResetFilterCommand { get; }

        public StockReportVM()
        {
            LoadProductsAsync().Wait();

            Categories = new ObservableCollection<string> { "Все", "Продукты", "Принадлежности" };
            SelectedCategoryFilter = "Все";

            ApplyFilterCommand = new AsyncRelayCommand(ExecuteApplyFilterAsync);
            ResetFilterCommand = new RelayCommand(ExecuteResetFilter);
        }

        private async Task LoadProductsAsync()
        {
            var dummyProducts = new ObservableCollection<Product>
            {
                new Product { ProductId = 1, ProductName = "Молоко", Category = new Category { CategoryName = "Продукты" }, Quantity = 15.5M, UnitOfMeasure = "литр", UnitPrice = 1.2M, LastSupplyDate = DateTime.Now.AddDays(-5) },
                new Product { ProductId = 2, ProductName = "Хлеб", Category = new Category { CategoryName = "Продукты" }, Quantity = 20.0M, UnitOfMeasure = "буханка", UnitPrice = 0.8M, LastSupplyDate = DateTime.Now.AddDays(-2) },
                new Product { ProductId = 3, ProductName = "Салфетки", Category = new Category { CategoryName = "Принадлежности" }, Quantity = 200.0M, UnitOfMeasure = "шт.", UnitPrice = 0.03M, LastSupplyDate = DateTime.Now.AddDays(-10) },
                new Product { ProductId = 4, ProductName = "Мука", Category = new Category { CategoryName = "Продукты" }, Quantity = 50.0M, UnitOfMeasure = "кг", UnitPrice = 0.9M, LastSupplyDate = DateTime.Now.AddDays(-7) }
            };
            Products = dummyProducts;
        }

        private async Task ExecuteApplyFilterAsync()
        {
            ErrorMessage = string.Empty;
            var filteredProducts = new ObservableCollection<Product>(Products.ToList());

            if (SelectedCategoryFilter != "Все" && SelectedCategoryFilter != null)
            {
                filteredProducts = new ObservableCollection<Product>(
                    filteredProducts.Where(p => p.Category?.CategoryName == SelectedCategoryFilter)
                );
            }

            if (!string.IsNullOrWhiteSpace(ProductNameFilter))
            {
                filteredProducts = new ObservableCollection<Product>(
                    filteredProducts.Where(p => p.ProductName.Contains(ProductNameFilter, StringComparison.OrdinalIgnoreCase))
                );
            }
            Products = filteredProducts;
        }

        private void ExecuteResetFilter()
        {
            ProductNameFilter = string.Empty;
            SelectedCategoryFilter = "Все";
            LoadProductsAsync().Wait();
            ErrorMessage = string.Empty;
        }
    }
}