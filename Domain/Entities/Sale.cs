// Domain/Entities/Sale.cs

using System;
using System.Collections.Generic;
using TradeSystem.Domain.Enums;

// Файл содержит определение класса Sale,
// который представляет собой сущность продажи товара в системе торговли.

namespace TradeSystem.Domain.Entities;

public sealed class Sale : Entity
{
    public string Number { get; set; } = string.Empty; // Номер продажи

    public DateTimeOffset SoldAtUtc { get; set; } = DateTimeOffset.UtcNow; // Дата и время продажи в формате UTC

    public SaleStatus Status { get; set; } = SaleStatus.Draft; // Статус продажи

    public string? Comment { get; set; } // Комментарий к продаже

    public List<SaleLine> Lines { get; set; } = new(); // Позиции продажи
}