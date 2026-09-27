// ViewModels/Orders/OrderManualEditViewModel.cs

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

// Файл содержит класс OrderManualEditViewModel,
// который представляет ViewModel формы ручного создания заказа на покупку в торговой системе.

namespace TradeSystem.App.ViewModels.Orders;

public partial class OrderManualEditViewModel : ViewModelBase
{
    private readonly ICatalogService _catalog; // Сервис справочных данных
    private readonly IPurchaseOrderService _orders; // Сервис заказов на покупку

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(AddLineCommand))]
    private Guid _supplierId; // Идентификатор выбранного поставщика

    [ObservableProperty] private string _comment = string.Empty; // Комментарий к заказу
    [ObservableProperty] private IReadOnlyList<SupplierDto> _suppliers = Array.Empty<SupplierDto>(); // Список поставщиков для выбора
    [ObservableProperty] private IReadOnlyList<SupplierProductDto> _availableProducts = Array.Empty<SupplierProductDto>(); // Товары выбранного поставщика
    [ObservableProperty] private ManualOrderLineViewModel? _selectedLine; // Выбранная позиция заказа

    public ObservableCollection<ManualOrderLineViewModel> Lines { get; } = new(); // Позиции заказа

    public decimal TotalAmount => Lines.Sum(l => l.Total); // Общая сумма заказа

    public event Action? CloseRequested; // Запрос на закрытие формы

    public OrderManualEditViewModel(ICatalogService catalog, IPurchaseOrderService orders, IDialogService dialog)
        : base(dialog)
    {
        _catalog = catalog;
        _orders = orders;
    }

    public async Task LoadAsync() // Загрузить данные формы
    {
        await RunAsync(async () =>
        {
            SupplierId = Guid.Empty;
            Comment = string.Empty;
            Lines.Clear();
            SelectedLine = null;
            Suppliers = await _catalog.GetSuppliersAsync(includeInactive: false);
            AvailableProducts = Array.Empty<SupplierProductDto>();
            OnPropertyChanged(nameof(TotalAmount));
        });
    }

    partial void OnSupplierIdChanged(Guid value) => _ = HandleSupplierChangedAsync(value); // Реакция на смену поставщика

    private async Task HandleSupplierChangedAsync(Guid supplierId) // Обработать смену поставщика
    {
        if (Lines.Count > 0 &&
            !Dialog.Confirm("Смена поставщика очистит добавленные позиции. Продолжить?", "Смена поставщика"))
        {
            return; // откат не делаем осознанно: пользователь сам разберётся, позиция останется битой до сохранения
        }

        await RunAsync(async () =>
        {
            Lines.Clear();
            SelectedLine = null;
            AvailableProducts = supplierId == Guid.Empty
                ? Array.Empty<SupplierProductDto>()
                : await _catalog.GetSupplierProductsBySupplierAsync(supplierId);
            OnPropertyChanged(nameof(TotalAmount));
        });
    }

    [RelayCommand(CanExecute = nameof(CanAddLine))]
    private void AddLine() // Добавить позицию в заказ
    {
        Lines.Add(new ManualOrderLineViewModel());
        OnPropertyChanged(nameof(TotalAmount));
    }

    private bool CanAddLine => SupplierId != Guid.Empty && AvailableProducts.Count > 0; // Условие доступности добавления позиции

    [RelayCommand(CanExecute = nameof(HasLineSelection))]
    private void RemoveLine() // Удалить выбранную позицию
    {
        if (SelectedLine is null) return;
        Lines.Remove(SelectedLine);
        SelectedLine = Lines.LastOrDefault();
        OnPropertyChanged(nameof(TotalAmount));
    }

    private bool HasLineSelection => SelectedLine is not null; // Признак наличия выбранной позиции

    [RelayCommand]
    private Task SaveAsync() => RunAsync(SaveImplementationAsync); // Команда сохранения заказа

    [RelayCommand]
    private void Cancel() => CloseRequested?.Invoke(); // Команда отмены создания заказа

    private async Task SaveImplementationAsync() // Сохранить заказ
    {
        var errors = Validate();
        if (errors.Count > 0)
        {
            Dialog.ShowError(string.Join(Environment.NewLine, errors));
            return;
        }

        var input = new CreatePurchaseOrderInput
        {
            SupplierId = SupplierId,
            Comment = string.IsNullOrWhiteSpace(Comment) ? null : Comment.Trim(),
            Lines = Lines.Select(l => new CreatePurchaseOrderLineInput
            {
                ProductId = l.ProductId,
                Quantity = l.Quantity,
                UnitPrice = l.UnitPrice,
                SupplierProductId = l.SupplierProductId
            }).ToList()
        };

        await _orders.CreateOrderAsync(input);
        CloseRequested?.Invoke();
    }

    private List<string> Validate() // Проверить корректность введённых данных
    {
        var errors = new List<string>();
        if (SupplierId == Guid.Empty)
            errors.Add("Выберите поставщика.");
        if (Lines.Count == 0)
            errors.Add("Добавьте хотя бы одну позицию.");
        for (int i = 0; i < Lines.Count; i++)
        {
            var l = Lines[i];
            if (l.SelectedSupplierProduct is null)
                errors.Add($"Позиция {i + 1}: не выбран товар.");
            else if (l.Quantity <= 0)
                errors.Add($"Позиция {i + 1}: количество должно быть больше нуля.");
        }
        if (Lines.GroupBy(l => l.ProductId).Any(g => g.Key != Guid.Empty && g.Count() > 1))
            errors.Add("Один и тот же товар не должен повторяться в позициях.");
        return errors;
    }
}