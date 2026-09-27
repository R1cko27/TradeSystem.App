// ViewModels/Receipts/ReceiptEditViewModel.cs

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TradeSystem.App.ViewModels.Abstractions;
using TradeSystem.Application.Contracts.Services;
using TradeSystem.Application.Dto;
using TradeSystem.Domain.Enums;

// Файл содержит класс ReceiptEditViewModel,
// который представляет ViewModel формы создания поступления товара в торговой системе.

namespace TradeSystem.App.ViewModels.Receipts;

/// <summary>Источник строк поступления.</summary>
public enum ReceiptSourceMode
{
    ByOrder = 0, // Строки берутся из заказа
    Free = 1     // Строки вводятся свободно
}

public partial class ReceiptEditViewModel : ViewModelBase
{
    private readonly IGoodsReceiptService _receipts; // Сервис поступлений товаров
    private readonly IPurchaseOrderService _orders; // Сервис заказов на покупку
    private readonly ICatalogService _catalog; // Сервис справочных данных

    // Полный набор заказов (для фильтра по поставщику) и словарь «товар -> полка по умолчанию».
    private IReadOnlyList<PurchaseOrderDto> _allOrders = Array.Empty<PurchaseOrderDto>();
    private Dictionary<Guid, Guid?> _defaultShelfByProduct = new();

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(LoadOrderLinesCommand))]
    [NotifyCanExecuteChangedFor(nameof(AddLineCommand))]
    [NotifyCanExecuteChangedFor(nameof(RemoveLineCommand))]
    private ReceiptSourceMode _sourceMode = ReceiptSourceMode.ByOrder; // Режим источника строк

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(LoadOrderLinesCommand))]
    private Guid _supplierId; // Идентификатор выбранного поставщика

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(LoadOrderLinesCommand))]
    private Guid _selectedOrderId; // Идентификатор выбранного заказа

    [ObservableProperty] private ReceiptLineViewModel? _selectedLine; // Выбранная строка поступления
    [ObservableProperty] private IReadOnlyList<SupplierDto> _suppliers = Array.Empty<SupplierDto>(); // Список поставщиков
    [ObservableProperty] private IReadOnlyList<ShelfDto> _shelves = Array.Empty<ShelfDto>(); // Список полок
    [ObservableProperty] private IReadOnlyList<SupplierProductDto> _availableProducts = Array.Empty<SupplierProductDto>(); // Товары выбранного поставщика
    [ObservableProperty] private IReadOnlyList<PurchaseOrderDto> _ordersForSupplier = Array.Empty<PurchaseOrderDto>(); // Заказы выбранного поставщика

    public ObservableCollection<ReceiptLineViewModel> Lines { get; } = new(); // Строки поступления

    public event Action? CloseRequested; // Запрос на закрытие формы

    public ReceiptEditViewModel(
        IGoodsReceiptService receipts,
        IPurchaseOrderService orders,
        ICatalogService catalog,
        IDialogService dialog)
        : base(dialog)
    {
        _receipts = receipts;
        _orders = orders;
        _catalog = catalog;
    }

    public async Task LoadAsync() // Загрузить данные формы
    {
        await RunAsync(async () =>
        {
            SourceMode = ReceiptSourceMode.ByOrder;
            SupplierId = Guid.Empty;
            SelectedOrderId = Guid.Empty;
            SelectedLine = null;
            Lines.Clear();

            var suppliersTask = _catalog.GetSuppliersAsync(includeInactive: false);
            var shelvesTask = _catalog.GetShelvesAsync(includeInactive: false);
            var productsTask = _catalog.GetProductsAsync(includeInactive: false);
            var ordersTask = _orders.GetOrdersAsync();
            await Task.WhenAll(suppliersTask, shelvesTask, productsTask, ordersTask);

            Suppliers = await suppliersTask;
            Shelves = await shelvesTask;
            _allOrders = await ordersTask;

            var products = await productsTask;
            _defaultShelfByProduct = products.ToDictionary(p => p.Id, p => p.DefaultShelfId);

            AvailableProducts = Array.Empty<SupplierProductDto>();
            OrdersForSupplier = Array.Empty<PurchaseOrderDto>();
        });
    }

    // ---- реакция на выбор поставщика ----
    partial void OnSupplierIdChanged(Guid value) => _ = HandleSupplierChangedAsync(value); // Реакция на смену поставщика

    private async Task HandleSupplierChangedAsync(Guid supplierId) // Обработать смену поставщика
    {
        // Смена поставщика сбрасывает заказ и строки: они принадлежат другому поставщику.
        SelectedOrderId = Guid.Empty;
        Lines.Clear();
        SelectedLine = null;

        if (supplierId == Guid.Empty)
        {
            AvailableProducts = Array.Empty<SupplierProductDto>();
            OrdersForSupplier = Array.Empty<PurchaseOrderDto>();
            return;
        }

        await RunAsync(async () =>
        {
            AvailableProducts = await _catalog.GetSupplierProductsBySupplierAsync(supplierId);
            OrdersForSupplier = _allOrders
                .Where(o => o.SupplierId == supplierId
                            && o.Status != PurchaseOrderStatus.Cancelled
                            && o.Lines.Any(l => !l.IsClosed && l.RemainingQuantity > 0))
                .OrderByDescending(o => o.OrderDateUtc)
                .ToList();
        });
    }

    // ---- реакция на смену режима / заказа ----
    partial void OnSourceModeChanged(ReceiptSourceMode value) // Реакция на смену режима источника строк
    {
        Lines.Clear();
        SelectedLine = null;
    }

    partial void OnSelectedOrderIdChanged(Guid value) // Реакция на смену заказа
    {
        // Не перегружаем строки молча, чтобы не потерять ручной ввод: очистка + явная кнопка.
        Lines.Clear();
        SelectedLine = null;
    }

    // ---- команды строк ----
    [RelayCommand(CanExecute = nameof(CanLoadOrderLines))]
    private Task LoadOrderLinesAsync() => RunAsync(async () => // Загрузить строки из заказа
    {
        var order = _allOrders.FirstOrDefault(o => o.Id == SelectedOrderId);
        if (order is null)
        {
            Dialog.ShowWarning("Заказ не найден.");
            return;
        }

        var openLines = order.Lines.Where(l => !l.IsClosed && l.RemainingQuantity > 0).ToList();
        if (openLines.Count == 0)
        {
            Dialog.ShowInfo("У этого заказа нет незакрытых позиций для прихода.");
            return;
        }

        Lines.Clear();
        foreach (var l in openLines)
        {
            _defaultShelfByProduct.TryGetValue(l.ProductId, out var defShelf);
            Lines.Add(new ReceiptLineViewModel
            {
                ProductId = l.ProductId,
                ShelfId = defShelf ?? Guid.Empty,
                Quantity = l.RemainingQuantity,
                MaxQuantity = l.RemainingQuantity,
                OrderLineId = l.Id,
                IsProductEditable = false
            });
        }
        SelectedLine = Lines.FirstOrDefault();
    });

    private bool CanLoadOrderLines => // Условие доступности загрузки строк из заказа
        SourceMode == ReceiptSourceMode.ByOrder && SelectedOrderId != Guid.Empty;

    [RelayCommand(CanExecute = nameof(CanAddLine))]
    private void AddLine() // Добавить новую строку
    {
        Lines.Add(new ReceiptLineViewModel { IsProductEditable = true });
        SelectedLine = Lines.Last();
    }

    private bool CanAddLine => SourceMode == ReceiptSourceMode.Free && SupplierId != Guid.Empty; // Условие доступности добавления строки

    [RelayCommand(CanExecute = nameof(HasLineSelection))]
    private void RemoveLine() // Удалить выбранную строку
    {
        if (SelectedLine is null) return;
        Lines.Remove(SelectedLine);
        SelectedLine = Lines.LastOrDefault();
    }

    private bool HasLineSelection => SelectedLine is not null; // Признак наличия выбранной строки

    // ---- сохранение / проведение ----
    [RelayCommand]
    private Task SaveDraftAsync() => RunAsync(async () => // Сохранить черновик поступления
    {
        if (!TryBuildInput(out var input)) return;
        await _receipts.CreateDraftAsync(input);
        CloseRequested?.Invoke();
    });

    [RelayCommand]
    private Task SaveAndPostAsync() => RunAsync(async () => // Сохранить и провести поступление
    {
        if (!TryBuildInput(out var input)) return;
        var draft = await _receipts.CreateDraftAsync(input);
        await _receipts.PostAsync(draft.Id);
        CloseRequested?.Invoke();
    });

    [RelayCommand]
    private void Cancel() => CloseRequested?.Invoke(); // Команда отмены создания поступления

    private bool TryBuildInput(out GoodsReceiptInput input) // Построить входные данные поступления
    {
        input = new GoodsReceiptInput();

        var errors = Validate();
        if (errors.Count > 0)
        {
            Dialog.ShowError(string.Join(Environment.NewLine, errors));
            return false;
        }

        input.SupplierId = SupplierId;
        input.OrderId = SourceMode == ReceiptSourceMode.ByOrder ? SelectedOrderId : null;
        input.Lines = Lines.Select(l => new GoodsReceiptLineInput
        {
            OrderLineId = l.OrderLineId,
            ProductId = l.ProductId,
            ShelfId = l.ShelfId,
            Quantity = l.Quantity
        }).ToList();

        return true;
    }

    private List<string> Validate() // Проверить корректность введённых данных
    {
        var errors = new List<string>();

        if (SupplierId == Guid.Empty)
            errors.Add("Выберите поставщика.");
        if (SourceMode == ReceiptSourceMode.ByOrder && SelectedOrderId == Guid.Empty)
            errors.Add("Выберите заказ (или переключитесь в свободный режим).");
        if (Lines.Count == 0)
            errors.Add("Добавьте хотя бы одну позицию поступления.");

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
            if (l.MaxQuantity is { } max && l.Quantity > max)
                errors.Add($"Позиция {n}: количество больше остатка по заказу ({max}).");
        }

        // Один товар на одну полку в рамках одного поступления — иначе смысл строки теряется.
        var dup = Lines
            .Where(l => l.ProductId != Guid.Empty && l.ShelfId != Guid.Empty)
            .GroupBy(l => (l.ProductId, l.ShelfId))
            .FirstOrDefault(g => g.Count() > 1);
        if (dup is not null)
            errors.Add("Один и тот же товар на одну и ту же полку указан в нескольких позициях.");

        return errors;
    }
}