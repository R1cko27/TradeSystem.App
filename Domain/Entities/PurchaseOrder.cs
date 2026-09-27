// Domain/Entities/PurchaseOrder.cs

using System;
using System.Collections.Generic;
using TradeSystem.Domain.Enums;

// Файл содержит определение класса PurchaseOrder,
// который представляет собой сущность заказа на покупку в системе торговли.

namespace TradeSystem.Domain.Entities;

public sealed class PurchaseOrder : Entity
{
    public string Number { get; set; } = string.Empty; // Номер заказа

    public Guid SupplierId { get; set; } // Идентификатор поставщика

    public DateTimeOffset OrderDateUtc { get; set; } = DateTimeOffset.UtcNow; // Дата и время оформления заказа в формате UTC

    public PurchaseOrderStatus Status { get; set; } = PurchaseOrderStatus.Draft; // Статус заказа

    public decimal TotalAmount { get; set; } // Общая сумма заказа

    public string? Comment { get; set; } // Комментарий к заказу

    public List<PurchaseOrderLine> Lines { get; set; } = new(); // Позиции заказа
}