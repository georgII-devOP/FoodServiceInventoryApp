using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Windows.Input;
using System;
using Microsoft.Extensions.DependencyInjection;

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

        public MainViewModel(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;

            NavigateToProductInputViewCommand = new RelayCommand(ExecuteNavigateToProductInputView);
            NavigateToProductRemovalViewCommand = new RelayCommand(ExecuteNavigateToProductRemovalView);
            NavigateToStockReportViewCommand = new RelayCommand(ExecuteNavigateToStockReportView);
            NavigateToPurchaseCostReportViewCommand = new RelayCommand(ExecuteNavigateToPurchaseCostReportView);
            NavigateToSupplierReportViewCommand = new RelayCommand(ExecuteNavigateToSupplierReportView);
            NavigateToPurchasePlanViewCommand = new RelayCommand(ExecuteNavigateToPurchasePlanView);

            ExecuteNavigateToProductInputView();
        }

        private void ExecuteNavigateToProductInputView()
        {
            CurrentViewModel = _serviceProvider.GetRequiredService<ProductInputVM>();
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
    }
}