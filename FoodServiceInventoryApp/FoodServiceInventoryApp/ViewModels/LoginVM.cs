using System;
using System.Windows.Input;
using System.Windows;
using FoodServiceInventoryApp.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Threading.Tasks;

namespace FoodServiceInventoryApp.ViewModels
{
    public partial class LoginVM : ObservableObject
    {
        private readonly AuthenticationService _authenticationService;
        private readonly INavigationService _navigationService;

        [ObservableProperty]
        private string _username;

        [ObservableProperty]
        private string _password;

        [ObservableProperty]
        private string _errorMessage;

        public event EventHandler LoginSuccess;

        public ICommand NavigateToMainWindowCommand { get; }

        public ICommand LoginCommand { get; }
        public ICommand ExitCommand { get; }

        public LoginVM(AuthenticationService authenticationService, INavigationService navigationService)
        {
            _authenticationService = authenticationService;
            _navigationService = navigationService;

            LoginCommand = new AsyncRelayCommand(ExecuteLoginAsync);
            ExitCommand = new RelayCommand(ExecuteExit);
            NavigateToMainWindowCommand = new RelayCommand<Window>(ExecuteNavigateToMainWindow);
        }

        private async Task ExecuteLoginAsync()
        {
            ErrorMessage = string.Empty;

            if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
            {
                ErrorMessage = "Пожалуйста, введите логин и пароль.";
                return;
            }

            try
            {
                bool isAuthenticated = await _authenticationService.AuthenticateUserAsync(Username, Password);

                if (isAuthenticated)
                {
                    LoginSuccess?.Invoke(this, EventArgs.Empty);
                }
                else
                {
                    ErrorMessage = "Неверный логин или пароль.";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Произошла ошибка: {ex.Message}";
            }
        }

        private void ExecuteNavigateToMainWindow(Window loginWindow)
        {
            if (loginWindow != null)
            {
                loginWindow.Hide();
                _navigationService.ShowMainWindow();
                loginWindow.Close();
            }
        }

        private void ExecuteExit()
        {
            Application.Current.Shutdown();
        }
    }
}