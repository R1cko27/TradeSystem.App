// ViewModels/Receipts/ReceiptLineViewModel.cs

using System;
using CommunityToolkit.Mvvm.ComponentModel;

// Файл содержит класс ReceiptLineViewModel,
// который представляет одну строку поступления товара в торговой системе.

namespace TradeSystem.App.ViewModels.Receipts;

/// <summary>
/// Строка поступления. Наблюдаемые поля (товар, полка, количество) меняются
/// пользователем; служебные (ссылка на строку заказа, редактируемость товара)
/// фиксируются при создании строки и потому не требуют уведомлений.
/// </summary>
public partial class ReceiptLineViewModel : ObservableObject
{
    [ObservableProperty] private Guid _productId; // Идентификатор товара
    [ObservableProperty] private Guid _shelfId; // Идентификатор полки
    [ObservableProperty] private int _quantity = 1; // Количество товара

    /// <summary>Верхняя граница количества (остаток по строке заказа). null — без лимита.</summary>
    [ObservableProperty] private int? _maxQuantity; // Верхняя граница количества

    /// <summary>Ссылка на строку заказа (для режима «по заказу»). Не меняется после создания.</summary>
    public Guid? OrderLineId { get; set; } // Идентификатор строки заказа

    /// <summary>Можно ли менять товар (true — свободный ввод, false — товар из заказа).</summary>
    public bool IsProductEditable { get; set; } // Признак возможности менять товар
}