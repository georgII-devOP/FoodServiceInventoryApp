using System.Threading.Tasks;

namespace FoodServiceInventoryApp.Services
{
    public interface IMessageService
    {
        void ShowMessage(string message, string title = "", MessageType type = MessageType.Information);
        bool ShowConfirmation(string message, string title = "");
    }

    public enum MessageType
    {
        Information,
        Warning,
        Error
    }
}