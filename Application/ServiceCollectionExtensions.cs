// Application/ServiceCollectionExtensions.cs

using System;
using Microsoft.Extensions.DependencyInjection;
using TradeSystem.Application.Contracts.Services;
using TradeSystem.Application.Services;

namespace TradeSystem.Application;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddTradeSystemApplication(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddSingleton<IStockService, StockService>();
        services.AddSingleton<ICatalogService, CatalogService>();
        services.AddSingleton<IPurchaseOrderService, PurchaseOrderService>();
        services.AddSingleton<IInvoiceService, InvoiceService>();
        services.AddSingleton<IGoodsReceiptService, GoodsReceiptService>();
        services.AddSingleton<ISaleService, SaleService>();
        services.AddSingleton<IReportService, ReportService>();

        return services;
    }
}