// ViewModels/Reports/ReportsViewModel.cs

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TradeSystem.App.ViewModels.Abstractions;
using TradeSystem.Application.Contracts.Services;
using TradeSystem.Application.Dto;
using TradeSystem.Application.Reports;

// Файл содержит класс ReportsViewModel,
// который представляет ViewModel формирования справочных отчётов с возможностью печати и экспорта.

namespace TradeSystem.App.ViewModels.Reports;

public partial class ReportsViewModel : ViewModelBase, IActivatable
{
    private readonly IReportService _reports; // Сервис отчётов
    private readonly ICatalogService _catalog; // Сервис справочных данных
    private readonly IDocumentOutputService _output; // Сервис вывода документов

    // Сформированный документ для печати/экспорта. Не биндится — обычное поле.
    private ReportDocument? _document;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsSupplierReport))]
    private ReportKind _selectedKind = ReportKind.AllProducts; // Выбранный вид отчёта

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(GenerateCommand))]
    private Guid _selectedSupplierId; // Идентификатор выбранного поставщика

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(PrintCommand))]
    [NotifyCanExecuteChangedFor(nameof(SaveTextCommand))]
    [NotifyCanExecuteChangedFor(nameof(SaveCsvCommand))]
    private string _previewText = string.Empty; // Текстовый предпросмотр отчёта

    [ObservableProperty] private IReadOnlyList<SupplierDto> _suppliers = Array.Empty<SupplierDto>(); // Список поставщиков для выбора

    public IReadOnlyList<ReportKindOption> AvailableKinds { get; } = new[] // Список доступных видов отчётов
    {
        new ReportKindOption(ReportKind.AllProducts, "Список всех товаров"),
        new ReportKindOption(ReportKind.Available, "Товары в наличии"),
        new ReportKindOption(ReportKind.ToReplenish, "Товары к пополнению"),
        new ReportKindOption(ReportKind.SupplierProducts, "Товары данного поставщика")
    };

    /// <summary>ComboBox поставщика виден только для отчёта по поставщику.</summary>
    public bool IsSupplierReport => SelectedKind == ReportKind.SupplierProducts; // Признак отчёта по поставщику

    public ReportsViewModel(
        IReportService reports,
        ICatalogService catalog,
        IDocumentOutputService output,
        IDialogService dialog)
        : base(dialog)
    {
        _reports = reports;
        _catalog = catalog;
        _output = output;
    }

    public async Task OnActivatedAsync() // Загрузить данные при активации ViewModel
    {
        await RunAsync(async () =>
        {
            Suppliers = await _catalog.GetSuppliersAsync(includeInactive: false);
            // Сбрасываем предыдущий документ при входе, чтобы не печатать устаревшее.
            _document = null;
            PreviewText = string.Empty;
        });
    }

    [RelayCommand(CanExecute = nameof(CanGenerate))]
    private Task GenerateAsync() => RunAsync(GenerateImplementationAsync); // Команда формирования отчёта

    private bool CanGenerate => // Условие доступности формирования отчёта
        SelectedKind != ReportKind.SupplierProducts || SelectedSupplierId != Guid.Empty;

    private async Task GenerateImplementationAsync() // Сформировать выбранный отчёт
    {
        ReportDocument doc = SelectedKind switch
        {
            ReportKind.AllProducts => ReportDocuments.FromAllProducts(await _reports.GetAllProductsReportAsync()),
            ReportKind.Available => ReportDocuments.FromAvailableProducts(await _reports.GetAvailableProductsReportAsync()),
            ReportKind.ToReplenish => ReportDocuments.FromToReplenish(await _reports.GetToReplenishReportAsync()),
            ReportKind.SupplierProducts => ReportDocuments.FromSupplierProducts(
                SupplierName(), await _reports.GetSupplierProductsReportAsync(SelectedSupplierId)),
            _ => throw new ArgumentOutOfRangeException()
        };

        _document = doc;
        PreviewText = doc.ToPlainText();
    }

    private string SupplierName() => // Получить имя выбранного поставщика
        Suppliers.FirstOrDefault(s => s.Id == SelectedSupplierId)?.Name ?? "—";

    [RelayCommand(CanExecute = nameof(HasDocument))]
    private void Print() // Отправить документ на печать
    {
        if (_document is null) return;

        if (_output.Print(_document))
            Dialog.ShowInfo("Документ отправлен на печать.", "Печать");
        else
            Dialog.ShowWarning("Печать отменена или принтер недоступен.", "Печать");
    }

    [RelayCommand(CanExecute = nameof(HasDocument))]
    private void SaveText() => Save(_document!.ToPlainText(), ".txt", "Текстовый файл|*.txt"); // Сохранить документ как текст

    [RelayCommand(CanExecute = nameof(HasDocument))]
    private void SaveCsv() => Save(_document!.ToCsv(), ".csv", "CSV (разделитель ;) |*.csv"); // Сохранить документ как CSV

    private void Save(string content, string extension, string filter) // Сохранить документ в файл
    {
        if (_document is null) return;

        var safeTitle = string.Concat(_document.Title.Select(c => c == '/' || c == '\\' || c == ':' || c == '*' || c == '?' || c == '"' || c == '<' || c == '>' || c == '|' ? '_' : c));
        var fileName = safeTitle + extension;

        if (_output.SaveDocument(content, fileName, filter))
            Dialog.ShowInfo($"Документ сохранён: {fileName}", "Экспорт");
        else
            Dialog.ShowWarning("Сохранение отменено или недоступно.", "Экспорт");
    }

    private bool HasDocument => _document is not null; // Признак наличия сформированного документа

    // Смена вида отчёта сбрасывает документ и пересчитывает доступность команд.
    partial void OnSelectedKindChanged(ReportKind value) // Реакция на смену вида отчёта
    {
        _document = null;
        PreviewText = string.Empty;
    }
}