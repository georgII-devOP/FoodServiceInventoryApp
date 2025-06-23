using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Windows.Input;
using System.Threading.Tasks;
using System;

namespace FoodServiceInventoryApp.ViewModels
{
    public partial class ProductRemovalVM : ObservableObject
    {
        [ObservableProperty]
        private string _productNameToRemove;

        [ObservableProperty]
        private string _quantityToRemove;

        [ObservableProperty]
        private string _unitOfMeasureToRemove;

        [ObservableProperty]
        private string _errorMessage;

        [ObservableProperty]
        private string _successMessage;

        public ICommand RemoveProductCommand { get; }
        public ICommand CancelCommand { get; }

        public ProductRemovalVM()
        {
            RemoveProductCommand = new AsyncRelayCommand(ExecuteRemoveProductAsync);
            CancelCommand = new RelayCommand(ExecuteCancel);
        }

        private async Task ExecuteRemoveProductAsync()
        {
            ErrorMessage = string.Empty;
            SuccessMessage = string.Empty;

            if (string.IsNullOrWhiteSpace(ProductNameToRemove) || string.IsNullOrWhiteSpace(QuantityToRemove))
            {
                ErrorMessage = "Пожалуйста, укажите название и количество для удаления.";
                return;
            }

            SuccessMessage = $"Запрос на удаление: {QuantityToRemove} {UnitOfMeasureToRemove} из {ProductNameToRemove}";
            Console.WriteLine($"Removing: {QuantityToRemove} {UnitOfMeasureToRemove} of {ProductNameToRemove}");

            ProductNameToRemove = string.Empty;
            QuantityToRemove = string.Empty;
            UnitOfMeasureToRemove = string.Empty;
        }

        private void ExecuteCancel()
        {
            ErrorMessage = string.Empty;
            SuccessMessage = string.Empty;
            ProductNameToRemove = string.Empty;
            QuantityToRemove = string.Empty;
            UnitOfMeasureToRemove = string.Empty;
            Console.WriteLine("Отмена удаления продукта.");
        }
    }
}