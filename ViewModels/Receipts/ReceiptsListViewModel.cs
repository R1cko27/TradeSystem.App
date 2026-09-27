// ViewModels/Receipts/ReceiptsListViewModel.cs

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

namespace TradeSystem.App.ViewModels.Receipts;

public partial class ReceiptsListViewModel : ViewModelBase, IActivatable
{
    private readonly IGoodsReceiptService _receipts;
    private IReadOnlyList<GoodsReceiptDto> _all = Array.Empty<GoodsReceiptDto>();

    public ObservableCollection<GoodsReceiptDto> Items { get; } = new();

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(PostCommand))]
    [NotifyCanExecuteChangedFor(nameof(DeleteCommand))]
    private GoodsReceiptDto? _selectedReceipt;

    [ObservableProperty] private string _searchText = string.Empty;

    public event Action? NewRequested;

    public ReceiptsListViewModel(IGoodsReceiptService receipts, IDialogService dialog)
        : base(dialog)
    {
        _receipts = receipts;
    }

    public Task OnActivatedAsync() => ReloadAsync(SelectedReceipt?.Id);

    [RelayCommand]
    private Task RefreshAsync() => RunAsync(() => ReloadAsync(SelectedReceipt?.Id));

    [RelayCommand]
    private void New() => NewRequested?.Invoke();

    [RelayCommand(CanExecute = nameof(CanPost))]
    private Task PostAsync()
    {
        var target = SelectedReceipt;
        if (target is null) return Task.CompletedTask;

        if (!Dialog.Confirm(
                $"Провести поступление {target.Number}? Товар будет оприходован на указанные полки.",
                "Проведение поступления"))
            return Task.CompletedTask;

        return RunAsync(async () =>
        {
            await _receipts.PostAsync(target.Id);
            await ReloadAsync(target.Id); // восстановление выделения -> пересчёт CanExecute
        });
    }

    [RelayCommand(CanExecute = nameof(CanDelete))]
    private Task DeleteAsync()
    {
        var target = SelectedReceipt;
        if (target is null) return Task.CompletedTask;

        if (!Dialog.Confirm($"Удалить черновик поступления {target.Number}?", "Удаление"))
            return Task.CompletedTask;

        return RunAsync(async () =>
        {
            await _receipts.DeleteDraftAsync(target.Id);
            await ReloadAsync(null);
        });
    }

    // Проводить и удалять можно только черновик.
    private bool CanPost => SelectedReceipt?.Status == GoodsReceiptStatus.Draft;
    private bool CanDelete => SelectedReceipt?.Status == GoodsReceiptStatus.Draft;

    partial void OnSearchTextChanged(string value) => ApplyFilter();

    private async Task ReloadAsync(Guid? keepSelectedId)
    {
        await RunAsync(async () =>
        {
            _all = await _receipts.GetReceiptsAsync();
            ApplyFilter(keepSelectedId);
        });
    }

    private void ApplyFilter(Guid? keepSelectedId = null)
    {
        IEnumerable<GoodsReceiptDto> query = _all;

        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            var s = SearchText.Trim();
            query = query.Where(r => Contains(r.Number, s) || Contains(r.SupplierName, s));
        }

        var previousId = keepSelectedId ?? SelectedReceipt?.Id;

        Items.Clear();
        foreach (var item in query)
            Items.Add(item);

        if (previousId is { } pid)
            SelectedReceipt = Items.FirstOrDefault(r => r.Id == pid);
    }

    private static bool Contains(string? source, string value) =>
        !string.IsNullOrEmpty(source) && source.Contains(value, StringComparison.CurrentCultureIgnoreCase);
}