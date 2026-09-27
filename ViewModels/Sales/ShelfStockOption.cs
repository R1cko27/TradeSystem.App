// ViewModels/Sales/ShelfStockOption.cs

using System;

namespace TradeSystem.App.ViewModels.Sales;

/// <summary>
/// Полка, на которой выбранного товара есть остаток. Используется как
/// источник для ComboBox «Полка» в строке продажи, чтобы нельзя было
/// указать полку, где товара нет.
/// </summary>
public sealed record ShelfStockOption(Guid ShelfId, string ShelfCode, string ShelfName, int Available)
{
    public string Display => $"{ShelfCode} · {ShelfName} (остаток {Available})";
}