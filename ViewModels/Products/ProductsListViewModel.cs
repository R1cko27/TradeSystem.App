// ViewModels/Products/ProductsListViewModel.cs

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

// Файл содержит класс ProductsListViewModel,
// который представляет ViewModel списка товаров с фильтрацией и операциями добавления, изменения и удаления.

namespace TradeSystem.App.ViewModels.Products;

public partial class ProductsListViewModel : ViewModelBase, IActivatable
{
    private readonly ICatalogService _catalog; // Сервис справочных данных

    // Полный набор активных товаров (источник фильтрации).
    private IReadOnlyList<ProductDto> _all = Array.Empty<ProductDto>();

    public ObservableCollection<ProductDto> Items { get; } = new(); // Отображаемый список товаров

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(EditCommand))]
    [NotifyCanExecuteChangedFor(nameof(DeleteCommand))]
    private ProductDto? _selectedItem;

    [ObservableProperty]
    private string _searchText = string.Empty; // Текст поиска

    [ObservableProperty]
    private bool _onlyLowStock; // Показывать только товары к пополнению

    // События-намерения: хост (MainViewModel) решает, что открывать.
    public event Action<Guid?>? EditRequested; // Запрос на редактирование товара
    public event Action? AddRequested; // Запрос на добавление товара

    public ProductsListViewModel(ICatalogService catalog, IDialogService dialog)
        : base(dialog)
    {
        _catalog = catalog;
    }

    public Task OnActivatedAsync() => LoadAndFilterAsync(); // Загрузить данные при активации ViewModel

    [RelayCommand]
    private Task RefreshAsync() => RunAsync(LoadAndFilterAsync); // Команда обновления списка

    [RelayCommand]
    private void Add() => AddRequested?.Invoke(); // Команда добавления товара

    [RelayCommand(CanExecute = nameof(HasSelection))]
    private void Edit() // Команда редактирования товара
    {
        if (SelectedItem is not null)
            EditRequested?.Invoke(SelectedItem.Id);
    }

    [RelayCommand(CanExecute = nameof(HasSelection))]
    private Task DeleteAsync() // Команда удаления товара
    {
        var target = SelectedItem;
        if (target is null) return Task.CompletedTask;

        if (!Dialog.Confirm($"Удалить товар «{target.Name}»?", "Удаление товара"))
            return Task.CompletedTask;

        return RunAsync(async () =>
        {
            await _catalog.DeleteProductAsync(target.Id);
            SelectedItem = null;
            await LoadAndFilterAsync();
        });
    }

    private bool HasSelection => SelectedItem is not null; // Признак наличия выбранного товара

    partial void OnSearchTextChanged(string value) => ApplyFilter(); // Реакция на изменение текста поиска
    partial void OnOnlyLowStockChanged(bool value) => ApplyFilter(); // Реакция на изменение фильтра по остаткам

    private async Task LoadAndFilterAsync() // Загрузить товары и применить фильтр
    {
        await RunAsync(async () =>
        {
            _all = await _catalog.GetProductsAsync(includeInactive: false);
            ApplyFilter();
        });
    }

    private void ApplyFilter() // Применить фильтр поиска и остатков
    {
        IEnumerable<ProductDto> query = _all;

        if (OnlyLowStock)
            query = query.Where(p => p.NeedsReplenish);

        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            var s = SearchText.Trim();
            query = query.Where(p =>
                ContainsIgnoreCase(p.Name, s) || ContainsIgnoreCase(p.Article, s));
        }

        Items.Clear();
        foreach (var item in query.OrderBy(p => p.Name, StringComparer.CurrentCultureIgnoreCase))
            Items.Add(item);
    }

    private static bool ContainsIgnoreCase(string source, string value) => // Проверить вхождение подстроки без учёта регистра
        source.Contains(value, StringComparison.CurrentCultureIgnoreCase);
}