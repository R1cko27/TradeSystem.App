// Application/Dto/PreviewDtos.cs

using System;
using System.Collections.Generic;

// Файл содержит набор DTO-классов,
// которые представляют предварительный план автоформирования заказов без записи в хранилище торговой системы.

namespace TradeSystem.Application.Dto;

/// <summary>
/// READ-ONLY план автоформирования: что будет создано, но без записи в хранилище.
/// Позволяет мастеру показать пользователю разбивку и варианты поставщиков
/// до подтверждения, не дублируя бизнес-правила выбора в UI.
/// </summary>
public sealed class AutoOrderPreviewDto
{
    public IReadOnlyList<PreviewProductDto> Products { get; init; } = Array.Empty<PreviewProductDto>(); // Товары в плане автоформирования
    public IReadOnlyList<string> Warnings { get; init; } = Array.Empty<string>(); // Предупреждения, возникшие при построении плана
}

public sealed class PreviewProductDto // DTO товара в предварительном плане
{
    public Guid ProductId { get; init; } // Идентификатор товара
    public string ProductName { get; init; } = string.Empty; // Наименование товара
    public string UnitLabel { get; init; } = string.Empty; // Название единицы измерения
    public int CurrentStock { get; init; } // Текущий остаток
    public int MinimumStockQuantity { get; init; } // Минимальный остаток

    /// <summary>Потребность до корректировки под конкретного поставщика.</summary>
    public int BaseNeededQuantity { get; init; } // Базовая потребность в товаре

    public IReadOnlyList<PreviewSupplierOptionDto> Options { get; init; } = Array.Empty<PreviewSupplierOptionDto>(); // Варианты поставщиков для товара

    /// <summary>Рекомендованный поставщик (с учётом явного выбора, если был передан).</summary>
    public Guid? RecommendedSupplierId { get; init; } // Идентификатор рекомендованного поставщика
}

public sealed class PreviewSupplierOptionDto // DTO варианта поставщика в предварительном плане
{
    public Guid SupplierId { get; init; } // Идентификатор поставщика
    public string SupplierName { get; init; } = string.Empty; // Название поставщика
    public Guid SupplierProductId { get; init; } // Идентификатор связи товара и поставщика
    public decimal? PurchasePrice { get; init; } // Закупочная цена
    public int? LeadTimeDays { get; init; } // Срок поставки в днях
    public int? MinimumOrderQuantity { get; init; } // Минимальное количество в заказе
    public int? OrderMultiple { get; init; } // Кратность заказа
    public bool IsPreferred { get; init; } // Признак предпочтительного поставщика

    /// <summary>Количество с учётом минималки и кратности именно этого поставщика.</summary>
    public int AdjustedQuantity { get; init; } // Скорректированное количество

    public decimal LineAmount { get; init; } // Сумма по позиции
}