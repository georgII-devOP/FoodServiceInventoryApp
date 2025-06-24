using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FoodServiceInventoryApp.Models;
using FoodServiceInventoryApp.Services;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using FoodServiceInventoryApp.Views;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace FoodServiceInventoryApp.ViewModels
{
    public partial class StockReportVM : ObservableObject
    {
        private readonly IProductService _productService;
        private readonly IServiceProvider _serviceProvider;
        private readonly MainViewModel _mainViewModel;

        [ObservableProperty]
        private ObservableCollection<Product> _products;

        [ObservableProperty]
        private Product _selectedProduct;

        public IAsyncRelayCommand LoadProductsCommand { get; }
        public IAsyncRelayCommand EditProductCommand { get; }

        public StockReportVM(IProductService productService, IServiceProvider serviceProvider, MainViewModel mainViewModel)
        {
            _productService = productService;
            _serviceProvider = serviceProvider;
            _mainViewModel = mainViewModel;

            Products = new ObservableCollection<Product>();

            LoadProductsCommand = new AsyncRelayCommand(LoadProductsAsync);
            EditProductCommand = new AsyncRelayCommand(EditProductAsync, CanEditProduct);

            PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(SelectedProduct))
                {
                    EditProductCommand.NotifyCanExecuteChanged();
                }
            };

            LoadProductsCommand.Execute(null);
        }

        protected internal virtual async Task LoadProductsAsync()
        {
            Products.Clear();
            var productsFromDb = await _productService.GetAllProductsAsync();
            foreach (var product in productsFromDb)
            {
                Products.Add(product);
            }
        }

        private bool CanEditProduct()
        {
            return SelectedProduct != null;
        }

        private async Task EditProductAsync()
        {
            if (SelectedProduct == null)
            {
                MessageBox.Show("Пожалуйста, выберите продукт для редактирования.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            _mainViewModel.NavigateToProductInputForEdit(SelectedProduct.ProductId);
        }
    }
}