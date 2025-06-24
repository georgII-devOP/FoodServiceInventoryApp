using Xunit;
using Moq;
using FoodServiceInventoryApp.ViewModels;
using FoodServiceInventoryApp.Services;
using System.Threading.Tasks;
using System;
using System.Windows;
using CommunityToolkit.Mvvm.Input;

namespace FoodServiceInventoryApp.Tests
{
    public class LoginVMTests
    {
        private readonly Mock<IAuthenticationService> _mockAuthenticationService;
        private readonly Mock<INavigationService> _mockNavigationService;
        private readonly LoginVM _sut;

        public LoginVMTests()
        {
            _mockAuthenticationService = new Mock<IAuthenticationService>();
            _mockNavigationService = new Mock<INavigationService>();

            _sut = new LoginVM(_mockAuthenticationService.Object, _mockNavigationService.Object);
        }


        [Fact]
        public async Task LoginCommand_AuthenticatesSuccessfully_InvokesLoginSuccessAndResetsErrorMessage()
        {
            _sut.Username = "testuser";
            _sut.Password = "password";
            _mockAuthenticationService.Setup(s => s.AuthenticateUserAsync("testuser", "password"))
                                      .ReturnsAsync(true);

            bool loginSuccessInvoked = false;
            _sut.LoginSuccess += (sender, args) => loginSuccessInvoked = true;

            await ((IAsyncRelayCommand)_sut.LoginCommand).ExecuteAsync(null);

            _mockAuthenticationService.Verify(s => s.AuthenticateUserAsync("testuser", "password"), Times.Once);
            Assert.True(loginSuccessInvoked);
            Assert.Equal(string.Empty, _sut.ErrorMessage);
        }

        [Fact]
        public async Task LoginCommand_FailsAuthentication_SetsErrorMessage()
        {
            _sut.Username = "wronguser";
            _sut.Password = "wrongpass";
            _mockAuthenticationService.Setup(s => s.AuthenticateUserAsync("wronguser", "wrongpass"))
                                      .ReturnsAsync(false);

            bool loginSuccessInvoked = false;
            _sut.LoginSuccess += (sender, args) => loginSuccessInvoked = true;

            await ((IAsyncRelayCommand)_sut.LoginCommand).ExecuteAsync(null);

            _mockAuthenticationService.Verify(s => s.AuthenticateUserAsync("wronguser", "wrongpass"), Times.Once);
            Assert.False(loginSuccessInvoked);
            Assert.Equal("Неверный логин или пароль.", _sut.ErrorMessage);
        }

        [Theory]
        [InlineData("", "password", "Пожалуйста, введите логин и пароль.")]
        [InlineData("username", "", "Пожалуйста, введите логин и пароль.")]
        [InlineData(" ", "password", "Пожалуйста, введите логин и пароль.")]
        [InlineData("username", " ", "Пожалуйста, введите логин и пароль.")]
        public async Task LoginCommand_MissingCredentials_SetsErrorMessageAndDoesNotAuthenticate(string username, string password, string expectedError)
        {
            _sut.Username = username;
            _sut.Password = password;

            await ((IAsyncRelayCommand)_sut.LoginCommand).ExecuteAsync(null);

            _mockAuthenticationService.Verify(s => s.AuthenticateUserAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            Assert.Equal(expectedError, _sut.ErrorMessage);
        }

        [Fact]
        public async Task LoginCommand_HandlesException_SetsErrorMessage()
        {
            _sut.Username = "testuser";
            _sut.Password = "password";
            _mockAuthenticationService.Setup(s => s.AuthenticateUserAsync(It.IsAny<string>(), It.IsAny<string>()))
                                      .ThrowsAsync(new InvalidOperationException("Тестовая ошибка подключения."));

            await ((IAsyncRelayCommand)_sut.LoginCommand).ExecuteAsync(null);

            Assert.StartsWith("Произошла ошибка: Тестовая ошибка подключения.", _sut.ErrorMessage);
        }

        [Fact]
        public void NavigateToMainWindowCommand_DoesNothing_IfWindowIsNull()
        {
            _sut.NavigateToMainWindowCommand.Execute(null);

            _mockNavigationService.Verify(ns => ns.ShowMainWindow(), Times.Never);
        }
    }
}