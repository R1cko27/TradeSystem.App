// ViewModels/Shelves/ShelfEditViewModel.cs

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TradeSystem.App.ViewModels.Abstractions;
using TradeSystem.Application.Contracts.Services;
using TradeSystem.Application.Dto;

// Файл содержит класс ShelfEditViewModel,
// который представляет ViewModel формы создания и редактирования полки в торговой системе.

namespace TradeSystem.App.ViewModels.Shelves;

public partial class ShelfEditViewModel : ViewModelBase
{
    private readonly ICatalogService _catalog; // Сервис справочных данных
    private Guid? _shelfId; // Идентификатор редактируемой полки

    [ObservableProperty] private bool _isNew = true; // Признак создания новой полки
    [ObservableProperty] private string _code = string.Empty; // Код полки
    [ObservableProperty] private string _name = string.Empty; // Название полки
    [ObservableProperty] private string? _location; // Расположение
    [ObservableProperty] private bool _isActive = true; // Признак активности
    [ObservableProperty] private string? _note; // Примечание

    public event Action? CloseRequested; // Запрос на закрытие формы

    public ShelfEditViewModel(ICatalogService catalog, IDialogService dialog)
        : base(dialog)
    {
        _catalog = catalog;
    }

    public async Task LoadAsync(Guid? id) // Загрузить данные формы
    {
        await RunAsync(async () =>
        {
            _shelfId = id;
            IsNew = id is null;

            if (id is null)
            {
                ResetForm();
                return;
            }

            var sh = await _catalog.GetShelfAsync(id.Value);
            if (sh is null)
            {
                Dialog.ShowError("Полка не найдена.");
                ResetForm();
                return;
            }

            Code = sh.Code;
            Name = sh.Name;
            Location = sh.Location;
            IsActive = sh.IsActive;
            Note = sh.Note;
        });
    }

    [RelayCommand]
    private Task SaveAsync() => RunAsync(SaveImplementationAsync); // Команда сохранения полки

    [RelayCommand]
    private void Cancel() => CloseRequested?.Invoke(); // Команда отмены редактирования

    private async Task SaveImplementationAsync() // Сохранить полку
    {
        var errors = Validate();
        if (errors.Count > 0)
        {
            Dialog.ShowError(string.Join(Environment.NewLine, errors));
            return;
        }

        var input = new ShelfInput
        {
            Id = _shelfId,
            Code = Code.Trim(),
            Name = Name.Trim(),
            Location = TrimOrNull(Location),
            IsActive = IsActive,
            Note = TrimOrNull(Note)
        };

        if (IsNew)
            await _catalog.CreateShelfAsync(input);
        else
            await _catalog.UpdateShelfAsync(input);

        CloseRequested?.Invoke();
    }

    private List<string> Validate() // Проверить корректность введённых данных
    {
        var errors = new List<string>();
        if (string.IsNullOrWhiteSpace(Code))
            errors.Add("Код полки обязателен.");
        if (string.IsNullOrWhiteSpace(Name))
            errors.Add("Название полки обязательно.");
        return errors;
    }

    private void ResetForm() // Сбросить поля формы
    {
        Code = string.Empty;
        Name = string.Empty;
        Location = null;
        IsActive = true;
        Note = null;
    }

    private static string? TrimOrNull(string? value) // Обрезать строку или вернуть null
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        return value.Trim();
    }
}