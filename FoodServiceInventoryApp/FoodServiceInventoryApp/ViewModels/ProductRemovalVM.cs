using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FoodServiceInventoryApp.Models;
using FoodServiceInventoryApp.Services;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System;

namespace FoodServiceInventoryApp.ViewModels
{
    public partial class ProductRemovalVM : ObservableObject
    {
        private readonly IProductService _productService;

        [ObservableProperty]
        private ObservableCollection<Product> _products;

        [ObservableProperty]
        private Product _selectedProduct;

        [ObservableProperty]
        private decimal _quantityToDeduct;

        public IAsyncRelayCommand LoadProductsCommand { get; }
        public IAsyncRelayCommand DeductProductQuantityCommand { get; }

        public ProductRemovalVM(IProductService productService)
        {
            _productService = productService;
            Products = new ObservableCollection<Product>();

            LoadProductsCommand = new AsyncRelayCommand(LoadProductsAsync);
            DeductProductQuantityCommand = new AsyncRelayCommand(DeductProductQuantityAsync, CanDeductProductQuantity);

            PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(SelectedProduct) || e.PropertyName == nameof(QuantityToDeduct))
                {
                    DeductProductQuantityCommand.NotifyCanExecuteChanged();
                }
            };

            LoadProductsCommand.Execute(null);
        }

        private async Task LoadProductsAsync()
        {
            Products.Clear();
            var productsFromDb = await _productService.GetAllProductsAsync();
            foreach (var product in productsFromDb)
            {
                Products.Add(product);
            }
        }

        private bool CanDeductProductQuantity()
        {
            return SelectedProduct != null &&
                   QuantityToDeduct > 0 &&
                   QuantityToDeduct <= SelectedProduct.Quantity;
        }

        private async Task DeductProductQuantityAsync()
        {
            if (!CanDeductProductQuantity())
            {
                if (SelectedProduct == null)
                {
                    MessageBox.Show("Пожалуйста, выберите продукт для списания.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                else if (QuantityToDeduct <= 0)
                {
                    MessageBox.Show("Количество для списания должно быть больше нуля.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                else if (QuantityToDeduct > SelectedProduct.Quantity)
                {
                    MessageBox.Show("Количество для списания превышает доступный остаток.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                return;
            }

            var result = MessageBox.Show($"Вы уверены, что хотите списать {QuantityToDeduct} {SelectedProduct.UnitOfMeasure} продукта '{SelectedProduct.ProductName}'?", "Подтвердите списание", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    await _productService.UpdateProductQuantityAsync(SelectedProduct.ProductId, -QuantityToDeduct);
                    MessageBox.Show("Количество продукта успешно списано!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    QuantityToDeduct = 0;
                    await LoadProductsAsync();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Произошла ошибка при списании количества продукта: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}