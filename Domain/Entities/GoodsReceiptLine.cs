// Domain/Entities/GoodsReceiptLine.cs

using System;

// Файл содержит определение класса GoodsReceiptLine,
// который представляет собой сущность позиции поступления товара в системе торговли.

namespace TradeSystem.Domain.Entities;

public sealed class GoodsReceiptLine : Entity
{
    public Guid? OrderLineId { get; set; } // Идентификатор связанной позиции заказа

    public Guid ProductId { get; set; } // Идентификатор товара

    public string ProductName { get; set; } = string.Empty; // Наименование товара

    public Guid ShelfId { get; set; } // Идентификатор полки для размещения товара

    public int Quantity { get; set; } // Количество поступившего товара

    public decimal? UnitCost { get; set; } // Себестоимость единицы товара

    public string? Comment { get; set; } // Комментарий к позиции поступления
}