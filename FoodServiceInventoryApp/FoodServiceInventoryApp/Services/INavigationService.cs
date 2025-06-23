using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace FoodServiceInventoryApp.Services
{
    public interface INavigationService
    {
        void ShowMainWindow();
        void CloseLoginWindow(Window loginWindow);
    }
}
