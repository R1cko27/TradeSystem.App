// ViewModels/Orders/OrderAutoPreviewViewModel.cs

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

// Файл содержит класс OrderAutoPreviewViewModel,
// который представляет ViewModel предпросмотра автоформирования заказов с возможностью выбора поставщиков и добавления товаров.

namespace TradeSystem.App.ViewModels.Orders;

public partial class OrderAutoPreviewViewModel : ViewModelBase
{
    private readonly IPurchaseOrderService _orders; // Сервис заказов на покупку
    private readonly ICatalogService _catalog; // Сервис справочных данных

    // Дополнительные товары, добавленные пользователем сверх дефицита.
    private readonly HashSet<Guid> _additionalIds = new();

    [ObservableProperty] private IReadOnlyList<ProductDto> _allActiveProducts = Array.Empty<ProductDto>(); // Список активных товаров для добавления
    [ObservableProperty] private ProductDto? _productToAdd; // Товар, выбранный для добавления

    public ObservableCollection<PreviewRowViewModel> Rows { get; } = new(); // Строки предпросмотра
    public ObservableCollection<string> Warnings { get; } = new(); // Предупреждения предпросмотра

    public event Action? CloseRequested; // Запрос на закрытие формы

    public OrderAutoPreviewViewModel(IPurchaseOrderService orders, ICatalogService catalog, IDialogService dialog)
        : base(dialog)
    {
        _orders = orders;
        _catalog = catalog;
    }

    public Task LoadAsync() => ReloadAsync(); // Загрузить данные формы

    private async Task ReloadAsync() // Перестроить предпросмотр
    {
        await RunAsync(async () =>
        {
            AllActiveProducts = await _catalog.GetProductsAsync(includeInactive: false);

            var request = BuildRequest();
            var preview = await _orders.PreviewAutoOrdersAsync(request);

            // Определяем, какие строки пришли из дефицита, а какие — дополнительные.
            Rows.Clear();
            foreach (var p in preview.Products)
                Rows.Add(new PreviewRowViewModel(p, isAdditional: _additionalIds.Contains(p.ProductId)));

            Warnings.Clear();
            foreach (var w in preview.Warnings)
                Warnings.Add(w);
        });
    }

    [RelayCommand]
    private Task AddAdditionalAsync() => RunAsync(async () => // Добавить дополнительный товар в план
    {
        if (ProductToAdd is null)
        {
            Dialog.ShowWarning("Выберите товар для добавления.");
            return;
        }
        if (_additionalIds.Contains(ProductToAdd.Id) || Rows.Any(r => r.ProductId == ProductToAdd.Id))
        {
            Dialog.ShowWarning("Этот товар уже в плане.");
            return;
        }
        _additionalIds.Add(ProductToAdd.Id);
        ProductToAdd = null;
        await ReloadAsync();
    });

    [RelayCommand(CanExecute = nameof(CanRemoveAdditional))]
    private Task RemoveAdditionalAsync() => RunAsync(async () => // Удалить последний добавленный дополнительный товар
    {
        // Удаляем последний добавленный доп. товар (простое и предсказуемое поведение).
        var target = Rows.LastOrDefault(r => r.IsAdditional);
        if (target is null) return;
        _additionalIds.Remove(target.ProductId);
        await ReloadAsync();
    });

    private bool CanRemoveAdditional => Rows.Any(r => r.IsAdditional); // Условие доступности удаления доп. товара

    [RelayCommand]
    private Task ConfirmAsync() => RunAsync(async () => // Подтвердить и создать заказы
    {
        if (Rows.Count == 0)
        {
            Dialog.ShowInfo("Нечего создавать: нет товаров к пополнению.");
            return;
        }

        var request = BuildRequest();
        var result = await _orders.AutoGenerateOrdersAsync(request);

        var msg = result.Orders.Count == 0
            ? "Заказы не созданы."
            : $"Создано заказов: {result.Orders.Count}.";
        if (result.Warnings.Count > 0)
            msg += Environment.NewLine + "Предупреждения:" + Environment.NewLine + string.Join(Environment.NewLine, result.Warnings);

        Dialog.ShowInfo(msg, "Итог автоформирования");
        CloseRequested?.Invoke();
    });

    [RelayCommand]
    private void Cancel() => CloseRequested?.Invoke(); // Команда отмены предпросмотра

    private AutoOrderRequest BuildRequest() // Построить запрос автоформирования
    {
        var explicitChoice = new Dictionary<Guid, Guid>();
        foreach (var row in Rows)
        {
            if (row.SelectedOption is { } opt && opt.SupplierId != Guid.Empty)
                explicitChoice[row.ProductId] = opt.SupplierId;
        }

        return new AutoOrderRequest
        {
            ExplicitSupplierChoice = explicitChoice.Count > 0 ? explicitChoice : null,
            AdditionalProductIds = _additionalIds.Count > 0 ? new HashSet<Guid>(_additionalIds) : null
        };
    }
}