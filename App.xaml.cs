// App.xaml.cs

using System;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using TradeSystem.App.ViewModels;
using TradeSystem.App.ViewModels.Abstractions;
using TradeSystem.App.ViewModels.Invoices;
using TradeSystem.App.ViewModels.Orders;
using TradeSystem.App.ViewModels.Products;
using TradeSystem.App.ViewModels.Receipts;
using TradeSystem.App.ViewModels.Reports;
using TradeSystem.App.ViewModels.Sales;
using TradeSystem.App.ViewModels.Shelves;
using TradeSystem.App.ViewModels.SupplierProducts;
using TradeSystem.App.ViewModels.Suppliers;
using TradeSystem.App.Views;
using TradeSystem.Application;
using TradeSystem.Infrastructure.Persistence.Json;
using AppBase = System.Windows.Application;

// Файл содержит класс App,
// который представляет точку входа WPF-приложения торговой системы и настраивает контейнер зависимостей.

namespace TradeSystem.App
{
    public partial class App : AppBase
    {
        public static IServiceProvider Services { get; private set; } = null!; // Контейнер зависимостей приложения

        protected override void OnStartup(StartupEventArgs e) // Запуск приложения
        {
            base.OnStartup(e);

            var services = new ServiceCollection(); // Создать коллекцию сервисов для DI

            services.AddTradeSystemJsonPersistence(); // Зарегистрировать JSON-хранилище и репозитории
            services.AddTradeSystemApplication(); // Зарегистрировать сервисы прикладного слоя
            services.AddSingleton<IDialogService, DialogService>(); // Сервис диалоговых окон
            services.AddSingleton<IDocumentOutputService, DocumentOutputService>(); // Сервис печати и сохранения документов

            services.AddSingleton<ProductsListViewModel>(); // ViewModel списка товаров
            services.AddSingleton<ProductEditViewModel>(); // ViewModel формы товара
            services.AddSingleton<SuppliersListViewModel>(); // ViewModel списка поставщиков
            services.AddSingleton<SupplierEditViewModel>(); // ViewModel формы поставщика
            services.AddSingleton<ShelvesListViewModel>(); // ViewModel списка полок
            services.AddSingleton<ShelfEditViewModel>(); // ViewModel формы полки
            services.AddSingleton<SupplierProductsListViewModel>(); // ViewModel списка связей товаров и поставщиков
            services.AddSingleton<SupplierProductEditViewModel>(); // ViewModel формы связи товара и поставщика
            services.AddSingleton<OrdersListViewModel>(); // ViewModel списка заказов на покупку
            services.AddSingleton<OrderAutoPreviewViewModel>(); // ViewModel предпросмотра автоформирования заказов
            services.AddSingleton<OrderManualEditViewModel>(); // ViewModel формы ручного создания заказа
            services.AddSingleton<InvoiceListViewModel>(); // ViewModel списка счетов на оплату
            services.AddSingleton<ReceiptsListViewModel>(); // ViewModel списка поступлений товаров
            services.AddSingleton<ReceiptEditViewModel>(); // ViewModel формы создания поступления
            services.AddSingleton<SalesListViewModel>(); // ViewModel списка продаж
            services.AddSingleton<SaleEditViewModel>(); // ViewModel формы создания продажи
            services.AddSingleton<ReportsViewModel>(); // ViewModel формирования справочных отчётов
            services.AddSingleton<MainViewModel>(); // Главная ViewModel приложения

            Services = services.BuildServiceProvider(); // Собрать провайдер сервисов

            var mainVm = Services.GetRequiredService<MainViewModel>(); // Получить главную ViewModel из DI
            var window = new MainWindow { DataContext = mainVm }; // Создать главное окно с привязкой к ViewModel
            window.Show(); // Показать главное окно
        }
    }
}