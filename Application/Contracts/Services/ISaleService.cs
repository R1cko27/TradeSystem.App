// Application/Contracts/Services/ISaleService.cs

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TradeSystem.Application.Dto;

// Файл содержит интерфейс ISaleService,
// который определяет контракт сервиса управления продажами товаров в торговой системе.

namespace TradeSystem.Application.Contracts.Services;

public interface ISaleService
{
    Task<IReadOnlyList<SaleDto>> GetSalesAsync(CancellationToken ct = default); // Получить список продаж
    Task<SaleDto?> GetSaleAsync(Guid id, CancellationToken ct = default); // Получить продажу по идентификатору

    Task<SaleDto> CreateDraftAsync(SaleInput input, CancellationToken ct = default); // Создать черновик продажи

    /// <summary>Проведение: списание с полок (с проверкой достаточности).</summary>
    Task<SaleDto> PostAsync(Guid saleId, CancellationToken ct = default); // Провести продажу

    Task DeleteDraftAsync(Guid saleId, CancellationToken ct = default); // Удалить черновик продажи
}