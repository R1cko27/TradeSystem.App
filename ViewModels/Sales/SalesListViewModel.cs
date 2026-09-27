// ViewModels/Sales/SalesListViewModel.cs

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

// Файл содержит класс SalesListViewModel,
// который представляет ViewModel списка продаж с фильтрацией и операциями проведения и удаления.

namespace TradeSystem.App.ViewModels.Sales;

public partial class SalesListViewModel : ViewModelBase, IActivatable
{
    private readonly ISaleService _sales; // Сервис продаж
    private IReadOnlyList<SaleDto> _all = Array.Empty<SaleDto>(); // Полный набор продаж (источник фильтрации)

    public ObservableCollection<SaleDto> Items { get; } = new(); // Отображаемый список продаж

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(PostCommand))]
    [NotifyCanExecuteChangedFor(nameof(DeleteCommand))]
    private SaleDto? _selectedSale; // Выбранная продажа

    [ObservableProperty] private string _searchText = string.Empty; // Текст поиска

    public event Action? NewRequested; // Запрос на создание новой продажи

    public SalesListViewModel(ISaleService sales, IDialogService dialog)
        : base(dialog)
    {
        _sales = sales;
    }

    public Task OnActivatedAsync() => ReloadAsync(SelectedSale?.Id); // Загрузить данные при активации ViewModel

    [RelayCommand]
    private Task RefreshAsync() => RunAsync(() => ReloadAsync(SelectedSale?.Id)); // Команда обновления списка

    [RelayCommand]
    private void New() => NewRequested?.Invoke(); // Команда создания новой продажи

    [RelayCommand(CanExecute = nameof(CanPost))]
    private Task PostAsync() // Команда проведения продажи
    {
        var target = SelectedSale;
        if (target is null) return Task.CompletedTask;

        if (!Dialog.Confirm(
                $"Провести продажу {target.Number}? Товар будет списан с указанных полок.",
                "Проведение продажи"))
            return Task.CompletedTask;

        return RunAsync(async () =>
        {
            await _sales.PostAsync(target.Id);
            await ReloadAsync(target.Id);
        });
    }

    [RelayCommand(CanExecute = nameof(CanDelete))]
    private Task DeleteAsync() // Команда удаления черновика продажи
    {
        var target = SelectedSale;
        if (target is null) return Task.CompletedTask;

        if (!Dialog.Confirm($"Удалить черновик продажи {target.Number}?", "Удаление"))
            return Task.CompletedTask;

        return RunAsync(async () =>
        {
            await _sales.DeleteDraftAsync(target.Id);
            await ReloadAsync(null);
        });
    }

    private bool CanPost => SelectedSale?.Status == SaleStatus.Draft; // Провести можно только черновик
    private bool CanDelete => SelectedSale?.Status == SaleStatus.Draft; // Удалить можно только черновик

    partial void OnSearchTextChanged(string value) => ApplyFilter(); // Реакция на изменение текста поиска

    private async Task ReloadAsync(Guid? keepSelectedId) // Загрузить продажи и применить фильтр
    {
        await RunAsync(async () =>
        {
            _all = await _sales.GetSalesAsync();
            ApplyFilter(keepSelectedId);
        });
    }

    private void ApplyFilter(Guid? keepSelectedId = null) // Применить фильтр поиска
    {
        IEnumerable<SaleDto> query = _all;

        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            var s = SearchText.Trim();
            query = query.Where(x => Contains(x.Number, s));
        }

        var previousId = keepSelectedId ?? SelectedSale?.Id;

        Items.Clear();
        foreach (var item in query)
            Items.Add(item);

        if (previousId is { } pid)
            SelectedSale = Items.FirstOrDefault(x => x.Id == pid);
    }

    private static bool Contains(string? source, string value) => // Проверить вхождение подстроки без учёта регистра
        !string.IsNullOrEmpty(source) && source.Contains(value, StringComparison.CurrentCultureIgnoreCase);
}