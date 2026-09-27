// Domain/Enums/PurchaseOrderStatus.cs

// Файл содержит перечисление PurchaseOrderStatus, 
// которое представляет различные статусы заказа на покупку,
// используемые в торговой системе.
namespace TradeSystem.Domain.Enums;

public enum PurchaseOrderStatus
{
    Draft = 0, // Черновик
    Confirmed = 1, // Подтвержден
    Sent = 2, // Отправлен
    PartiallyReceived = 3, // Частично получен
    Received = 4, // Получен
    Closed = 5, // Закрыт
    Cancelled = 6 // Отменен
}