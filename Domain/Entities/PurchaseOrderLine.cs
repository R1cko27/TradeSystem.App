// Domain/Entities/PurchaseOrderLine.cs

using System;
using TradeSystem.Domain.Enums;

// Файл содержит определение класса PurchaseOrderLine,
// который представляет собой сущность позиции заказа на покупку в системе торговли.

namespace TradeSystem.Domain.Entities;

public sealed class PurchaseOrderLine : Entity
{
    public Guid ProductId { get; set; } // Идентификатор товара

    public Guid? SupplierProductId { get; set; } // Идентификатор товара у поставщика

    public string ProductName { get; set; } = string.Empty; // Наименование товара

    public MeasurementUnit Unit { get; set; } = MeasurementUnit.NotSpecified; // Единица измерения

    public int OrderedQuantity { get; set; } // Заказанное количество

    public int ReceivedQuantity { get; set; } // Полученное количество

    public decimal UnitPrice { get; set; } // Цена за единицу

    public decimal TotalPrice { get; set; } // Общая стоимость позиции

    public bool IsClosed { get; set; } // Признак закрытия позиции

    public string? Comment { get; set; } // Комментарий к позиции
}
