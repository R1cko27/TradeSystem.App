// ViewModels/SupplierProducts/SupplierProductEditViewModel.cs

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TradeSystem.App.ViewModels.Abstractions;
using TradeSystem.Application.Contracts.Services;
using TradeSystem.Application.Dto;

// Файл содержит класс SupplierProductEditViewModel,
// который представляет ViewModel формы создания и редактирования связи «товар-поставщик» в торговой системе.

namespace TradeSystem.App.ViewModels.SupplierProducts;

public partial class SupplierProductEditViewModel : ViewModelBase
{
    private readonly ICatalogService _catalog; // Сервис справочных данных
    private Guid? _linkId; // Идентификатор редактируемой связи

    [ObservableProperty] private bool _isNew = true; // Признак создания новой связи
    [ObservableProperty] private Guid _productId; // Идентификатор товара
    [ObservableProperty] private Guid _supplierId; // Идентификатор поставщика
    [ObservableProperty] private string? _supplierArticle; // Артикул поставщика
    [ObservableProperty] private string _purchasePriceText = string.Empty; // Закупочная цена в виде текста
    [ObservableProperty] private string _leadTimeText = string.Empty; // Срок поставки в виде текста
    [ObservableProperty] private string _minOrderText = string.Empty; // Минимальное количество заказа в виде текста
    [ObservableProperty] private string _multipleText = string.Empty; // Кратность заказа в виде текста
    [ObservableProperty] private bool _isPreferred; // Признак предпочтительного поставщика
    [ObservableProperty] private bool _isActive = true; // Признак активности
    [ObservableProperty] private string? _note; // Примечание

    [ObservableProperty] private IReadOnlyList<ProductDto> _products = Array.Empty<ProductDto>(); // Список товаров для выбора
    [ObservableProperty] private IReadOnlyList<SupplierDto> _suppliers = Array.Empty<SupplierDto>(); // Список поставщиков для выбора

    public event Action? CloseRequested; // Запрос на закрытие формы

    public SupplierProductEditViewModel(ICatalogService catalog, IDialogService dialog)
        : base(dialog)
    {
        _catalog = catalog;
    }

    public async Task LoadAsync(Guid? id) // Загрузить данные формы
    {
        await RunAsync(async () =>
        {
            _linkId = id;
            IsNew = id is null;

            // Списки для ComboBox (без «все») — грузим активные.
            Products = await _catalog.GetProductsAsync(includeInactive: false);
            Suppliers = await _catalog.GetSuppliersAsync(includeInactive: false);

            if (id is null)
            {
                ResetForm();
                return;
            }

            var all = await _catalog.GetSupplierProductsAsync();
            var link = all.FirstOrDefault(x => x.Id == id.Value);
            if (link is null)
            {
                Dialog.ShowError("Связь не найдена.");
                ResetForm();
                return;
            }

            ProductId = link.ProductId;
            SupplierId = link.SupplierId;
            SupplierArticle = link.SupplierArticle;
            PurchasePriceText = link.PurchasePrice?.ToString(CultureInfo.CurrentCulture) ?? string.Empty;
            LeadTimeText = link.LeadTimeDays?.ToString(CultureInfo.InvariantCulture) ?? string.Empty;
            MinOrderText = link.MinimumOrderQuantity?.ToString(CultureInfo.InvariantCulture) ?? string.Empty;
            MultipleText = link.OrderMultiple?.ToString(CultureInfo.InvariantCulture) ?? string.Empty;
            IsPreferred = link.IsPreferred;
            IsActive = link.IsActive;
            Note = link.Note;
        });
    }

    [RelayCommand]
    private Task SaveAsync() => RunAsync(SaveImplementationAsync); // Команда сохранения связи

    [RelayCommand]
    private void Cancel() => CloseRequested?.Invoke(); // Команда отмены редактирования

    private async Task SaveImplementationAsync() // Сохранить связь
    {
        var errors = Validate(out decimal? price, out int? lead, out int? min, out int? mult);
        if (errors.Count > 0)
        {
            Dialog.ShowError(string.Join(Environment.NewLine, errors));
            return;
        }

        var input = new SupplierProductInput
        {
            Id = _linkId,
            ProductId = ProductId,
            SupplierId = SupplierId,
            SupplierArticle = TrimOrNull(SupplierArticle),
            PurchasePrice = price,
            LeadTimeDays = lead,
            MinimumOrderQuantity = min,
            OrderMultiple = mult,
            IsPreferred = IsPreferred,
            IsActive = IsActive,
            Note = TrimOrNull(Note)
        };

        if (IsNew)
            await _catalog.LinkProductToSupplierAsync(input);
        else
            await _catalog.UpdateLinkAsync(input);

        CloseRequested?.Invoke();
    }

    private List<string> Validate( // Проверить корректность введённых данных
        out decimal? price, out int? lead, out int? min, out int? mult)
    {
        price = lead = min = mult = null;
        var errors = new List<string>();

        if (ProductId == Guid.Empty)
            errors.Add("Выберите товар.");
        if (SupplierId == Guid.Empty)
            errors.Add("Выберите поставщика.");

        if (!TryParseDecimal(PurchasePriceText, out price))
            errors.Add("Цена закупки: введите число или оставьте поле пустым.");
        else if (price < 0)
            errors.Add("Цена закупки не может быть отрицательной.");

        if (!TryParseInt(LeadTimeText, out lead))
            errors.Add("Срок поставки: целое число дней или пусто.");
        else if (lead < 0)
            errors.Add("Срок поставки не может быть отрицательным.");

        if (!TryParseInt(MinOrderText, out min))
            errors.Add("Минимальное количество заказа: целое число или пусто.");
        else if (min < 0)
            errors.Add("Минимальное количество заказа не может быть отрицательным.");

        if (!TryParseInt(MultipleText, out mult))
            errors.Add("Кратность заказа: целое число или пусто.");
        else if (mult is < 1)
            errors.Add("Кратность заказа должна быть не меньше 1.");

        return errors;
    }

    private void ResetForm() // Сбросить поля формы
    {
        ProductId = Guid.Empty;
        SupplierId = Guid.Empty;
        SupplierArticle = null;
        PurchasePriceText = string.Empty;
        LeadTimeText = string.Empty;
        MinOrderText = string.Empty;
        MultipleText = string.Empty;
        IsPreferred = false;
        IsActive = true;
        Note = null;
    }

    private static bool TryParseDecimal(string? text, out decimal? value) // Разобрать десятичное число из текста
    {
        value = null;
        if (string.IsNullOrWhiteSpace(text)) return true;
        if (decimal.TryParse(text.Trim(), NumberStyles.Number, CultureInfo.CurrentCulture, out var d))
        {
            value = d;
            return true;
        }
        return false;
    }

    private static bool TryParseInt(string? text, out int? value) // Разобрать целое число из текста
    {
        value = null;
        if (string.IsNullOrWhiteSpace(text)) return true;
        if (int.TryParse(text.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var i))
        {
            value = i;
            return true;
        }
        return false;
    }

    private static string? TrimOrNull(string? value) => // Обрезать строку или вернуть null
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}