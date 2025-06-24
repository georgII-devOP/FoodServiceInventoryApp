using FoodServiceInventoryApp.Services;
using System.Windows;

namespace FoodServiceInventoryApp
{
    public class MessageBoxService : IMessageService
    {
        public void ShowMessage(string message, string title = "", MessageType type = MessageType.Information)
        {
            MessageBoxImage image = MessageBoxImage.Information;
            switch (type)
            {
                case MessageType.Warning:
                    image = MessageBoxImage.Warning;
                    break;
                case MessageType.Error:
                    image = MessageBoxImage.Error;
                    break;
                default:
                    image = MessageBoxImage.Information;
                    break;
            }
            MessageBox.Show(message, title, MessageBoxButton.OK, image);
        }

        public bool ShowConfirmation(string message, string title = "")
        {
            MessageBoxResult result = MessageBox.Show(message, title, MessageBoxButton.YesNo, MessageBoxImage.Question);
            return result == MessageBoxResult.Yes;
        }
    }
}