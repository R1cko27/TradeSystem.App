// ViewModels/Suppliers/SupplierEditViewModel.cs

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TradeSystem.App.ViewModels.Abstractions;
using TradeSystem.Application.Contracts.Services;
using TradeSystem.Application.Dto;
using TradeSystem.Domain.ValueObjects;

// Файл содержит класс SupplierEditViewModel,
// который представляет ViewModel формы создания и редактирования поставщика в торговой системе.

namespace TradeSystem.App.ViewModels.Suppliers;

public partial class SupplierEditViewModel : ViewModelBase
{
    private readonly ICatalogService _catalog; // Сервис справочных данных
    private Guid? _supplierId; // Идентификатор редактируемого поставщика

    [ObservableProperty] private bool _isNew = true; // Признак создания нового поставщика
    [ObservableProperty] private string _name = string.Empty; // Название фирмы
    [ObservableProperty] private string? _taxId; // ИНН/налоговый номер
    [ObservableProperty] private string _phone = string.Empty; // Телефон
    [ObservableProperty] private string? _email; // Электронная почта
    [ObservableProperty] private bool _isActive = true; // Признак активности
    [ObservableProperty] private string? _note; // Примечание

    // Компоненты адреса.
    [ObservableProperty] private string? _country; // Страна
    [ObservableProperty] private string? _region; // Регион
    [ObservableProperty] private string? _city; // Город
    [ObservableProperty] private string? _street; // Улица
    [ObservableProperty] private string? _building; // Здание
    [ObservableProperty] private string? _apartment; // Квартира
    [ObservableProperty] private string? _postalCode; // Почтовый индекс

    public event Action? CloseRequested; // Запрос на закрытие формы

    public SupplierEditViewModel(ICatalogService catalog, IDialogService dialog)
        : base(dialog)
    {
        _catalog = catalog;
    }

    public async Task LoadAsync(Guid? id) // Загрузить данные формы
    {
        await RunAsync(async () =>
        {
            _supplierId = id;
            IsNew = id is null;

            if (id is null)
            {
                ResetForm();
                return;
            }

            var s = await _catalog.GetSupplierAsync(id.Value);
            if (s is null)
            {
                Dialog.ShowError("Поставщик не найден.");
                ResetForm();
                return;
            }

            Name = s.Name;
            TaxId = s.TaxId;
            Phone = s.Phone;
            Email = s.Email;
            IsActive = s.IsActive;
            Note = s.Note;
            Country = s.Country;
            Region = s.Region;
            City = s.City;
            Street = s.Street;
            Building = s.Building;
            Apartment = s.Apartment;
            PostalCode = s.PostalCode;
        });
    }

    [RelayCommand]
    private Task SaveAsync() => RunAsync(SaveImplementationAsync); // Команда сохранения поставщика

    [RelayCommand]
    private void Cancel() => CloseRequested?.Invoke(); // Команда отмены редактирования

    private async Task SaveImplementationAsync() // Сохранить поставщика
    {
        var errors = Validate();
        if (errors.Count > 0)
        {
            Dialog.ShowError(string.Join(Environment.NewLine, errors));
            return;
        }

        var input = new SupplierInput
        {
            Id = _supplierId,
            Name = Name.Trim(),
            TaxId = TrimOrNull(TaxId),
            Phone = Phone.Trim(),
            Email = TrimOrNull(Email),
            IsActive = IsActive,
            Note = TrimOrNull(Note),
            Address = new Address
            {
                Country = TrimOrNull(Country),
                Region = TrimOrNull(Region),
                City = TrimOrNull(City),
                Street = TrimOrNull(Street),
                Building = TrimOrNull(Building),
                Apartment = TrimOrNull(Apartment),
                PostalCode = TrimOrNull(PostalCode)
            }
        };

        if (IsNew)
            await _catalog.CreateSupplierAsync(input);
        else
            await _catalog.UpdateSupplierAsync(input);

        CloseRequested?.Invoke();
    }

    private List<string> Validate() // Проверить корректность введённых данных
    {
        var errors = new List<string>();
        if (string.IsNullOrWhiteSpace(Name))
            errors.Add("Название фирмы обязательно.");
        if (string.IsNullOrWhiteSpace(Phone))
            errors.Add("Телефон обязателен.");
        return errors;
    }

    private void ResetForm() // Сбросить поля формы
    {
        Name = string.Empty;
        TaxId = null;
        Phone = string.Empty;
        Email = null;
        IsActive = true;
        Note = null;
        Country = Region = City = Street = Building = Apartment = PostalCode = null;
    }

    private static string? TrimOrNull(string? value) // Обрезать строку или вернуть null
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        return value.Trim();
    }
}