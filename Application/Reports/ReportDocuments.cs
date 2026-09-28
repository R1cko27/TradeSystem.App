// Application/Reports/ReportDocuments.cs

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using TradeSystem.Application.Dto;

// Файл содержит класс ReportDocuments,
// который предоставляет фабричные методы для построения справочных документов из DTO торговой системы.

namespace TradeSystem.Application.Reports;

/// <summary>
/// Фабрика справочных документов из DTO. Чистые функции без состояния и зависимостей,
/// поэтому статический класс уместнее, чем ещё один инъектируемый сервис.
/// </summary>
public static class ReportDocuments
{
    private static readonly CultureInfo Cul = CultureInfo.CurrentCulture; // Текущая культура для форматирования

    public static ReportDocument FromAllProducts(IReadOnlyList<ProductDto> items) // Отчёт по всем товарам
    {
        var headers = new[] { "Наименование", "Артикул", "Ед.", "Остаток", "Мин.", "Целевой", "Поставщиков", "Пополнить" };

        var rows = items.Select(p => new[]
        {
            p.Name,
            p.Article,
            p.UnitLabel,
            Num(p.CurrentStock),
            Num(p.MinimumStockQuantity),
            p.TargetStockQuantity is { } t ? Num(t) : "—",
            Num(p.SuppliersCount),
            YesNo(p.NeedsReplenish)
        }).ToList();

        return new ReportDocument("Список всех товаров", headers, rows);
    }

    public static ReportDocument FromAvailableProducts(IReadOnlyList<ProductDto> items) // Отчёт по товарам в наличии
    {
        var headers = new[] { "Наименование", "Артикул", "Ед.", "Остаток", "Полка по умолчанию" };

        var rows = items.Select(p => new[]
        {
            p.Name,
            p.Article,
            p.UnitLabel,
            Num(p.CurrentStock),
            p.DefaultShelfName ?? "—"
        }).ToList();

        return new ReportDocument("Товары, имеющиеся в наличии", headers, rows);
    }

    public static ReportDocument FromToReplenish(IReadOnlyList<LowStockProductDto> items) // Отчёт по товарам к пополнению
    {
        var headers = new[] { "Наименование", "Ед.", "Текущий остаток", "Минимум", "Рекомендуется заказать" };

        var rows = items.Select(p => new[]
        {
            p.ProductName,
            p.UnitLabel,
            Num(p.CurrentStock),
            Num(p.MinimumStockQuantity),
            Num(p.RecommendedOrderQuantity)
        }).ToList();

        return new ReportDocument("Товары, количество которых необходимо пополнить", headers, rows);
    }

    public static ReportDocument FromSupplierProducts(string supplierName, IReadOnlyList<SupplierProductDto> items) // Отчёт по товарам поставщика
    {
        var headers = new[] { "Товар", "Артикул поставщика", "Цена закупки", "Срок, дн.", "Мин. заказ", "Кратность", "Предпочт." };

        var rows = items.Select(s => new[]
        {
            s.ProductName,
            s.SupplierArticle ?? "—",
            s.PurchasePrice is { } price ? price.ToString("N2", Cul) : "—",
            s.LeadTimeDays is { } d ? Num(d) : "—",
            s.MinimumOrderQuantity is { } m ? Num(m) : "—",
            s.OrderMultiple is { } k ? Num(k) : "—",
            YesNo(s.IsPreferred)
        }).ToList();

        return new ReportDocument($"Товары, поставляемые: {supplierName}", headers, rows);
    }

    private static string Num(int value) => value.ToString(Cul); // Отформатировать целое число
    private static string YesNo(bool value) => value ? "Да" : ""; // Отформатировать булево значение
}