// Domain/Entities/GoodsReceipt.cs

using System;
using System.Collections.Generic;
using TradeSystem.Domain.Enums;

// Файл содержит определение класса GoodsReceipt,
// который представляет собой сущность поступления товара в системе торговли.

namespace TradeSystem.Domain.Entities;

public sealed class GoodsReceipt : Entity
{
    public string Number { get; set; } = string.Empty; // Номер документа поступления

    public Guid SupplierId { get; set; } // Идентификатор поставщика

    public Guid? OrderId { get; set; } // Идентификатор связанного заказа

    public DateTimeOffset ReceivedAtUtc { get; set; } = DateTimeOffset.UtcNow; // Дата и время поступления товара в формате UTC

    public GoodsReceiptStatus Status { get; set; } = GoodsReceiptStatus.Draft; // Статус документа поступления

    public string? Comment { get; set; } // Комментарий к поступлению

    public List<GoodsReceiptLine> Lines { get; set; } = new(); // Позиции поступления
}