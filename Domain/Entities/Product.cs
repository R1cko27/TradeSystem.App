// Domain/Entities/Product.cs

using System;
using TradeSystem.Domain.Enums;

// Файл содержит класс Product,
// который представляет продукт в торговой системе.

namespace TradeSystem.Domain.Entities;

public sealed class Product : Entity
{
    public string Name { get; set; } = string.Empty; // Название продукта

    public string Article { get; set; } = string.Empty; // Артикул продукта

    public MeasurementUnit Unit { get; set; } = MeasurementUnit.NotSpecified; // Единица измерения продукта

    public int MinimumStockQuantity { get; set; } // Минимальное количество продукта на складе

    public int? TargetStockQuantity { get; set; } // Целевое количество продукта на складе (необязательное поле)

    public bool IsActive { get; set; } = true; // Флаг активности продукта

    public string? Description { get; set; } // Описание продукта

    public Guid? DefaultShelfId { get; set; } // Идентификатор полки по умолчанию
}