// ViewModels/Sales/SaleEditViewModel.cs

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TradeSystem.App.ViewModels.Abstractions;
using TradeSystem.Application.Contracts.Services;
using TradeSystem.Application.Dto;

// Файл содержит класс SaleEditViewModel,
// который представляет ViewModel формы создания продажи товара в торговой системе.

namespace TradeSystem.App.ViewModels.Sales;

public partial class SaleEditViewModel : ViewModelBase
{
    private readonly ISaleService _sales; // Сервис продаж
    private readonly ICatalogService _catalog; // Сервис справочных данных
    private readonly IStockService _stock; // Сервис складских остатков

    [ObservableProperty] private SaleLineViewModel? _selectedLine; // Выбранная строка продажи
    [ObservableProperty] private IReadOnlyList<ProductDto> _availableProducts = Array.Empty<ProductDto>(); // Список товаров для выбора

    public ObservableCollection<SaleLineViewModel> Lines { get; } = new(); // Строки продажи

    public decimal TotalAmount => Lines.Sum(l => l.Total); // Общая сумма продажи

    public event Action? CloseRequested; // Запрос на закрытие формы

    public SaleEditViewModel(
        ISaleService sales,
        ICatalogService catalog,
        IStockService stock,
        IDialogService dialog)
        : base(dialog)
    {
        _sales = sales;
        _catalog = catalog;
        _stock = stock;

        // Живой итог: пересчитываем при изменении состава и содержимого строк.
        Lines.CollectionChanged += OnLinesCollectionChanged;
    }

    public async Task LoadAsync() // Загрузить данные формы
    {
        await RunAsync(async () =>
        {
            SelectedLine = null;
            Lines.Clear();
            AvailableProducts = await _catalog.GetProductsAsync(includeInactive: false);
            OnPropertyChanged(nameof(TotalAmount));
        });
    }

    // ---- строки ----

    [RelayCommand]
    private void AddLine() // Добавить строку продажи
    {
        var line = new SaleLineViewModel();
        line.ProductChanged += pid => _ = ReloadShelvesAsync(line, pid);
        Lines.Add(line);
        SelectedLine = line;
    }

    [RelayCommand(CanExecute = nameof(HasLineSelection))]
    private void RemoveLine() // Удалить выбранную строку
    {
        if (SelectedLine is null) return;

        SelectedLine.PropertyChanged -= OnLinePropertyChanged;
        // отписка ProductChanged не требуется: подписка «строка → лямбда редактора»
        // не держит строку живой, а после Lines.Remove на неё нет ссылок.
        Lines.Remove(SelectedLine);
        SelectedLine = Lines.LastOrDefault();
    }

    private bool HasLineSelection => SelectedLine is not null; // Признак наличия выбранной строки

    private void OnLinesCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e) // Реакция на изменение состава строк
    {
        if (e.NewItems is not null)
            foreach (SaleLineViewModel l in e.NewItems)
                l.PropertyChanged += OnLinePropertyChanged;

        if (e.OldItems is not null)
            foreach (SaleLineViewModel l in e.OldItems)
                l.PropertyChanged -= OnLinePropertyChanged;

        OnPropertyChanged(nameof(TotalAmount));
    }

    private void OnLinePropertyChanged(object? sender, PropertyChangedEventArgs e) // Реакция на изменение свойств строки
    {
        if (e.PropertyName is nameof(SaleLineViewModel.Quantity)
                          or nameof(SaleLineViewModel.UnitPrice))
        {
            OnPropertyChanged(nameof(TotalAmount));
        }
    }

    // ---- подгрузка полок для строки (с защитой от гонки) ----

    private async Task ReloadShelvesAsync(SaleLineViewModel line, Guid productId) // Загрузить полки с остатком товара
    {
        if (productId == Guid.Empty)
        {
            line.AvailableShelves = Array.Empty<ShelfStockOption>();
            line.ShelfId = Guid.Empty;
            return;
        }

        var balances = await _stock.GetBalancesByProductAsync(productId);

        // Гонка: если за время await товар сменился — отбрасываем устаревший результат.
        if (line.ProductId != productId)
            return;

        line.AvailableShelves = balances
            .Where(b => b.Quantity > 0)
            .Select(b => new ShelfStockOption(b.ShelfId, b.ShelfCode, b.ShelfName, b.Quantity))
            .OrderBy(o => o.ShelfCode, StringComparer.CurrentCultureIgnoreCase)
            .ToList();

        line.ShelfId = Guid.Empty; // прежняя полка невалидна для нового товара
    }

    // ---- сохранение / проведение ----

    [RelayCommand]
    private Task SaveDraftAsync() => RunAsync(async () => // Сохранить черновик продажи
    {
        if (!TryBuildInput(out var input)) return;
        await _sales.CreateDraftAsync(input);
        CloseRequested?.Invoke();
    });

    [RelayCommand]
    private Task SaveAndPostAsync() => RunAsync(async () => // Сохранить и провести продажу
    {
        if (!TryBuildInput(out var input)) return;
        var draft = await _sales.CreateDraftAsync(input);
        await _sales.PostAsync(draft.Id);
        CloseRequested?.Invoke();
    });

    [RelayCommand]
    private void Cancel() => CloseRequested?.Invoke(); // Команда отмены создания продажи

    private bool TryBuildInput(out SaleInput input) // Построить входные данные продажи
    {
        input = new SaleInput();

        var errors = Validate();
        if (errors.Count > 0)
        {
            Dialog.ShowError(string.Join(Environment.NewLine, errors));
            return false;
        }

        input.Lines = Lines.Select(l => new SaleLineInput
        {
            ProductId = l.ProductId,
            ShelfId = l.ShelfId,
            Quantity = l.Quantity,
            UnitPrice = l.UnitPrice
        }).ToList();

        return true;
    }

    private List<string> Validate() // Проверить корректность введённых данных
    {
        var errors = new List<string>();

        if (Lines.Count == 0)
            errors.Add("Добавьте хотя бы одну позицию продажи.");

        for (int i = 0; i < Lines.Count; i++)
        {
            var l = Lines[i];
            var n = i + 1;

            if (l.ProductId == Guid.Empty)
                errors.Add($"Позиция {n}: не выбран товар.");
            if (l.ShelfId == Guid.Empty)
                errors.Add($"Позиция {n}: не выбрана полка.");
            if (l.Quantity <= 0)
                errors.Add($"Позиция {n}: количество должно быть больше нуля.");
            if (l.UnitPrice < 0)
                errors.Add($"Позиция {n}: цена продажи не может быть отрицательной.");

            var available = l.AvailableOnShelf;
            if (l.ShelfId != Guid.Empty && l.Quantity > available)
                errors.Add($"Позиция {n}: на полке доступно {available}, запрошено {l.Quantity}.");
        }

        // Одна и та же пара товар+полка в нескольких строках теряет смысл.
        var dup = Lines
            .Where(l => l.ProductId != Guid.Empty && l.ShelfId != Guid.Empty)
            .GroupBy(l => (l.ProductId, l.ShelfId))
            .FirstOrDefault(g => g.Count() > 1);
        if (dup is not null)
            errors.Add("Один и тот же товар на одной полке указан в нескольких позициях.");

        return errors;
    }
}