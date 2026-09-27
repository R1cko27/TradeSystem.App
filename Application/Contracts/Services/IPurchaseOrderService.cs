// Application/Contracts/Services/IPurchaseOrderService.cs

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TradeSystem.Application.Dto;

// Файл содержит интерфейс IPurchaseOrderService,
// который определяет контракт сервиса управления заказами на покупку в торговой системе.

namespace TradeSystem.Application.Contracts.Services;

public interface IPurchaseOrderService
{
    Task<IReadOnlyList<PurchaseOrderDto>> GetOrdersAsync(CancellationToken ct = default); // Получить список заказов на покупку
    Task<PurchaseOrderDto?> GetOrderAsync(Guid id, CancellationToken ct = default); // Получить заказ по идентификатору

    Task<PurchaseOrderDto> CreateOrderAsync(CreatePurchaseOrderInput input, CancellationToken ct = default); // Создать заказ на покупку

    /// <summary>
    /// Автоформирование заказов по дефицитным товарам (+ доп. товары).
    /// Товары автоматически разносятся по поставщикам; для товара с несколькими
    /// поставщиками применяется стратегия выбора (явный выбор &gt; предпочтительный &gt; дешевле &gt; первый).
    /// </summary>
    Task<AutoOrderPreviewDto> PreviewAutoOrdersAsync(AutoOrderRequest? request = null, CancellationToken ct = default);
    Task<AutoOrderResult> AutoGenerateOrdersAsync(AutoOrderRequest? request = null, CancellationToken ct = default); // Автоматически сформировать заказы по дефициту

    Task ConfirmOrderAsync(Guid orderId, CancellationToken ct = default); // Подтвердить заказ
    Task CancelOrderAsync(Guid orderId, CancellationToken ct = default); // Отменить заказ
    Task DeleteOrderAsync(Guid orderId, CancellationToken ct = default); // Удалить заказ
}