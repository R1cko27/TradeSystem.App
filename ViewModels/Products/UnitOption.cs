// ViewModels/Products/UnitOption.cs

using TradeSystem.Domain.Enums;

// Файл содержит класс UnitOption,
// который представляет пару «единица измерения + её отображаемое название» для привязки в пользовательском интерфейсе.

namespace TradeSystem.App.ViewModels.Products;

/// <summary>Пара «значение enum + человекочитаемая метка» для привязки к ComboBox.</summary>
public sealed record UnitOption(MeasurementUnit Value, string Label); // Единица измерения и её отображаемое название