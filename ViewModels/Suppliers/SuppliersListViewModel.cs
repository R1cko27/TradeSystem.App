// ViewModels/Suppliers/SuppliersListViewModel.cs

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

// Файл содержит класс SuppliersListViewModel,
// который представляет ViewModel списка поставщиков с фильтрацией и операциями добавления, изменения и удаления.

namespace TradeSystem.App.ViewModels.Suppliers;

public partial class SuppliersListViewModel : ViewModelBase, IActivatable
{
    private readonly ICatalogService _catalog; // Сервис справочных данных
    private IReadOnlyList<SupplierDto> _all = Array.Empty<SupplierDto>(); // Полный набор поставщиков (источник фильтрации)

    public ObservableCollection<SupplierDto> Items { get; } = new(); // Отображаемый список поставщиков

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(EditCommand))]
    [NotifyCanExecuteChangedFor(nameof(DeleteCommand))]
    private SupplierDto? _selectedItem;
    
    [ObservableProperty] private string _searchText = string.Empty; // Текст поиска
    [ObservableProperty] private bool _showInactive; // Показывать неактивных поставщиков

    public event Action? AddRequested; // Запрос на добавление поставщика
    public event Action<Guid?>? EditRequested; // Запрос на редактирование поставщика

    public SuppliersListViewModel(ICatalogService catalog, IDialogService dialog)
        : base(dialog)
    {
        _catalog = catalog;
    }

    public Task OnActivatedAsync() => LoadAndFilterAsync(); // Загрузить данные при активации ViewModel

    [RelayCommand]
    private Task RefreshAsync() => RunAsync(LoadAndFilterAsync); // Команда обновления списка

    [RelayCommand]
    private void Add() => AddRequested?.Invoke(); // Команда добавления поставщика

    [RelayCommand(CanExecute = nameof(HasSelection))]
    private void Edit() // Команда редактирования поставщика
    {
        if (SelectedItem is not null)
            EditRequested?.Invoke(SelectedItem.Id);
    }

    [RelayCommand(CanExecute = nameof(HasSelection))]
    private Task DeleteAsync() // Команда удаления поставщика
    {
        var target = SelectedItem;
        if (target is null) return Task.CompletedTask;

        if (!Dialog.Confirm($"Удалить поставщика «{target.Name}»?", "Удаление поставщика"))
            return Task.CompletedTask;

        return RunAsync(async () =>
        {
            await _catalog.DeleteSupplierAsync(target.Id);
            SelectedItem = null;
            await LoadAndFilterAsync();
        });
    }

    private bool HasSelection => SelectedItem is not null; // Признак наличия выбранного поставщика

    // Поиск фильтрует локально; переключатель активности перечитывает из сервиса.
    partial void OnSearchTextChanged(string value) => ApplyFilter(); // Реакция на изменение текста поиска
    partial void OnShowInactiveChanged(bool value) => _ = LoadAndFilterAsync(); // Реакция на изменение фильтра активности

    private async Task LoadAndFilterAsync() // Загрузить поставщиков и применить фильтр
    {
        await RunAsync(async () =>
        {
            _all = await _catalog.GetSuppliersAsync(ShowInactive);
            ApplyFilter();
        });
    }

    private void ApplyFilter() // Применить фильтр поиска
    {
        IEnumerable<SupplierDto> query = _all;

        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            var s = SearchText.Trim();
            query = query.Where(x =>
                Contains(x.Name, s) || Contains(x.Phone, s) || Contains(x.TaxId, s));
        }

        Items.Clear();
        foreach (var item in query.OrderBy(x => x.Name, StringComparer.CurrentCultureIgnoreCase))
            Items.Add(item);
    }

    private static bool Contains(string? source, string value) => // Проверить вхождение подстроки без учёта регистра
        !string.IsNullOrEmpty(source) && source.Contains(value, StringComparison.CurrentCultureIgnoreCase);
}