using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FoodServiceInventoryApp.Models;
using FoodServiceInventoryApp.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Diagnostics;

namespace FoodServiceInventoryApp.ViewModels
{
    public partial class ProductInputVM : ObservableObject
    {
        private readonly IMessageService _messageService;
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;
        private readonly ISupplierService _supplierService;
        private readonly IProductSupplyHistoryService _productSupplyHistoryService;

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

        [ObservableProperty]
        private ObservableCollection<Supplier> _suppliers;

        [ObservableProperty]
        private Supplier _selectedSupplier;
        public IAsyncRelayCommand SaveProductCommand { get; }
        public IAsyncRelayCommand LoadCategoriesCommand { get; }
        public IAsyncRelayCommand LoadSuppliersCommand { get; }

        public ProductInputVM(IProductService productService, ICategoryService categoryService,
                              ISupplierService supplierService,
                              IProductSupplyHistoryService productSupplyHistoryService,
                              IMessageService messageService)
        {
            _productService = productService;
            _categoryService = categoryService;
            _supplierService = supplierService;
            _productSupplyHistoryService = productSupplyHistoryService;
            _messageService = messageService;

            Categories = new ObservableCollection<Category>();
            Suppliers = new ObservableCollection<Supplier>();

            SaveProductCommand = new AsyncRelayCommand(SaveProductAsync, CanSaveProduct);
            LoadCategoriesCommand = new AsyncRelayCommand(LoadCategoriesAsync);
            LoadSuppliersCommand = new AsyncRelayCommand(LoadSuppliersAsync);

            _ = LoadCategoriesAsync();
            _ = LoadSuppliersAsync();

            PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(ProductName) ||
                    e.PropertyName == nameof(SelectedCategory) ||
                    e.PropertyName == nameof(Quantity) ||
                    e.PropertyName == nameof(UnitOfMeasure) ||
                    e.PropertyName == nameof(UnitPrice) ||
                    e.PropertyName == nameof(SelectedSupplier))
                {
                    SaveProductCommand.NotifyCanExecuteChanged();
                }
            };
        }

        public virtual async Task LoadCategoriesAsync()
        {
            Categories.Clear();
            var categoriesFromDb = await _categoryService.GetAllCategoriesAsync();
            if (categoriesFromDb != null)
            {
                foreach (var category in categoriesFromDb.OrderBy(c => c.CategoryName))
                {
                    Categories.Add(category);
                }
            }
            if (SelectedCategory == null && Categories.Any())
            {
                SelectedCategory = Categories.FirstOrDefault();
            }
        }

        public virtual async Task LoadSuppliersAsync()
        {
            Suppliers.Clear();
            var allSuppliers = await _supplierService.GetAllSuppliersAsync();
            if (allSuppliers != null)
            {
                foreach (var s in allSuppliers.OrderBy(s => s.CompanyName))
                {
                    Suppliers.Add(s);
                }
            }
            if (SelectedSupplier == null && Suppliers.Any())
            {
                SelectedSupplier = Suppliers.FirstOrDefault();
            }
        }

        public virtual async Task LoadProductForEdit(int productId)
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
                SelectedSupplier = null;
            }
            else
            {

                _messageService.ShowMessage("Продукт не найден для редактирования.", "Ошибка", MessageType.Error);
                ResetForm();
            }
        }

        public virtual void ResetForm()
        {
            IsEditMode = false;
            ProductId = 0;
            ProductName = string.Empty;
            Quantity = 0;
            UnitOfMeasure = string.Empty;
            UnitPrice = 0;
            LastSupplyDate = DateTime.Now;
            SelectedCategory = Categories.FirstOrDefault();
            SelectedSupplier = Suppliers.FirstOrDefault();
        }

        private bool CanSaveProduct()
        {
            bool baseValid = !string.IsNullOrWhiteSpace(ProductName) &&
                             SelectedCategory != null &&
                             !string.IsNullOrWhiteSpace(UnitOfMeasure);

            if (IsEditMode)
            {
                return baseValid && Quantity >= 0 && UnitPrice >= 0;
            }
            else
            {
                return baseValid &&
                       SelectedSupplier != null &&
                       Quantity > 0 &&
                       UnitPrice > 0;
            }
        }

        private async Task SaveProductAsync()
        {
            if (!CanSaveProduct())
            {
                _messageService.ShowMessage("Пожалуйста, заполните все обязательные поля корректными значениями.", "Ошибка ввода", MessageType.Warning);
                return;
            }

            Product productToSave;

            if (IsEditMode)
            {
                productToSave = await _productService.GetProductByIdAsync(ProductId);
                if (productToSave == null)
                {
                    _messageService.ShowMessage("Редактируемый продукт не найден в базе данных.", "Ошибка", MessageType.Error);
                    ResetForm();
                    return;
                }

                if (await _productService.ProductExistsByNameAsync(ProductName) && ProductName.ToLower() != productToSave.ProductName.ToLower())
                {
                    _messageService.ShowMessage($"Продукт с названием '{ProductName}' уже существует.", "Ошибка", MessageType.Error);
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
                    _messageService.ShowMessage("Продукт успешно обновлен!", "Успех", MessageType.Information);
                }
                catch (Exception ex)
                {
                    _messageService.ShowMessage($"Ошибка при обновлении продукта: {ex.Message}", "Ошибка", MessageType.Error);
                }
            }
            else
            {
                if (await _productService.ProductExistsByNameAsync(ProductName))
                {
                    _messageService.ShowMessage($"Продукт с названием '{ProductName}' уже существует.", "Ошибка", MessageType.Error);
                    return;
                }

                try
                {
                    productToSave = new Product
                    {
                        ProductName = ProductName,
                        Quantity = Quantity,
                        UnitOfMeasure = UnitOfMeasure,
                        CategoryId = SelectedCategory.CategoryId,
                        UnitPrice = UnitPrice,
                        LastSupplyDate = LastSupplyDate
                    };

                    await _productService.AddProductAsync(productToSave);
                    ProductId = productToSave.ProductId;

                    if (productToSave.ProductId == 0)
                    {
                        throw new InvalidOperationException("Не удалось получить ProductId после добавления продукта.");
                    }

                    var supplyRecord = new ProductSupplyHistory
                    {
                        ProductId = productToSave.ProductId,
                        SupplierId = SelectedSupplier.SupplierId,
                        SuppliedQuantity = Quantity,
                        SupplyUnitPrice = UnitPrice,
                        SupplyDate = LastSupplyDate
                    };

                    await _productSupplyHistoryService.AddSupplyRecordAsync(supplyRecord);

                    _messageService.ShowMessage($"Продукт '{ProductName}' и его первая поставка успешно добавлены!", "Успех", MessageType.Information);
                    ResetForm();
                }
                catch (Exception ex)
                {
                    _messageService.ShowMessage($"Ошибка при добавлении продукта и его первой поставки: {ex.Message}", "Ошибка", MessageType.Error);
                    Debug.WriteLine($"Ошибка при добавлении продукта и его первой поставки: {ex.Message}");
                }
            }
        }
    }
}