using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FoodServiceInventoryApp.Models;
using FoodServiceInventoryApp.Services;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System;

namespace FoodServiceInventoryApp.ViewModels
{
    public partial class ProductRemovalVM : ObservableObject
    {
        private readonly IProductService _productService;
        private readonly IMessageService _messageService;

        [ObservableProperty]
        private ObservableCollection<Product> _products;

        [ObservableProperty]
        private Product _selectedProduct;

        [ObservableProperty]
        private decimal _quantityToDeduct;

        public IAsyncRelayCommand LoadProductsCommand { get; }
        public IAsyncRelayCommand DeductProductQuantityCommand { get; }

        public ProductRemovalVM(IProductService productService, IMessageService messageService)
        {
            _productService = productService;
            _messageService = messageService;
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

        protected internal virtual async Task LoadProductsAsync()
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
                    _messageService.ShowMessage("Пожалуйста, выберите продукт для списания.", "Ошибка", MessageType.Warning);
                }
                else if (QuantityToDeduct <= 0)
                {
                    _messageService.ShowMessage("Количество для списания должно быть больше нуля.", "Ошибка", MessageType.Warning);
                }
                else if (QuantityToDeduct > SelectedProduct.Quantity)
                {
                    _messageService.ShowMessage("Количество для списания превышает доступный остаток.", "Ошибка", MessageType.Warning);
                }
                return;
            }
            var confirmationResult = _messageService.ShowConfirmation(
                $"Вы уверены, что хотите списать {QuantityToDeduct} {SelectedProduct.UnitOfMeasure} продукта '{SelectedProduct.ProductName}'?",
                "Подтвердите списание"
            );

            if (confirmationResult)
            {
                try
                {
                    await _productService.UpdateProductQuantityAsync(SelectedProduct.ProductId, -QuantityToDeduct);
                    _messageService.ShowMessage("Количество продукта успешно списано!", "Успех", MessageType.Information); // <-- ИЗМЕНЕНО
                    QuantityToDeduct = 0;
                    await LoadProductsAsync();
                }
                catch (Exception ex)
                {
                    _messageService.ShowMessage($"Произошла ошибка при списании количества продукта: {ex.Message}", "Ошибка", MessageType.Error); // <-- ИЗМЕНЕНО
                }
            }
        }
    }
}