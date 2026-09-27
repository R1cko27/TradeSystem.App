// Converters/NullToVisibilityConverters.cs

using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

// Файл содержит конвертеры NullToVisibilityConverter и NotNullToVisibilityConverter,
// которые преобразуют null/не-null значения в Visibility для привязок в пользовательском интерфейсе торговой системы.

namespace TradeSystem.App.Converters;

/// <summary>null → Visible, не null → Collapsed (подсказка «выберите заказ»).</summary>
public sealed class NullToVisibilityConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) // null → Visible, не null → Collapsed
        => value is null ? Visibility.Visible : Visibility.Collapsed;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) // Обратное преобразование не поддерживается
        => throw new NotSupportedException();
}

/// <summary>не null → Visible, null → Collapsed (панель деталей).</summary>
public sealed class NotNullToVisibilityConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) // не null → Visible, null → Collapsed
        => value is not null ? Visibility.Visible : Visibility.Collapsed;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) // Обратное преобразование не поддерживается
        => throw new NotSupportedException();
}