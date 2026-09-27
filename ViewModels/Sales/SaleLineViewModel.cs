// ViewModels/Sales/SaleLineViewModel.cs

using System;
using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;

// Файл содержит класс SaleLineViewModel,
// который представляет одну строку продажи товара в торговой системе.

namespace TradeSystem.App.ViewModels.Sales;

public partial class SaleLineViewModel : ObservableObject
{
    [ObservableProperty] private Guid _productId; // Идентификатор товара
    [ObservableProperty] private Guid _shelfId; // Идентификатор полки

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Total))]
    private int _quantity = 1; // Количество товара

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Total))]
    private decimal _unitPrice; // Цена за единицу

    // Доступные полки для текущего товара (заполняет редактор по событию ProductChanged).
    [ObservableProperty] private IReadOnlyList<ShelfStockOption> _availableShelves = Array.Empty<ShelfStockOption>(); // Список полок с остатком товара

    /// <summary>Поднимается при смене товара, чтобы редактор перезагрузил полки.</summary>
    public event Action<Guid>? ProductChanged; // Событие смены товара

    partial void OnProductIdChanged(Guid value) => ProductChanged?.Invoke(value); // Реакция на смену товара

    public decimal Total => UnitPrice * Math.Max(0, Quantity); // Общая стоимость позиции

    /// <summary>Сколько товара выбранного вида лежит на выбранной полке (0, если полка не выбрана/нет остатка).</summary>
    public int AvailableOnShelf => // Доступное количество товара на выбранной полке
        ShelfId == Guid.Empty
            ? 0
            : AvailableShelves.FirstOrDefault(s => s.ShelfId == ShelfId)?.Available ?? 0;
}