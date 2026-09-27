// Domain/Entities/Invoice.cs

using System;
using TradeSystem.Domain.Enums;

// Файл содержит определение класса Invoice,
// который представляет собой сущность счёта на оплату в системе торговли.

namespace TradeSystem.Domain.Entities;

public sealed class Invoice : Entity
{
    public Guid OrderId { get; set; } // Идентификатор связанного заказа

    public string Number { get; set; } = string.Empty; // Номер счёта

    public DateTimeOffset IssueDateUtc { get; set; } = DateTimeOffset.UtcNow; // Дата и время выставления счёта в формате UTC

    public DateTimeOffset? DueDateUtc { get; set; } // Срок оплаты счёта в формате UTC

    public decimal Amount { get; set; } // Сумма счёта

    public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Unpaid; // Статус оплаты счёта

    public DateTimeOffset? PaidAtUtc { get; set; } // Дата и время оплаты счёта в формате UTC

    public string? Comment { get; set; } // Комментарий к счёту
}