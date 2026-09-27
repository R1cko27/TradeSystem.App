// Application/Contracts/Services/IInvoiceService.cs

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TradeSystem.Application.Dto;

// Файл содержит интерфейс IInvoiceService,
// который определяет контракт сервиса управления счетами на оплату в торговой системе.

namespace TradeSystem.Application.Contracts.Services;

public interface IInvoiceService
{
    Task<IReadOnlyList<InvoiceDto>> GetInvoicesAsync(CancellationToken ct = default); // Получить список всех счетов
    Task<IReadOnlyList<InvoiceDto>> GetInvoicesByOrderAsync(Guid orderId, CancellationToken ct = default); // Получить счета по заказу

    Task<InvoiceDto> CreateInvoiceAsync(Guid orderId, string number, decimal amount, DateTimeOffset? dueDate, string? comment, CancellationToken ct = default); // Создать счёт на оплату

    Task<InvoiceDto> MarkPaidAsync(Guid invoiceId, DateTimeOffset? paidAt = null, string? comment = null, CancellationToken ct = default); // Отметить счёт как оплаченный

    Task DeleteInvoiceAsync(Guid invoiceId, CancellationToken ct = default); // Удалить счёт
}