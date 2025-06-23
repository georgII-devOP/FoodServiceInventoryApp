using Microsoft.Extensions.DependencyInjection;
using FoodServiceInventoryApp.Views;
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
            mainWindow.Show();
        }

        public void CloseLoginWindow(Window loginWindow)
        {
            loginWindow.Close();
        }
    }
}