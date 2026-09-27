// Application/Contracts/Services/IReportService.cs

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TradeSystem.Application.Dto;

// Файл содержит интерфейс IReportService,
// который определяет контракт сервиса формирования справочной информации в торговой системе.

namespace TradeSystem.Application.Contracts.Services;

public interface IReportService
{
    /// <summary>Список всех товаров.</summary>
    Task<IReadOnlyList<ProductDto>> GetAllProductsReportAsync(CancellationToken ct = default); // Отчёт по всем товарам

    /// <summary>Товары, имеющиеся в наличии (остаток &gt; 0).</summary>
    Task<IReadOnlyList<ProductDto>> GetAvailableProductsReportAsync(CancellationToken ct = default); // Отчёт по товарам в наличии

    /// <summary>Товары, количество которых необходимо пополнить.</summary>
    Task<IReadOnlyList<LowStockProductDto>> GetToReplenishReportAsync(CancellationToken ct = default); // Отчёт по товарам к пополнению

    /// <summary>Товары, поставляемые данным поставщиком.</summary>
    Task<IReadOnlyList<SupplierProductDto>> GetSupplierProductsReportAsync(Guid supplierId, CancellationToken ct = default); // Отчёт по товарам поставщика
}