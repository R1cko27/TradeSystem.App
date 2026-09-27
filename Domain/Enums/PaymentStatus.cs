// Domain/Enums/PaymentStatus.cs

// Файл содержит перечисление PaymentStatus, 
// которое представляет различные статусы оплаты,
// используемые в торговой системе.

namespace TradeSystem.Domain.Enums;

public enum PaymentStatus
{
    Unpaid = 0, // Не оплачено
    PartiallyPaid = 1, // Частично оплачено
    Paid = 2, // Оплачено
    Cancelled = 3 // Отменено
}