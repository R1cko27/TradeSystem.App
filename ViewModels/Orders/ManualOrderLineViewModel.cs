// ViewModels/Orders/ManualOrderLineViewModel.cs

using System;
using CommunityToolkit.Mvvm.ComponentModel;
using TradeSystem.Application.Dto;

// Файл содержит класс ManualOrderLineViewModel,
// который представляет одну позицию вручную создаваемого заказа на покупку в торговой системе.

namespace TradeSystem.App.ViewModels.Orders;

/// <summary>
/// Одна позиция будущего заказа. Держит выбранный вариант «товар-поставщик»
/// целиком, поэтому ProductId и цена всегда согласованы между собой.
/// </summary>
public partial class ManualOrderLineViewModel : ObservableObject
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Total))]
    [NotifyPropertyChangedFor(nameof(UnitPrice))]
    private SupplierProductDto? _selectedSupplierProduct; // Выбранный вариант «товар-поставщик»

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Total))]
    private int _quantity = 1; // Количество товара в позиции

    public Guid ProductId => SelectedSupplierProduct?.ProductId ?? Guid.Empty; // Идентификатор товара
    public Guid SupplierProductId => SelectedSupplierProduct?.Id ?? Guid.Empty; // Идентификатор связи товара и поставщика

    public decimal UnitPrice => SelectedSupplierProduct?.PurchasePrice ?? 0m; // Цена за единицу

    public decimal Total => UnitPrice * Math.Max(0, Quantity); // Общая стоимость позиции
}