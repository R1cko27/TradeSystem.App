// ViewModels/Products/ProductEditViewModel.cs

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TradeSystem.App.ViewModels.Abstractions;
using TradeSystem.Application.Contracts.Services;
using TradeSystem.Application.Dto;
using TradeSystem.Application.Mappers;
using TradeSystem.Domain.Enums;

// Файл содержит класс ProductEditViewModel,
// который представляет ViewModel формы создания и редактирования товара в торговой системе.

namespace TradeSystem.App.ViewModels.Products;

public partial class ProductEditViewModel : ViewModelBase
{
    private readonly ICatalogService _catalog; // Сервис справочных данных
    private Guid? _productId; // Идентификатор редактируемого товара

    [ObservableProperty] private bool _isNew = true; // Признак создания нового товара
    [ObservableProperty] private string _name = string.Empty; // Наименование товара
    [ObservableProperty] private string _article = string.Empty; // Артикул товара
    [ObservableProperty] private MeasurementUnit _unit = MeasurementUnit.Piece; // Единица измерения
    [ObservableProperty] private int _minimumStockQuantity; // Минимальный остаток
    [ObservableProperty] private string _targetStockText = string.Empty; // Целевой остаток в виде текста
    [ObservableProperty] private Guid? _defaultShelfId; // Идентификатор полки по умолчанию
    [ObservableProperty] private IReadOnlyList<ShelfDto> _shelves = Array.Empty<ShelfDto>(); // Список доступных полок

    public IReadOnlyList<UnitOption> AvailableUnits { get; } = // Список единиц измерения для выбора
        Enum.GetValues<MeasurementUnit>()
            .Select(u => new UnitOption(u, u.Of()))
            .ToList();

    public event Action? CloseRequested; // Запрос на закрытие формы

    public ProductEditViewModel(ICatalogService catalog, IDialogService dialog)
        : base(dialog)
    {
        _catalog = catalog;
    }

    /// <summary>Загружает форму. id == null — новый товар.</summary>
    public async Task LoadAsync(Guid? id) // Загрузить данные формы
    {
        await RunAsync(async () =>
        {
            _productId = id;
            IsNew = id is null;

            Shelves = await _catalog.GetShelvesAsync(includeInactive: false);

            if (id is null)
            {
                ResetForm();
                return;
            }

            var p = await _catalog.GetProductAsync(id.Value);
            if (p is null)
            {
                Dialog.ShowError("Товар не найден.");
                ResetForm();
                return;
            }

            Name = p.Name;
            Article = p.Article;
            Unit = p.Unit;
            MinimumStockQuantity = p.MinimumStockQuantity;
            TargetStockText = p.TargetStockQuantity?.ToString() ?? string.Empty;
            DefaultShelfId = p.DefaultShelfId;
        });
    }

    [RelayCommand]
    private Task SaveAsync() => RunAsync(SaveImplementationAsync); // Команда сохранения товара

    [RelayCommand]
    private void Cancel() => CloseRequested?.Invoke(); // Команда отмены редактирования

    private async Task SaveImplementationAsync() // Сохранить товар
    {
        var errors = Validate(out int? target);
        if (errors.Count > 0)
        {
            Dialog.ShowError(string.Join(Environment.NewLine, errors));
            return;
        }

        var input = new ProductInput
        {
            Id = _productId,
            Name = Name.Trim(),
            Article = Article.Trim(),
            Unit = Unit,
            MinimumStockQuantity = MinimumStockQuantity,
            TargetStockQuantity = target,
            IsActive = true,
            DefaultShelfId = DefaultShelfId
        };

        if (IsNew)
            await _catalog.CreateProductAsync(input);
        else
            await _catalog.UpdateProductAsync(input);

        CloseRequested?.Invoke();
    }

    private List<string> Validate(out int? target) // Проверить корректность введённых данных
    {
        target = null;
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(Name))
            errors.Add("Наименование товара обязательно.");
        if (string.IsNullOrWhiteSpace(Article))
            errors.Add("Артикул товара обязателен.");
        if (MinimumStockQuantity < 0)
            errors.Add("Минимальный остаток не может быть отрицательным.");

        if (!string.IsNullOrWhiteSpace(TargetStockText))
        {
            if (!int.TryParse(TargetStockText.Trim(), out var t) || t < 0)
                errors.Add("Целевой остаток должен быть целым числом >= 0.");
            else
                target = t;
        }

        return errors;
    }

    private void ResetForm() // Сбросить поля формы
    {
        Name = string.Empty;
        Article = string.Empty;
        Unit = MeasurementUnit.Piece;
        MinimumStockQuantity = 0;
        TargetStockText = string.Empty;
        DefaultShelfId = null;
    }
}