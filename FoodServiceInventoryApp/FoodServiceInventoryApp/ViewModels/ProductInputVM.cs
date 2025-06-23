using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FoodServiceInventoryApp.Models;
using System;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace FoodServiceInventoryApp.ViewModels
{
    public partial class ProductInputVM : ObservableObject
    {
        [ObservableProperty]
        private string _productName;

        [ObservableProperty]
        private DateTime? _deliveryDate = DateTime.Now;

        [ObservableProperty]
        private string _quantity;

        [ObservableProperty]
        private string _unitOfMeasure;

        [ObservableProperty]
        private ObservableCollection<string> _categories;
        [ObservableProperty]
        private string _selectedCategory;

        [ObservableProperty]
        private ObservableCollection<string> _suppliers;
        [ObservableProperty]
        private string _selectedSupplier;

        [ObservableProperty]
        private string _pricePerUnit;

        [ObservableProperty]
        private string _errorMessage;

        [ObservableProperty]
        private string _successMessage;

        public ICommand SaveProductCommand { get; }
        public ICommand CancelCommand { get; }

        public ProductInputVM()
        {
            Categories = new ObservableCollection<string> { "Продукты", "Принадлежности" };
            Suppliers = new ObservableCollection<string> { "Поставщик А", "Поставщик Б" };

            SaveProductCommand = new AsyncRelayCommand(ExecuteSaveProductAsync);
            CancelCommand = new RelayCommand(ExecuteCancel);
        }

        private async Task ExecuteSaveProductAsync()
        {
            ErrorMessage = string.Empty;
            SuccessMessage = string.Empty;

            if (string.IsNullOrWhiteSpace(ProductName) || string.IsNullOrWhiteSpace(Quantity) || string.IsNullOrWhiteSpace(UnitOfMeasure))
            {
                ErrorMessage = "Пожалуйста, заполните основные поля.";
                return;
            }

            SuccessMessage = $"Данные введены: Название: {ProductName}, Количество: {Quantity}, Категория: {SelectedCategory}";
            Console.WriteLine($"Saving Product: {ProductName}, Quantity: {Quantity}, Unit: {UnitOfMeasure}, Category: {SelectedCategory}, Supplier: {SelectedSupplier}, Price: {PricePerUnit}, Date: {DeliveryDate?.ToShortDateString()}");

            ProductName = string.Empty;
            Quantity = string.Empty;
            UnitOfMeasure = string.Empty;
            PricePerUnit = string.Empty;
            DeliveryDate = DateTime.Now;
            SelectedCategory = null;
            SelectedSupplier = null;
        }

        private void ExecuteCancel()
        {
            ErrorMessage = string.Empty;
            SuccessMessage = string.Empty;
            ProductName = string.Empty;
            Quantity = string.Empty;
            UnitOfMeasure = string.Empty;
            PricePerUnit = string.Empty;
            DeliveryDate = DateTime.Now;
            SelectedCategory = null;
            SelectedSupplier = null;
            Console.WriteLine("Отмена ввода продукта.");
        }
    }
}