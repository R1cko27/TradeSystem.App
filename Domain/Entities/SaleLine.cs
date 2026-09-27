// Domain/Entities/SaleLine.cs

using System;

// Файл содержит определение класса SaleLine,
// который представляет собой сущность позиции продажи товара в системе торговли.

namespace TradeSystem.Domain.Entities;

public sealed class SaleLine : Entity
{
    public Guid ProductId { get; set; } // Идентификатор товара

    public string ProductName { get; set; } = string.Empty; // Наименование товара

    public Guid ShelfId { get; set; } // Идентификатор полки, с которой списан товар

    public int Quantity { get; set; } // Количество проданного товара

    public decimal UnitPrice { get; set; } // Цена за единицу

    public decimal TotalPrice { get; set; } // Общая стоимость позиции

    public string? Comment { get; set; } // Комментарий к позиции продажи
}