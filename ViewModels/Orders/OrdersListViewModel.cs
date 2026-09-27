// ViewModels/Orders/OrdersListViewModel.cs

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

// Файл содержит класс OrdersListViewModel,
// который представляет ViewModel списка заказов на покупку с фильтрацией, сменой статусов и переходом к счетам.

namespace TradeSystem.App.ViewModels.Orders;

public partial class OrdersListViewModel : ViewModelBase, IActivatable
{
    private readonly IPurchaseOrderService _orders; // Сервис заказов на покупку
    private IReadOnlyList<PurchaseOrderDto> _all = Array.Empty<PurchaseOrderDto>(); // Полный набор заказов (источник фильтрации)

    public ObservableCollection<PurchaseOrderDto> Items { get; } = new(); // Отображаемый список заказов

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ConfirmCommand))]
    [NotifyCanExecuteChangedFor(nameof(CancelCommand))]
    [NotifyCanExecuteChangedFor(nameof(DeleteCommand))]
    [NotifyCanExecuteChangedFor(nameof(InvoicesCommand))]
    private PurchaseOrderDto? _selectedOrder; // Выбранный заказ

    [ObservableProperty] private string _searchText = string.Empty; // Текст поиска
    [ObservableProperty] private bool _showCancelled; // Показывать отменённые заказы

    public event Action? AutoPreviewRequested; // Запрос на предпросмотр автоформирования
    public event Action? ManualCreateRequested; // Запрос на ручное создание заказа
    public event Action<Guid>? InvoicesRequested; // Запрос на просмотр счетов заказа

    public OrdersListViewModel(IPurchaseOrderService orders, IDialogService dialog)
        : base(dialog)
    {
        _orders = orders;
    }

    public Task OnActivatedAsync() => ReloadAsync(keepSelectedId: SelectedOrder?.Id); // Загрузить данные при активации ViewModel

    [RelayCommand]
    private Task RefreshAsync() => RunAsync(() => ReloadAsync(SelectedOrder?.Id)); // Команда обновления списка

    [RelayCommand]
    private void AutoGenerate() => AutoPreviewRequested?.Invoke(); // Команда перехода к предпросмотру автоформирования

    [RelayCommand]
    private void NewManual() => ManualCreateRequested?.Invoke(); // Команда ручного создания заказа

    [RelayCommand(CanExecute = nameof(CanConfirm))]
    private Task ConfirmAsync() => GuardedStatusChangeAsync( // Команда подтверждения заказа
        o => _orders.ConfirmOrderAsync(o.Id), "Подтверждение заказа");

    [RelayCommand(CanExecute = nameof(CanCancel))]
    private Task CancelAsync() => GuardedStatusChangeAsync( // Команда отмены заказа
        o => _orders.CancelOrderAsync(o.Id), "Отмена заказа");

    [RelayCommand(CanExecute = nameof(CanDelete))]
    private Task DeleteAsync() // Команда удаления черновика заказа
    {
        var target = SelectedOrder;
        if (target is null) return Task.CompletedTask;
        if (!Dialog.Confirm($"Удалить черновик заказа {target.Number}?", "Удаление заказа"))
            return Task.CompletedTask;

        return RunAsync(async () =>
        {
            await _orders.DeleteOrderAsync(target.Id);
            await ReloadAsync(null);
        });
    }

    [RelayCommand(CanExecute = nameof(HasSelection))]
    private void Invoices() // Команда перехода к счетам заказа
    {
        if (SelectedOrder is not null)
            InvoicesRequested?.Invoke(SelectedOrder.Id);
    }

    // ---- Условия доступности (зависят от статуса выделенного заказа) ----
    private bool HasSelection => SelectedOrder is not null; // Признак наличия выбранного заказа
    private bool CanConfirm => SelectedOrder?.Status == PurchaseOrderStatus.Draft; // Подтвердить можно только черновик
    private bool CanCancel => SelectedOrder is { } o && // Отменить можно любой, кроме полученного/закрытого/отменённого
        o.Status is not (PurchaseOrderStatus.Received or PurchaseOrderStatus.Closed or PurchaseOrderStatus.Cancelled);
    private bool CanDelete => SelectedOrder?.Status == PurchaseOrderStatus.Draft; // Удалить можно только черновик

    partial void OnSearchTextChanged(string value) => ApplyFilter(); // Реакция на изменение текста поиска
    partial void OnShowCancelledChanged(bool value) => ApplyFilter(); // Реакция на изменение фильтра отменённых

    private async Task GuardedStatusChangeAsync(Func<PurchaseOrderDto, Task> action, string title) // Выполнить смену статуса с подтверждением
    {
        var target = SelectedOrder;
        if (target is null) return;
        if (!Dialog.Confirm($"{title}: {target.Number}?", title))
            return;

        await RunAsync(async () =>
        {
            await action(target);
            await ReloadAsync(target.Id); // восстановление выделения триггерит пересчёт CanExecute
        });
    }

    private async Task ReloadAsync(Guid? keepSelectedId) // Загрузить заказы и применить фильтр
    {
        await RunAsync(async () =>
        {
            _all = await _orders.GetOrdersAsync();
            ApplyFilter(keepSelectedId);
        });
    }

    private void ApplyFilter(Guid? keepSelectedId = null) // Применить фильтр поиска и отменённых
    {
        IEnumerable<PurchaseOrderDto> query = _all;

        if (!ShowCancelled)
            query = query.Where(o => o.Status != PurchaseOrderStatus.Cancelled);

        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            var s = SearchText.Trim();
            query = query.Where(o =>
                Contains(o.Number, s) || Contains(o.SupplierName, s));
        }

        var previousId = keepSelectedId ?? SelectedOrder?.Id;

        Items.Clear();
        foreach (var item in query)
            Items.Add(item);

        if (previousId is { } pid)
            SelectedOrder = Items.FirstOrDefault(o => o.Id == pid);
    }

    private static bool Contains(string? source, string value) => // Проверить вхождение подстроки без учёта регистра
        !string.IsNullOrEmpty(source) && source.Contains(value, StringComparison.CurrentCultureIgnoreCase);
}