// Domain/Entities/SupplierProduct.cs

using System;

// Файл содержит класс SupplierProduct,
// который представляет связь между поставщиком и продуктом в торговой системе.

namespace TradeSystem.Domain.Entities;

public sealed class SupplierProduct : Entity
{
    public Guid ProductId { get; set; } // Идентификатор продукта

    public Guid SupplierId { get; set; } // Идентификатор поставщика

    public string? SupplierArticle { get; set; } // Артикул продукта у поставщика

    public decimal? PurchasePrice { get; set; } // Цена покупки

    public int? LeadTimeDays { get; set; } // Время доставки в днях

    public int? MinimumOrderQuantity { get; set; } // Минимальный заказ

    public int? OrderMultiple { get; set; } // Кратность заказа

    public bool IsPreferred { get; set; } // Флаг предпочтительного поставщика

    public bool IsActive { get; set; } = true; // Флаг активности связи между поставщиком и продуктом

    public string? Note { get; set; } // Примечание о связи между поставщиком и продуктом
}