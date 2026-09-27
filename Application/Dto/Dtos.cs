// Application/Dto/Dtos.cs

using System;
using System.Collections.Generic;
using TradeSystem.Domain.Enums;

// Файл содержит набор DTO-классов,
// которые используются для передачи данных между слоями приложения торговой системы.

namespace TradeSystem.Application.Dto;

public sealed class SupplierDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? TaxId { get; init; }

    // Компоненты адреса — нужны форме редактирования.
    public string? Country { get; init; }
    public string? Region { get; init; }
    public string? City { get; init; }
    public string? Street { get; init; }
    public string? Building { get; init; }
    public string? Apartment { get; init; }
    public string? PostalCode { get; init; }

    // Склеенный адрес — для вывода в списках/отчётах.
    public string FullAddress { get; init; } = string.Empty;

    public string Phone { get; init; } = string.Empty;
    public string? Email { get; init; }
    public bool IsActive { get; init; }
    public string? Note { get; init; }
    public int ProductsCount { get; init; }
}
public sealed class ProductDto // DTO товара
{
    public Guid Id { get; init; } // Идентификатор товара
    public string Name { get; init; } = string.Empty; // Наименование товара
    public string Article { get; init; } = string.Empty; // Артикул
    public MeasurementUnit Unit { get; init; } // Единица измерения
    public string UnitLabel { get; init; } = string.Empty; // Название единицы измерения
    public int MinimumStockQuantity { get; init; } // Минимальный остаток
    public int? TargetStockQuantity { get; init; } // Целевой остаток
    public bool IsActive { get; init; } // Признак активности
    public string? Description { get; init; } // Описание
    public Guid? DefaultShelfId { get; init; } // Идентификатор полки по умолчанию
    public string? DefaultShelfName { get; init; } // Название полки по умолчанию

    /// <summary>Суммарный остаток по всем полкам.</summary>
    public int CurrentStock { get; init; } // Текущий остаток

    /// <summary>Сколько активных поставщиков по этому товару.</summary>
    public int SuppliersCount { get; init; } // Количество активных поставщиков

    public bool NeedsReplenish { get; init; } // Признак необходимости пополнения
}

public sealed class ShelfDto // DTO полки
{
    public Guid Id { get; init; } // Идентификатор полки
    public string Code { get; init; } = string.Empty; // Код полки
    public string Name { get; init; } = string.Empty; // Название полки
    public string? Location { get; init; } // Расположение
    public bool IsActive { get; init; } // Признак активности
    public string? Note { get; init; } // Примечание
    public int ProductsCount { get; init; } // Количество товаров на полке
}

public sealed class SupplierProductDto // DTO связи поставщика и товара
{
    public Guid Id { get; init; } // Идентификатор связи
    public Guid ProductId { get; init; } // Идентификатор товара
    public string ProductName { get; init; } = string.Empty; // Наименование товара
    public Guid SupplierId { get; init; } // Идентификатор поставщика
    public string SupplierName { get; init; } = string.Empty; // Название поставщика
    public string? SupplierArticle { get; init; } // Артикул поставщика
    public decimal? PurchasePrice { get; init; } // Закупочная цена
    public int? LeadTimeDays { get; init; } // Срок поставки в днях
    public int? MinimumOrderQuantity { get; init; } // Минимальное количество в заказе
    public int? OrderMultiple { get; init; } // Кратность заказа
    public bool IsPreferred { get; init; } // Признак предпочтительного поставщика
    public bool IsActive { get; init; } // Признак активности
    public string? Note { get; init; } // Примечание
}

public sealed class StockBalanceDto // DTO остатка товара на полке
{
    public Guid ProductId { get; init; } // Идентификатор товара
    public string ProductName { get; init; } = string.Empty; // Наименование товара
    public Guid ShelfId { get; init; } // Идентификатор полки
    public string ShelfCode { get; init; } = string.Empty; // Код полки
    public string ShelfName { get; init; } = string.Empty; // Название полки
    public int Quantity { get; init; } // Количество товара
}

public sealed class LowStockProductDto // DTO товара с недостаточным остатком
{
    public Guid ProductId { get; init; } // Идентификатор товара
    public string ProductName { get; init; } = string.Empty; // Наименование товара
    public string UnitLabel { get; init; } = string.Empty; // Название единицы измерения
    public int CurrentStock { get; init; } // Текущий остаток
    public int MinimumStockQuantity { get; init; } // Минимальный остаток
    public int RecommendedOrderQuantity { get; init; } // Рекомендуемое количество к заказу
}

public sealed class PurchaseOrderLineDto // DTO позиции заказа на покупку
{
    public Guid Id { get; init; } // Идентификатор позиции
    public Guid ProductId { get; init; } // Идентификатор товара
    public string ProductName { get; init; } = string.Empty; // Наименование товара
    public string UnitLabel { get; init; } = string.Empty; // Название единицы измерения
    public int OrderedQuantity { get; init; } // Заказанное количество
    public int ReceivedQuantity { get; init; } // Полученное количество
    public int RemainingQuantity { get; init; } // Оставшееся количество
    public decimal UnitPrice { get; init; } // Цена за единицу
    public decimal TotalPrice { get; init; } // Общая стоимость позиции
    public bool IsClosed { get; init; } // Признак закрытия позиции
}

public sealed class PurchaseOrderDto // DTO заказа на покупку
{
    public Guid Id { get; init; } // Идентификатор заказа
    public string Number { get; init; } = string.Empty; // Номер заказа
    public Guid SupplierId { get; init; } // Идентификатор поставщика
    public string SupplierName { get; init; } = string.Empty; // Название поставщика
    public DateTimeOffset OrderDateUtc { get; init; } // Дата оформления заказа в формате UTC
    public PurchaseOrderStatus Status { get; init; } // Статус заказа
    public string StatusLabel { get; init; } = string.Empty; // Название статуса заказа
    public decimal TotalAmount { get; init; } // Общая сумма заказа
    public PaymentStatus PaymentStatus { get; init; } // Статус оплаты
    public string PaymentStatusLabel { get; init; } = string.Empty; // Название статуса оплаты
    public string? Comment { get; init; } // Комментарий
    public IReadOnlyList<PurchaseOrderLineDto> Lines { get; init; } = Array.Empty<PurchaseOrderLineDto>(); // Позиции заказа
}

public sealed class InvoiceDto // DTO счёта на оплату
{
    public Guid Id { get; init; } // Идентификатор счёта
    public Guid OrderId { get; init; } // Идентификатор заказа
    public string? OrderNumber { get; init; } // Номер заказа
    public string Number { get; init; } = string.Empty; // Номер счёта
    public DateTimeOffset IssueDateUtc { get; init; } // Дата выставления счёта в формате UTC
    public DateTimeOffset? DueDateUtc { get; init; } // Срок оплаты в формате UTC
    public decimal Amount { get; init; } // Сумма счёта
    public PaymentStatus PaymentStatus { get; init; } // Статус оплаты
    public string PaymentStatusLabel { get; init; } = string.Empty; // Название статуса оплаты
    public DateTimeOffset? PaidAtUtc { get; init; } // Дата оплаты в формате UTC
    public string? Comment { get; init; } // Комментарий
}

public sealed class GoodsReceiptLineDto // DTO позиции поступления товара
{
    public Guid Id { get; init; } // Идентификатор позиции
    public Guid? OrderLineId { get; init; } // Идентификатор позиции заказа
    public Guid ProductId { get; init; } // Идентификатор товара
    public string ProductName { get; init; } = string.Empty; // Наименование товара
    public Guid ShelfId { get; init; } // Идентификатор полки
    public string ShelfName { get; init; } = string.Empty; // Название полки
    public int Quantity { get; init; } // Количество товара
    public decimal? UnitCost { get; init; } // Себестоимость единицы
}

public sealed class GoodsReceiptDto // DTO поступления товара
{
    public Guid Id { get; init; } // Идентификатор поступления
    public string Number { get; init; } = string.Empty; // Номер документа поступления
    public Guid SupplierId { get; init; } // Идентификатор поставщика
    public string SupplierName { get; init; } = string.Empty; // Название поставщика
    public Guid? OrderId { get; init; } // Идентификатор связанного заказа
    public string? OrderNumber { get; init; } // Номер связанного заказа
    public DateTimeOffset ReceivedAtUtc { get; init; } // Дата поступления в формате UTC
    public GoodsReceiptStatus Status { get; init; } // Статус поступления
    public string StatusLabel { get; init; } = string.Empty; // Название статуса поступления
    public string? Comment { get; init; } // Комментарий
    public IReadOnlyList<GoodsReceiptLineDto> Lines { get; init; } = Array.Empty<GoodsReceiptLineDto>(); // Позиции поступления
}

public sealed class SaleLineDto // DTO позиции продажи
{
    public Guid Id { get; init; } // Идентификатор позиции
    public Guid ProductId { get; init; } // Идентификатор товара
    public string ProductName { get; init; } = string.Empty; // Наименование товара
    public Guid ShelfId { get; init; } // Идентификатор полки
    public string ShelfName { get; init; } = string.Empty; // Название полки
    public int Quantity { get; init; } // Количество проданного товара
    public decimal UnitPrice { get; init; } // Цена за единицу
    public decimal TotalPrice { get; init; } // Общая стоимость позиции
}

public sealed class SaleDto // DTO продажи
{
    public Guid Id { get; init; } // Идентификатор продажи
    public string Number { get; init; } = string.Empty; // Номер продажи
    public DateTimeOffset SoldAtUtc { get; init; } // Дата продажи в формате UTC
    public SaleStatus Status { get; init; } // Статус продажи
    public string StatusLabel { get; init; } = string.Empty; // Название статуса продажи
    public decimal TotalAmount { get; init; } // Общая сумма продажи
    public string? Comment { get; init; } // Комментарий
    public IReadOnlyList<SaleLineDto> Lines { get; init; } = Array.Empty<SaleLineDto>(); // Позиции продажи
}