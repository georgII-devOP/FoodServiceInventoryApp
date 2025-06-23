using FoodServiceInventoryApp.ViewModels;
using FoodServiceInventoryApp.Views;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;

namespace FoodServiceInventoryApp.Services
{
    public class NavigationService : INavigationService
    {
        private readonly IServiceProvider _serviceProvider;

        public NavigationService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public void ShowMainWindow()
        {
            var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
            mainWindow.DataContext = _serviceProvider.GetRequiredService<MainViewModel>();
            mainWindow.Show();
        }

        public void CloseLoginWindow(Window loginWindow)
        {
            loginWindow.Close();
        }
        public interface INavigationService
        {
            void ShowMainWindow();
            void CloseLoginWindow(Window loginWindow);
        }
    }
}