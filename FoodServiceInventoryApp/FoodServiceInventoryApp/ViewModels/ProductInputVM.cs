using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FoodServiceInventoryApp.Models;
using FoodServiceInventoryApp.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace FoodServiceInventoryApp.ViewModels
{
    public partial class ProductInputVM : ObservableObject
    {
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;

        [ObservableProperty]
        private int _productId;

        [ObservableProperty]
        private string _productName;

        [ObservableProperty]
        private decimal _quantity;

        [ObservableProperty]
        private string _unitOfMeasure;

        [ObservableProperty]
        private decimal _unitPrice;

        [ObservableProperty]
        private DateTime _lastSupplyDate = DateTime.Now;

        [ObservableProperty]
        private ObservableCollection<Category> _categories;

        [ObservableProperty]
        private Category _selectedCategory;

        [ObservableProperty]
        private bool _isEditMode;

        public IAsyncRelayCommand SaveProductCommand { get; }
        public IAsyncRelayCommand LoadCategoriesCommand { get; }

        public ProductInputVM(IProductService productService, ICategoryService categoryService)
        {
            _productService = productService;
            _categoryService = categoryService;

            Categories = new ObservableCollection<Category>();

            SaveProductCommand = new AsyncRelayCommand(SaveProductAsync, CanSaveProduct);
            LoadCategoriesCommand = new AsyncRelayCommand(LoadCategoriesAsync);

            _ = LoadCategoriesAsync();

            PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(ProductName) ||
                    e.PropertyName == nameof(SelectedCategory) ||
                    e.PropertyName == nameof(Quantity) ||
                    e.PropertyName == nameof(UnitOfMeasure) ||
                    e.PropertyName == nameof(UnitPrice))
                {
                    SaveProductCommand.NotifyCanExecuteChanged();
                }
            };
        }
        private async Task LoadCategoriesAsync()
        {
            Categories.Clear();
            var categoriesFromDb = await _categoryService.GetAllCategoriesAsync();
            foreach (var category in categoriesFromDb)
            {
                Categories.Add(category);
            }
            if (SelectedCategory == null && Categories.Any())
            {
                SelectedCategory = Categories.FirstOrDefault();
            }
        }

        public async Task LoadProductForEdit(int productId)
        {
            IsEditMode = true;
            ProductId = productId;

            var productToEdit = await _productService.GetProductByIdAsync(productId);
            if (productToEdit != null)
            {
                ProductName = productToEdit.ProductName;
                Quantity = productToEdit.Quantity;
                UnitOfMeasure = productToEdit.UnitOfMeasure;
                UnitPrice = productToEdit.UnitPrice;
                LastSupplyDate = productToEdit.LastSupplyDate;
                SelectedCategory = Categories.FirstOrDefault(c => c.CategoryId == productToEdit.CategoryId);
            }
            else
            {
                MessageBox.Show("Продукт не найден для редактирования.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                ResetForm();
            }
        }

        public void ResetForm()
        {
            IsEditMode = false;
            ProductId = 0;
            ProductName = string.Empty;
            Quantity = 0;
            UnitOfMeasure = string.Empty;
            UnitPrice = 0;
            LastSupplyDate = DateTime.Now;
            SelectedCategory = Categories.FirstOrDefault();
        }

        private bool CanSaveProduct()
        {
            return !string.IsNullOrWhiteSpace(ProductName) &&
                   SelectedCategory != null &&
                   Quantity >= 0 &&
                   !string.IsNullOrWhiteSpace(UnitOfMeasure) &&
                   UnitPrice >= 0;
        }

        private async Task SaveProductAsync()
        {
            if (!CanSaveProduct())
            {
                MessageBox.Show("Пожалуйста, заполните все обязательные поля корректными значениями.", "Ошибка ввода", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            Product productToSave;

            if (IsEditMode)
            {
                productToSave = await _productService.GetProductByIdAsync(ProductId);
                if (productToSave == null)
                {
                    MessageBox.Show("Редактируемый продукт не найден в базе данных.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    ResetForm();
                    return;
                }

                if (await _productService.ProductExistsByNameAsync(ProductName) && ProductName.ToLower() != productToSave.ProductName.ToLower())
                {
                    MessageBox.Show($"Продукт с названием '{ProductName}' уже существует.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                productToSave.ProductName = ProductName;
                productToSave.Quantity = Quantity;
                productToSave.UnitOfMeasure = UnitOfMeasure;
                productToSave.CategoryId = SelectedCategory.CategoryId;
                productToSave.UnitPrice = UnitPrice;
                productToSave.LastSupplyDate = LastSupplyDate;

                try
                {
                    await _productService.UpdateProductAsync(productToSave);
                    MessageBox.Show("Продукт успешно обновлен!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при обновлении продукта: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                if (await _productService.ProductExistsByNameAsync(ProductName))
                {
                    MessageBox.Show($"Продукт с названием '{ProductName}' уже существует.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                productToSave = new Product
                {
                    ProductName = ProductName,
                    Quantity = Quantity,
                    UnitOfMeasure = UnitOfMeasure,
                    CategoryId = SelectedCategory.CategoryId,
                    UnitPrice = UnitPrice,
                    LastSupplyDate = LastSupplyDate
                };

                try
                {
                    await _productService.AddProductAsync(productToSave);
                    MessageBox.Show("Продукт успешно добавлен!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    ResetForm();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при добавлении продукта: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}