// ViewModels/Invoices/InvoiceListViewModel.cs

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TradeSystem.App.ViewModels.Abstractions;
using TradeSystem.Application.Contracts.Services;
using TradeSystem.Application.Dto;
using TradeSystem.Domain.Enums;

// Файл содержит класс InvoiceListViewModel,
// который представляет ViewModel списка счетов на оплату по заказу с операциями создания, оплаты и удаления.

namespace TradeSystem.App.ViewModels.Invoices;

public partial class InvoiceListViewModel : ViewModelBase
{
    private readonly IInvoiceService _invoices; // Сервис счетов на оплату
    private readonly IPurchaseOrderService _orders; // Сервис заказов на покупку
    private Guid _orderId; // Идентификатор заказа

    [ObservableProperty] private string _orderNumber = string.Empty; // Номер заказа
    [ObservableProperty] private string _supplierName = string.Empty; // Название поставщика
    [ObservableProperty] private decimal _orderTotal; // Сумма заказа

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(PayCommand))]
    [NotifyCanExecuteChangedFor(nameof(DeleteCommand))]
    private InvoiceDto? _selectedInvoice; // Выбранный счёт

    // Поля формы нового счёта.
    [ObservableProperty] private string _newNumber = string.Empty; // Номер нового счёта
    [ObservableProperty] private string _newAmountText = string.Empty; // Сумма нового счёта в виде текста
    [ObservableProperty] private string _newDueDateText = string.Empty; // Срок оплаты нового счёта в виде текста

    public ObservableCollection<InvoiceDto> Items { get; } = new(); // Отображаемый список счетов

    public event Action? CloseRequested; // Запрос на закрытие формы

    public InvoiceListViewModel(IInvoiceService invoices, IPurchaseOrderService orders, IDialogService dialog)
        : base(dialog)
    {
        _invoices = invoices;
        _orders = orders;
    }

    public async Task LoadAsync(Guid orderId) // Загрузить данные формы
    {
        await RunAsync(async () =>
        {
            _orderId = orderId;
            var order = await _orders.GetOrderAsync(orderId);
            if (order is null)
            {
                Dialog.ShowError("Заказ не найден.");
                CloseRequested?.Invoke();
                return;
            }

            OrderNumber = order.Number;
            SupplierName = order.SupplierName;
            OrderTotal = order.TotalAmount;

            NewNumber = await GenerateNextNumberAsync(orderId);
            NewAmountText = order.TotalAmount.ToString("0.##", CultureInfo.CurrentCulture);
            NewDueDateText = string.Empty;

            await ReloadInvoicesAsync();
        });
    }

    [RelayCommand]
    private Task CreateAsync() => RunAsync(async () => // Создать новый счёт
    {
        var errors = ValidateNew(out decimal amount, out DateTimeOffset? due);
        if (errors.Count > 0)
        {
            Dialog.ShowError(string.Join(Environment.NewLine, errors));
            return;
        }

        await _invoices.CreateInvoiceAsync(_orderId, NewNumber.Trim(), amount, due, null);
        NewNumber = await GenerateNextNumberAsync(_orderId);
        await ReloadInvoicesAsync();
    });

    [RelayCommand(CanExecute = nameof(CanPay))]
    private Task PayAsync() => RunAsync(async () => // Отметить счёт как оплаченный
    {
        if (SelectedInvoice is null) return;
        if (!Dialog.Confirm($"Отметить оплату счёта {SelectedInvoice.Number}?", "Оплата"))
            return;
        await _invoices.MarkPaidAsync(SelectedInvoice.Id);
        await ReloadInvoicesAsync();
    });

    [RelayCommand(CanExecute = nameof(CanDelete))]
    private Task DeleteAsync() => RunAsync(async () => // Удалить счёт
    {
        if (SelectedInvoice is null) return;
        if (!Dialog.Confirm($"Удалить счёт {SelectedInvoice.Number}?", "Удаление счёта"))
            return;
        await _invoices.DeleteInvoiceAsync(SelectedInvoice.Id);
        await ReloadInvoicesAsync();
    });

    [RelayCommand]
    private void Close() => CloseRequested?.Invoke(); // Команда закрытия формы

    private bool CanPay => SelectedInvoice is { } i && i.PaymentStatus != PaymentStatus.Paid && i.PaymentStatus != PaymentStatus.Cancelled; // Оплатить можно неоплаченный счёт
    private bool CanDelete => SelectedInvoice is { } i && i.PaymentStatus != PaymentStatus.Paid; // Удалить можно неоплаченный счёт

    private async Task ReloadInvoicesAsync() // Перезагрузить список счетов
    {
        var list = await _invoices.GetInvoicesByOrderAsync(_orderId);
        Items.Clear();
        foreach (var i in list) Items.Add(i);
        SelectedInvoice = null;
    }

    private List<string> ValidateNew(out decimal amount, out DateTimeOffset? due) // Проверить корректность данных нового счёта
    {
        amount = 0m; due = null;
        var errors = new List<string>();
        if (string.IsNullOrWhiteSpace(NewNumber)) errors.Add("Укажите номер счёта.");
        if (!decimal.TryParse(NewAmountText?.Trim(), NumberStyles.Number, CultureInfo.CurrentCulture, out amount) || amount <= 0)
            errors.Add("Сумма счёта должна быть положительным числом.");
        if (!string.IsNullOrWhiteSpace(NewDueDateText))
        {
            if (!DateTimeOffset.TryParse(NewDueDateText.Trim(), CultureInfo.CurrentCulture, DateTimeStyles.None, out var d))
                errors.Add("Срок оплаты: введите дату в формате дд.мм.гггг.");
            else due = d;
        }
        return errors;
    }

    private async Task<string> GenerateNextNumberAsync(Guid orderId) // Сгенерировать номер нового счёта
    {
        var existing = await _invoices.GetInvoicesByOrderAsync(orderId);
        var today = DateTime.UtcNow.ToString("yyyyMMdd");
        var n = existing.Count(i => i.Number.StartsWith($"INV-{today}-", StringComparison.Ordinal));
        return $"INV-{today}-{n + 1:D4}";
    }
}