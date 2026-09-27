// ViewModels/Orders/PreviewRowViewModel.cs

using System;
using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using TradeSystem.Application.Dto;

// Файл содержит класс PreviewRowViewModel,
// который представляет строку предпросмотра автоформирования заказов с возможностью выбора поставщика.

namespace TradeSystem.App.ViewModels.Orders;

/// <summary>
/// Обёртка над read-only DTO предпросмотра: добавляет наблюдаемый выбор
/// поставщика (для товаров с несколькими вариантами) и признак «дополнительный».
/// </summary>
public partial class PreviewRowViewModel : ObservableObject
{
    private readonly PreviewProductDto _source; // Исходный DTO товара предпросмотра

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SelectedSupplierId))]
    private PreviewSupplierOptionDto? _selectedOption; // Выбранный вариант поставщика

    public PreviewRowViewModel(PreviewProductDto source, bool isAdditional) // Создать строку предпросмотра
    {
        _source = source;
        IsAdditional = isAdditional;
        Options = source.Options;
        SelectedOption = source.Options.FirstOrDefault(o => o.SupplierId == source.RecommendedSupplierId)
                         ?? source.Options.FirstOrDefault();
    }

    public bool IsAdditional { get; } // Признак дополнительного товара
    public IReadOnlyList<PreviewSupplierOptionDto> Options { get; } // Варианты поставщиков для товара

    public Guid ProductId => _source.ProductId; // Идентификатор товара
    public string ProductName => _source.ProductName; // Наименование товара
    public string UnitLabel => _source.UnitLabel; // Название единицы измерения
    public int CurrentStock => _source.CurrentStock; // Текущий остаток
    public int MinimumStockQuantity => _source.MinimumStockQuantity; // Минимальный остаток
    public bool HasMultipleOptions => Options.Count > 1; // Признак наличия нескольких поставщиков
    public Guid SelectedSupplierId => SelectedOption?.SupplierId ?? Guid.Empty; // Идентификатор выбранного поставщика
}