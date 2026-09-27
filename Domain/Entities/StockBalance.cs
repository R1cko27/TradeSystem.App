// Domain/Entities/StockBalance.cs

using System;

// Файл содержит определение класса StockBalance, 
// который представляет собой сущность баланса запасов в системе торговли. 

namespace TradeSystem.Domain.Entities;

public sealed class StockBalance : Entity
{
    public Guid ProductId { get; set; } // Идентификатор продукта

    public Guid ShelfId { get; set; } // Идентификатор полки

    public int Quantity { get; set; } // Количество

    public string? Note { get; set; } // Примечание о балансе запасов
}