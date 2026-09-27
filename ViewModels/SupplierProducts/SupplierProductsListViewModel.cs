// ViewModels/SupplierProducts/SupplierProductsListViewModel.cs

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

// Файл содержит класс SupplierProductsListViewModel,
// который представляет ViewModel плоского списка связей «товар-поставщик» с фильтрацией и операциями добавления, изменения и удаления.

namespace TradeSystem.App.ViewModels.SupplierProducts;

/// <summary>
/// Плоский список связей «товар-поставщик» с фильтрами по товару, поставщику,
/// поиском и переключателем показа неактивных связей.
/// Наглядно демонстрирует случай «один товар — несколько поставщиков».
/// </summary>
public partial class SupplierProductsListViewModel : ViewModelBase, IActivatable
{
    private readonly ICatalogService _catalog; // Сервис справочных данных
    private IReadOnlyList<SupplierProductDto> _all = Array.Empty<SupplierProductDto>(); // Полный набор связей (источник фильтрации)

    public ObservableCollection<SupplierProductDto> Items { get; } = new(); // Отображаемый список связей

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(EditCommand))]
    [NotifyCanExecuteChangedFor(nameof(DeleteCommand))]
    private SupplierProductDto? _selectedItem; // Выбранная связь

    [ObservableProperty] private string _searchText = string.Empty; // Текст поиска
    [ObservableProperty] private Guid _filterProductId;     // Guid.Empty = все товары
    [ObservableProperty] private Guid _filterSupplierId;    // Guid.Empty = все поставщики
    [ObservableProperty] private bool _showInactive; // Показывать неактивные связи

    // Источники для ComboBox-фильтров (первый элемент — «все»).
    [ObservableProperty] private IReadOnlyList<ProductDto> _productFilters = Array.Empty<ProductDto>(); // Список товаров для фильтра
    [ObservableProperty] private IReadOnlyList<SupplierDto> _supplierFilters = Array.Empty<SupplierDto>(); // Список поставщиков для фильтра

    public event Action? AddRequested; // Запрос на добавление связи
    public event Action<Guid?>? EditRequested; // Запрос на редактирование связи

    public SupplierProductsListViewModel(ICatalogService catalog, IDialogService dialog)
        : base(dialog)
    {
        _catalog = catalog;
    }

    public Task OnActivatedAsync() => LoadAsync(); // Загрузить данные при активации ViewModel

    [RelayCommand]
    private Task RefreshAsync() => RunAsync(LoadAsync); // Команда обновления списка

    [RelayCommand]
    private void Add() => AddRequested?.Invoke(); // Команда добавления связи

    [RelayCommand(CanExecute = nameof(HasSelection))]
    private void Edit() // Команда редактирования связи
    {
        if (SelectedItem is not null)
            EditRequested?.Invoke(SelectedItem.Id);
    }

    [RelayCommand(CanExecute = nameof(HasSelection))]
    private Task DeleteAsync() // Команда удаления связи
    {
        var target = SelectedItem;
        if (target is null) return Task.CompletedTask;

        if (!Dialog.Confirm(
                $"Удалить связь «{target.ProductName} ↔ {target.SupplierName}»?",
                "Удаление поставки"))
            return Task.CompletedTask;

        return RunAsync(async () =>
        {
            await _catalog.UnlinkAsync(target.Id);
            SelectedItem = null;
            await LoadAsync();
        });
    }

    private bool HasSelection => SelectedItem is not null; // Признак наличия выбранной связи

    // Все фильтры локальные — не дёргаем хранилище повторно.
    partial void OnSearchTextChanged(string value) => ApplyFilter(); // Реакция на изменение текста поиска
    partial void OnFilterProductIdChanged(Guid value) => ApplyFilter(); // Реакция на изменение фильтра по товару
    partial void OnFilterSupplierIdChanged(Guid value) => ApplyFilter(); // Реакция на изменение фильтра по поставщику
    partial void OnShowInactiveChanged(bool value) => ApplyFilter(); // Реакция на изменение фильтра активности

    private async Task LoadAsync() // Загрузить связи и справочники для фильтров
    {
        await RunAsync(async () =>
        {
            _all = await _catalog.GetSupplierProductsAsync();

            var productsTask = _catalog.GetProductsAsync(includeInactive: false);
            var suppliersTask = _catalog.GetSuppliersAsync(includeInactive: false);
            await Task.WhenAll(productsTask, suppliersTask);

            ProductFilters = BuildProductFilters(await productsTask);
            SupplierFilters = BuildSupplierFilters(await suppliersTask);

            ApplyFilter();
        });
    }

    private static IReadOnlyList<ProductDto> BuildProductFilters(IReadOnlyList<ProductDto> real) // Построить список товаров для фильтра
    {
        var list = new List<ProductDto>(real.Count + 1)
        {
            new() { Id = Guid.Empty, Name = "— Все товары —" }
        };
        list.AddRange(real);
        return list;
    }

    private static IReadOnlyList<SupplierDto> BuildSupplierFilters(IReadOnlyList<SupplierDto> real) // Построить список поставщиков для фильтра
    {
        var list = new List<SupplierDto>(real.Count + 1)
        {
            new() { Id = Guid.Empty, Name = "— Все поставщики —" }
        };
        list.AddRange(real);
        return list;
    }

    private void ApplyFilter() // Применить фильтры
    {
        IEnumerable<SupplierProductDto> query = _all;

        if (!ShowInactive)
            query = query.Where(x => x.IsActive);

        if (FilterProductId != Guid.Empty)
            query = query.Where(x => x.ProductId == FilterProductId);

        if (FilterSupplierId != Guid.Empty)
            query = query.Where(x => x.SupplierId == FilterSupplierId);

        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            var s = SearchText.Trim();
            query = query.Where(x =>
                Contains(x.ProductName, s) ||
                Contains(x.SupplierName, s) ||
                Contains(x.SupplierArticle, s));
        }

        Items.Clear();
        foreach (var item in query
                     .OrderBy(x => x.ProductName, StringComparer.CurrentCultureIgnoreCase)
                     .ThenBy(x => x.SupplierName, StringComparer.CurrentCultureIgnoreCase))
        {
            Items.Add(item);
        }
    }

    private static bool Contains(string? source, string value) => // Проверить вхождение подстроки без учёта регистра
        !string.IsNullOrEmpty(source) && source.Contains(value, StringComparison.CurrentCultureIgnoreCase);
}