using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.IO;
using System.Windows;
using Microsoft.EntityFrameworkCore;
using FoodServiceInventoryApp.Services;
using FoodServiceInventoryApp.ViewModels;
using FoodServiceInventoryApp.Views;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FoodServiceInventoryApp
{
    public partial class App : Application
    {
        public readonly IHost _host;

        public App()
        {
            _host = Host.CreateDefaultBuilder()
                .ConfigureAppConfiguration((context, config) =>
                {
                    config.SetBasePath(Directory.GetCurrentDirectory());
                    config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
                })
                .ConfigureServices((context, services) =>
                {
                    string connectionString = context.Configuration.GetConnectionString("FoodServiceDbConnection");

                    services.AddDbContext<FoodServiceDbContext>(options =>
                        options.UseSqlServer(connectionString));

                    services.AddSingleton<AuthenticationService>();

                    services.AddTransient<IProductService, ProductService>();
                    services.AddTransient<ICategoryService, CategoryService>();
                    services.AddTransient<ISupplierService, SupplierService>();
                    services.AddTransient<IProductSupplyHistoryService, ProductSupplyHistoryService>();

                    services.AddTransient<LoginVM>();
                    services.AddSingleton<MainViewModel>();
                    services.AddTransient<ProductInputVM>();
                    services.AddTransient<ProductRemovalVM>();
                    services.AddTransient<StockReportVM>();
                    services.AddTransient<PurchaseCostReportVM>();
                    services.AddTransient<SupplierReportVM>();
                    services.AddTransient<PurchasePlanVM>();

                    services.AddSingleton<LoginWindow>();
                    services.AddSingleton<MainWindow>();
                    services.AddTransient<ProductInputView>();
                    services.AddTransient<ProductRemovalView>();
                    services.AddTransient<StockReportView>();
                    services.AddTransient<PurchaseCostReportView>();
                    services.AddTransient<SupplierReportView>();
                    services.AddTransient<PurchasePlanView>();

                    services.AddSingleton<INavigationService, NavigationService>();
                })
                .Build();
        }

        protected override async void OnStartup(StartupEventArgs e)
        {
            await _host.StartAsync();
            using (var scope = _host.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<FoodServiceDbContext>();
                try
                {
                    await dbContext.Database.MigrateAsync();
                }
                catch (Exception ex)
                {
                    Application.Current.Shutdown();
                    return;
                }
            }

            var loginWindow = _host.Services.GetRequiredService<LoginWindow>();
            loginWindow.Show();

            base.OnStartup(e);
        }

        protected override async void OnExit(ExitEventArgs e)
        {
            using (_host)
            {
                await _host.StopAsync();
            }
            base.OnExit(e);
        }
    }
}