// Infrastructure/Persistence/Json/ServiceCollectionExtensions.cs

using System;
using System.IO;
using Microsoft.Extensions.DependencyInjection;
using TradeSystem.Application.Contracts.Repositories;
using TradeSystem.Domain.Entities;

// Файл содержит класс ServiceCollectionExtensions,
// который предоставляет методы расширения для регистрации JSON-хранилища торговой системы в контейнере зависимостей.

namespace TradeSystem.Infrastructure.Persistence.Json;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddTradeSystemJsonPersistence( // Зарегистрировать JSON-хранилище и репозитории торговой системы
        this IServiceCollection services,
        JsonStorageOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        options ??= new JsonStorageOptions();

        services.AddSingleton(options);

        var directoryPath = string.IsNullOrWhiteSpace(options.DirectoryPath)
            ? "data"
            : options.DirectoryPath;

        var rootPath = Path.IsPathRooted(directoryPath) // Определить абсолютный путь к каталогу данных
            ? directoryPath
            : Path.Combine(AppContext.BaseDirectory, directoryPath);

        rootPath = Path.GetFullPath(rootPath);

        AddStore<Supplier>(services, rootPath, JsonFileNames.Suppliers, options.WriteIndented); // Хранилище поставщиков
        AddStore<Product>(services, rootPath, JsonFileNames.Products, options.WriteIndented); // Хранилище товаров
        AddStore<Shelf>(services, rootPath, JsonFileNames.Shelves, options.WriteIndented); // Хранилище полок
        AddStore<SupplierProduct>(services, rootPath, JsonFileNames.SupplierProducts, options.WriteIndented); // Хранилище связей поставщиков и товаров
        AddStore<StockBalance>(services, rootPath, JsonFileNames.StockBalances, options.WriteIndented); // Хранилище остатков товаров на полках
        AddStore<PurchaseOrder>(services, rootPath, JsonFileNames.PurchaseOrders, options.WriteIndented); // Хранилище заказов на покупку
        AddStore<Invoice>(services, rootPath, JsonFileNames.Invoices, options.WriteIndented); // Хранилище счетов на оплату
        AddStore<GoodsReceipt>(services, rootPath, JsonFileNames.GoodsReceipts, options.WriteIndented); // Хранилище поступлений товаров
        AddStore<Sale>(services, rootPath, JsonFileNames.Sales, options.WriteIndented); // Хранилище продаж
        AddStore<StockMovement>(services, rootPath, JsonFileNames.StockMovements, options.WriteIndented); // Хранилище движений товаров

        services.AddSingleton<ISupplierRepository, SupplierRepository>(); // Репозиторий поставщиков
        services.AddSingleton<IProductRepository, ProductRepository>(); // Репозиторий товаров
        services.AddSingleton<IShelfRepository, ShelfRepository>(); // Репозиторий полок
        services.AddSingleton<ISupplierProductRepository, SupplierProductRepository>(); // Репозиторий связей поставщиков и товаров
        services.AddSingleton<IStockBalanceRepository, StockBalanceRepository>(); // Репозиторий остатков товаров на полках
        services.AddSingleton<IPurchaseOrderRepository, PurchaseOrderRepository>(); // Репозиторий заказов на покупку
        services.AddSingleton<IInvoiceRepository, InvoiceRepository>(); // Репозиторий счетов на оплату
        services.AddSingleton<IGoodsReceiptRepository, GoodsReceiptRepository>(); // Репозиторий поступлений товаров
        services.AddSingleton<ISaleRepository, SaleRepository>(); // Репозиторий продаж
        services.AddSingleton<IStockMovementRepository, StockMovementRepository>(); // Репозиторий движений товаров

        return services;
    }

    private static void AddStore<TEntity>( // Зарегистрировать JSON-хранилище для указанной сущности
        IServiceCollection services,
        string rootPath,
        string fileName,
        bool writeIndented)
    {
        var filePath = Path.Combine(rootPath, fileName);

        services.AddSingleton<IJsonFileStore<TEntity>>(
            new JsonFileStore<TEntity>(filePath, writeIndented));
    }
}