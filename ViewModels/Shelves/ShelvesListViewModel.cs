// ViewModels/Shelves/ShelvesListViewModel.cs

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

// Файл содержит класс ShelvesListViewModel,
// который представляет ViewModel списка полок с фильтрацией и операциями добавления, изменения и удаления.

namespace TradeSystem.App.ViewModels.Shelves;

public partial class ShelvesListViewModel : ViewModelBase, IActivatable
{
    private readonly ICatalogService _catalog; // Сервис справочных данных
    private IReadOnlyList<ShelfDto> _all = Array.Empty<ShelfDto>(); // Полный набор полок (источник фильтрации)

    public ObservableCollection<ShelfDto> Items { get; } = new(); // Отображаемый список полок

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(EditCommand))]
    [NotifyCanExecuteChangedFor(nameof(DeleteCommand))]
    private ShelfDto? _selectedItem;
    [ObservableProperty] private string _searchText = string.Empty; // Текст поиска
    [ObservableProperty] private bool _showInactive; // Показывать неактивные полки

    public event Action? AddRequested; // Запрос на добавление полки
    public event Action<Guid?>? EditRequested; // Запрос на редактирование полки

    public ShelvesListViewModel(ICatalogService catalog, IDialogService dialog)
        : base(dialog)
    {
        _catalog = catalog;
    }

    public Task OnActivatedAsync() => LoadAndFilterAsync(); // Загрузить данные при активации ViewModel

    [RelayCommand]
    private Task RefreshAsync() => RunAsync(LoadAndFilterAsync); // Команда обновления списка

    [RelayCommand]
    private void Add() => AddRequested?.Invoke(); // Команда добавления полки

    [RelayCommand(CanExecute = nameof(HasSelection))]
    private void Edit() // Команда редактирования полки
    {
        if (SelectedItem is not null)
            EditRequested?.Invoke(SelectedItem.Id);
    }

    [RelayCommand(CanExecute = nameof(HasSelection))]
    private Task DeleteAsync() // Команда удаления полки
    {
        var target = SelectedItem;
        if (target is null) return Task.CompletedTask;

        if (!Dialog.Confirm($"Удалить полку «{target.Name}» ({target.Code})?", "Удаление полки"))
            return Task.CompletedTask;

        return RunAsync(async () =>
        {
            await _catalog.DeleteShelfAsync(target.Id);
            SelectedItem = null;
            await LoadAndFilterAsync();
        });
    }

    private bool HasSelection => SelectedItem is not null; // Признак наличия выбранной полки

    partial void OnSearchTextChanged(string value) => ApplyFilter(); // Реакция на изменение текста поиска
    partial void OnShowInactiveChanged(bool value) => _ = LoadAndFilterAsync(); // Реакция на изменение фильтра активности

    private async Task LoadAndFilterAsync() // Загрузить полки и применить фильтр
    {
        await RunAsync(async () =>
        {
            _all = await _catalog.GetShelvesAsync(ShowInactive);
            ApplyFilter();
        });
    }

    private void ApplyFilter() // Применить фильтр поиска
    {
        IEnumerable<ShelfDto> query = _all;

        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            var s = SearchText.Trim();
            query = query.Where(x =>
                Contains(x.Code, s) || Contains(x.Name, s) || Contains(x.Location, s));
        }

        Items.Clear();
        foreach (var item in query.OrderBy(x => x.Code, StringComparer.CurrentCultureIgnoreCase))
            Items.Add(item);
    }

    private static bool Contains(string? source, string value) => // Проверить вхождение подстроки без учёта регистра
        !string.IsNullOrEmpty(source) && source.Contains(value, StringComparison.CurrentCultureIgnoreCase);
}