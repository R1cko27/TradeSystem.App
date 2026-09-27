// Application/Contracts/Services/IGoodsReceiptService.cs

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TradeSystem.Application.Dto;

// Файл содержит интерфейс IGoodsReceiptService,
// который определяет контракт сервиса управления поступлениями товаров в торговой системе.

namespace TradeSystem.Application.Contracts.Services;

public interface IGoodsReceiptService
{
    Task<IReadOnlyList<GoodsReceiptDto>> GetReceiptsAsync(CancellationToken ct = default); // Получить список поступлений товаров
    Task<GoodsReceiptDto?> GetReceiptAsync(Guid id, CancellationToken ct = default); // Получить поступление по идентификатору

    Task<GoodsReceiptDto> CreateDraftAsync(GoodsReceiptInput input, CancellationToken ct = default); // Создать черновик поступления

    /// <summary>Проведение: приход на полки + обновление полученных количеств в заказе.</summary>
    Task<GoodsReceiptDto> PostAsync(Guid receiptId, CancellationToken ct = default); // Провести поступление товара

    Task DeleteDraftAsync(Guid receiptId, CancellationToken ct = default); // Удалить черновик поступления
}