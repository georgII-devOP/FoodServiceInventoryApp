using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Windows.Input;
using System;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;

namespace FoodServiceInventoryApp.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        private readonly IServiceProvider _serviceProvider;

        [ObservableProperty]
        private ObservableObject _currentViewModel;

        public ICommand NavigateToProductInputViewCommand { get; }
        public ICommand NavigateToProductRemovalViewCommand { get; }
        public ICommand NavigateToStockReportViewCommand { get; }
        public ICommand NavigateToPurchaseCostReportViewCommand { get; }
        public ICommand NavigateToSupplierReportViewCommand { get; }
        public ICommand NavigateToPurchasePlanViewCommand { get; }
        public ICommand ExitApplicationCommand { get; }

        public MainViewModel(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;

            NavigateToProductInputViewCommand = new RelayCommand(ExecuteNavigateToProductInputView);
            NavigateToProductRemovalViewCommand = new RelayCommand(ExecuteNavigateToProductRemovalView);
            NavigateToStockReportViewCommand = new RelayCommand(ExecuteNavigateToStockReportView);
            NavigateToPurchaseCostReportViewCommand = new RelayCommand(ExecuteNavigateToPurchaseCostReportView);
            NavigateToSupplierReportViewCommand = new RelayCommand(ExecuteNavigateToSupplierReportView);
            NavigateToPurchasePlanViewCommand = new RelayCommand(ExecuteNavigateToPurchasePlanView);
            ExitApplicationCommand = new RelayCommand(ExecuteExitApplication);

            ExecuteNavigateToProductInputView();
        }

        public async void NavigateToProductInputForEdit(int productId)
        {
            var productInputVm = _serviceProvider.GetRequiredService<ProductInputVM>();
            await productInputVm.LoadProductForEdit(productId);
            CurrentViewModel = productInputVm;
        }

        private void ExecuteNavigateToProductInputView()
        {
            var productInputVm = _serviceProvider.GetRequiredService<ProductInputVM>();
            productInputVm.ResetForm();
            CurrentViewModel = productInputVm;
        }

        private void ExecuteNavigateToProductRemovalView()
        {
            CurrentViewModel = _serviceProvider.GetRequiredService<ProductRemovalVM>();
        }

        private void ExecuteNavigateToStockReportView()
        {
            CurrentViewModel = _serviceProvider.GetRequiredService<StockReportVM>();
        }

        private void ExecuteNavigateToPurchaseCostReportView()
        {
            CurrentViewModel = _serviceProvider.GetRequiredService<PurchaseCostReportVM>();
        }

        private void ExecuteNavigateToSupplierReportView()
        {
            CurrentViewModel = _serviceProvider.GetRequiredService<SupplierReportVM>();
        }

        private void ExecuteNavigateToPurchasePlanView()
        {
            CurrentViewModel = _serviceProvider.GetRequiredService<PurchasePlanVM>();
        }

        private void ExecuteExitApplication()
        {
            Application.Current.Shutdown();
        }
    }
}