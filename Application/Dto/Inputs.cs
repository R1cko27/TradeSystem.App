// Application/Dto/Inputs.cs

using System;
using System.Collections.Generic;
using TradeSystem.Domain.Enums;
using TradeSystem.Domain.ValueObjects;

// Файл содержит набор input-классов,
// которые используются для передачи данных от пользовательского интерфейса в прикладной слой торговой системы.

namespace TradeSystem.Application.Dto;

public sealed class SupplierInput // Входные данные поставщика
{
    public Guid? Id { get; set; } // Идентификатор поставщика
    public string Name { get; set; } = string.Empty; // Название фирмы
    public string? TaxId { get; set; } // ИНН/налоговый номер
    public Address Address { get; set; } = new(); // Адрес поставщика
    public string Phone { get; set; } = string.Empty; // Телефон
    public string? Email { get; set; } // Электронная почта
    public bool IsActive { get; set; } = true; // Признак активности
    public string? Note { get; set; } // Примечание
}

public sealed class ProductInput // Входные данные товара
{
    public Guid? Id { get; set; } // Идентификатор товара
    public string Name { get; set; } = string.Empty; // Наименование товара
    public string Article { get; set; } = string.Empty; // Артикул
    public MeasurementUnit Unit { get; set; } = MeasurementUnit.Piece; // Единица измерения
    public int MinimumStockQuantity { get; set; } // Минимальный остаток
    public int? TargetStockQuantity { get; set; } // Целевой остаток
    public bool IsActive { get; set; } = true; // Признак активности
    public string? Description { get; set; } // Описание
    public Guid? DefaultShelfId { get; set; } // Идентификатор полки по умолчанию
}

public sealed class ShelfInput // Входные данные полки
{
    public Guid? Id { get; set; } // Идентификатор полки
    public string Code { get; set; } = string.Empty; // Код полки
    public string Name { get; set; } = string.Empty; // Название полки
    public string? Location { get; set; } // Расположение
    public bool IsActive { get; set; } = true; // Признак активности
    public string? Note { get; set; } // Примечание
}

public sealed class SupplierProductInput // Входные данные связи поставщика и товара
{
    public Guid? Id { get; set; } // Идентификатор связи
    public Guid ProductId { get; set; } // Идентификатор товара
    public Guid SupplierId { get; set; } // Идентификатор поставщика
    public string? SupplierArticle { get; set; } // Артикул поставщика
    public decimal? PurchasePrice { get; set; } // Закупочная цена
    public int? LeadTimeDays { get; set; } // Срок поставки в днях
    public int? MinimumOrderQuantity { get; set; } // Минимальное количество в заказе
    public int? OrderMultiple { get; set; } // Кратность заказа
    public bool IsPreferred { get; set; } // Признак предпочтительного поставщика
    public bool IsActive { get; set; } = true; // Признак активности
    public string? Note { get; set; } // Примечание
}

public sealed class CreatePurchaseOrderLineInput // Входные данные позиции создаваемого заказа
{
    public Guid ProductId { get; set; } // Идентификатор товара
    public int Quantity { get; set; } // Количество товара
    public decimal? UnitPrice { get; set; } // Цена за единицу
    public Guid? SupplierProductId { get; set; } // Идентификатор связи поставщика и товара
}

public sealed class CreatePurchaseOrderInput // Входные данные создаваемого заказа на покупку
{
    public Guid SupplierId { get; set; } // Идентификатор поставщика
    public string? Comment { get; set; } // Комментарий
    public List<CreatePurchaseOrderLineInput> Lines { get; set; } = new(); // Позиции заказа
}

/// <summary>
/// Параметры автоформирования заказов по дефициту.
/// ExplicitSupplierChoice позволяет вручную указать поставщика для товара,
/// который поставляют несколько фирм (ключ — ProductId, значение — SupplierId).
/// AdditionalProductIds — дополнительные товары, включаемые в заказ сверх дефицита.
/// </summary>
public sealed class AutoOrderRequest // Параметры автоформирования заказов по дефициту
{
    public Dictionary<Guid, Guid>? ExplicitSupplierChoice { get; set; } // Явный выбор поставщика для товара
    public HashSet<Guid>? AdditionalProductIds { get; set; } // Дополнительные товары для включения в заказ
}

public sealed class GoodsReceiptLineInput // Входные данные позиции поступления товара
{
    public Guid? OrderLineId { get; set; } // Идентификатор позиции заказа
    public Guid ProductId { get; set; } // Идентификатор товара
    public Guid ShelfId { get; set; } // Идентификатор полки
    public int Quantity { get; set; } // Количество товара
    public decimal? UnitCost { get; set; } // Себестоимость единицы
}

public sealed class GoodsReceiptInput // Входные данные поступления товара
{
    public Guid SupplierId { get; set; } // Идентификатор поставщика
    public Guid? OrderId { get; set; } // Идентификатор связанного заказа
    public string? Comment { get; set; } // Комментарий
    public List<GoodsReceiptLineInput> Lines { get; set; } = new(); // Позиции поступления
}

public sealed class SaleLineInput // Входные данные позиции продажи
{
    public Guid ProductId { get; set; } // Идентификатор товара
    public Guid ShelfId { get; set; } // Идентификатор полки
    public int Quantity { get; set; } // Количество товара
    public decimal UnitPrice { get; set; } // Цена за единицу
}

public sealed class SaleInput // Входные данные продажи
{
    public string? Comment { get; set; } // Комментарий
    public List<SaleLineInput> Lines { get; set; } = new(); // Позиции продажи
}