// ViewModels/Reports/ReportKind.cs

// Файл содержит перечисление ReportKind и запись ReportKindOption,
// которые определяют виды справочных отчётов торговой системы и их отображаемые названия.

namespace TradeSystem.App.ViewModels.Reports;

/// <summary>Четыре справочных списка из задания 38.</summary>
public enum ReportKind
{
    AllProducts = 0, // Список всех товаров
    Available = 1, // Товары в наличии
    ToReplenish = 2, // Товары к пополнению
    SupplierProducts = 3 // Товары поставщика
}

/// <summary>Пара «значение + метка» для привязки к ComboBox (без x:Static в XAML).</summary>
public sealed record ReportKindOption(ReportKind Value, string Label); // Вид отчёта и его отображаемое название