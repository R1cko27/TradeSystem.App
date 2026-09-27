// Application/Mappers/Labels.cs

using System.Collections.Generic;
using TradeSystem.Domain.Enums;

// Файл содержит класс Labels,
// который предоставляет методы расширения для получения русскоязычных названий значений перечислений торговой системы.

namespace TradeSystem.Application.Mappers;

public static class Labels
{
    private static readonly Dictionary<MeasurementUnit, string> Units = new() // Названия единиц измерения
    {
        [MeasurementUnit.NotSpecified] = "не указано",
        [MeasurementUnit.Piece]        = "шт.",
        [MeasurementUnit.Kilogram]     = "кг",
        [MeasurementUnit.Liter]        = "л",
        [MeasurementUnit.Meter]        = "м",
        [MeasurementUnit.Pack]         = "уп.",
        [MeasurementUnit.Box]          = "кор.",
        [MeasurementUnit.Other]        = "др."
    };

    private static readonly Dictionary<PurchaseOrderStatus, string> OrderStatuses = new() // Названия статусов заказа на покупку
    {
        [PurchaseOrderStatus.Draft]             = "Черновик",
        [PurchaseOrderStatus.Confirmed]         = "Подтверждён",
        [PurchaseOrderStatus.Sent]              = "Отправлен",
        [PurchaseOrderStatus.PartiallyReceived] = "Частично получен",
        [PurchaseOrderStatus.Received]          = "Получен",
        [PurchaseOrderStatus.Closed]            = "Закрыт",
        [PurchaseOrderStatus.Cancelled]         = "Отменён"
    };

    private static readonly Dictionary<PaymentStatus, string> PaymentStatuses = new() // Названия статусов оплаты счёта
    {
        [PaymentStatus.Unpaid]         = "Не оплачен",
        [PaymentStatus.PartiallyPaid]  = "Частично оплачен",
        [PaymentStatus.Paid]           = "Оплачен",
        [PaymentStatus.Cancelled]      = "Отменён"
    };

    private static readonly Dictionary<GoodsReceiptStatus, string> ReceiptStatuses = new() // Названия статусов поступления товара
    {
        [GoodsReceiptStatus.Draft]     = "Черновик",
        [GoodsReceiptStatus.Posted]    = "Проведён",
        [GoodsReceiptStatus.Cancelled] = "Отменён"
    };

    private static readonly Dictionary<SaleStatus, string> SaleStatuses = new() // Названия статусов продажи
    {
        [SaleStatus.Draft]     = "Черновик",
        [SaleStatus.Completed] = "Завершена",
        [SaleStatus.Cancelled] = "Отменена"
    };

    public static string Of(this MeasurementUnit unit) => // Получить русскоязычное название единицы измерения
        Units.TryGetValue(unit, out var v) ? v : unit.ToString();

    public static string Of(this PurchaseOrderStatus status) => // Получить русскоязычное название статуса заказа на покупку
        OrderStatuses.TryGetValue(status, out var v) ? v : status.ToString();

    public static string Of(this PaymentStatus status) => // Получить русскоязычное название статуса оплаты счёта
        PaymentStatuses.TryGetValue(status, out var v) ? v : status.ToString();

    public static string Of(this GoodsReceiptStatus status) => // Получить русскоязычное название статуса поступления товара
        ReceiptStatuses.TryGetValue(status, out var v) ? v : status.ToString();

    public static string Of(this SaleStatus status) => // Получить русскоязычное название статуса продажи
        SaleStatuses.TryGetValue(status, out var v) ? v : status.ToString();
}