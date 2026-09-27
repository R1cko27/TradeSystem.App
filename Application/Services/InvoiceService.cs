// Application/Services/InvoiceService.cs

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TradeSystem.Application.Contracts.Repositories;
using TradeSystem.Application.Contracts.Services;
using TradeSystem.Application.Dto;
using TradeSystem.Application.Exceptions;
using TradeSystem.Application.Mappers;
using TradeSystem.Domain.Entities;
using TradeSystem.Domain.Enums;

// Файл содержит класс InvoiceService,
// который реализует сервис управления счетами на оплату в торговой системе.

namespace TradeSystem.Application.Services;

public sealed class InvoiceService : IInvoiceService
{
    private readonly IInvoiceRepository _invoices; // Репозиторий счетов на оплату
    private readonly IPurchaseOrderRepository _orders; // Репозиторий заказов на покупку

    public InvoiceService(IInvoiceRepository invoices, IPurchaseOrderRepository orders)
    {
        _invoices = invoices;
        _orders = orders;
    }

    public async Task<IReadOnlyList<InvoiceDto>> GetInvoicesAsync(CancellationToken ct = default) // Получить список всех счетов
    {
        var all = await _invoices.GetAllAsync(ct);
        return await BuildDtosAsync(all.ToList(), ct);
    }

    public async Task<IReadOnlyList<InvoiceDto>> GetInvoicesByOrderAsync(Guid orderId, CancellationToken ct = default) // Получить счета по заказу
    {
        var all = (await _invoices.GetAllAsync(ct)).Where(i => i.OrderId == orderId).ToList();
        return await BuildDtosAsync(all.ToList(), ct);
    }

    public async Task<InvoiceDto> CreateInvoiceAsync( // Создать счёт на оплату
        Guid orderId, string number, decimal amount, DateTimeOffset? dueDate, string? comment,
        CancellationToken ct = default)
    {
        if (await _orders.GetByIdAsync(orderId, ct) is null)
            throw new BusinessException("Заказ не найден.");
        if (string.IsNullOrWhiteSpace(number))
            throw new BusinessException("Номер счёта обязателен.");
        if (amount <= 0)
            throw new BusinessException("Сумма счёта должна быть больше нуля.");

        var invoice = new Invoice
        {
            OrderId = orderId,
            Number = number.Trim(),
            IssueDateUtc = DateTimeOffset.UtcNow,
            DueDateUtc = dueDate,
            Amount = amount,
            PaymentStatus = PaymentStatus.Unpaid,
            Comment = comment
        };

        await _invoices.AddAsync(invoice, ct);
        var dtos = await BuildDtosAsync(new List<Invoice> { invoice }, ct);
        return dtos.First();
    }

    public async Task<InvoiceDto> MarkPaidAsync( // Отметить счёт как оплаченный
        Guid invoiceId, DateTimeOffset? paidAt = null, string? comment = null,
        CancellationToken ct = default)
    {
        var invoice = await _invoices.GetByIdAsync(invoiceId, ct)
                      ?? throw new BusinessException("Счёт не найден.");

        if (invoice.PaymentStatus == PaymentStatus.Cancelled)
            throw new BusinessException("Нельзя оплатить отменённый счёт.");

        invoice.PaymentStatus = PaymentStatus.Paid;
        invoice.PaidAtUtc = paidAt ?? DateTimeOffset.UtcNow;
        if (!string.IsNullOrWhiteSpace(comment))
            invoice.Comment = string.IsNullOrWhiteSpace(invoice.Comment) ? comment : $"{invoice.Comment} | {comment}";

        await _invoices.UpdateAsync(invoice, ct);
        var dtos = await BuildDtosAsync(new List<Invoice> { invoice }, ct);
        return dtos.First();
    }

    public async Task DeleteInvoiceAsync(Guid invoiceId, CancellationToken ct = default) // Удалить счёт
    {
        var invoice = await _invoices.GetByIdAsync(invoiceId, ct) ?? throw new BusinessException("Счёт не найден.");
        if (invoice.PaymentStatus == PaymentStatus.Paid)
            throw new BusinessException("Нельзя удалить оплаченный счёт.");
        await _invoices.DeleteAsync(invoiceId, ct);
    }

    private async Task<IReadOnlyList<InvoiceDto>> BuildDtosAsync(List<Invoice> invoices, CancellationToken ct) // Построить DTO счетов
    {
        if (invoices.Count == 0) return Array.Empty<InvoiceDto>();
        var orders = (await _orders.GetAllAsync(ct)).ToDictionary(o => o.Id, o => o.Number);
        return invoices
            .OrderByDescending(i => i.IssueDateUtc)
            .Select(i => DtoMapper.ToDto(i, orders.GetValueOrDefault(i.OrderId)))
            .ToList();
    }
}