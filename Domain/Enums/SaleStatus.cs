// Domain/Enums/SaleStatus.cs

// Файл содержит перечисление SaleStatus, 
// которое представляет различные статусы продажи,
// используемые в торговой системе.

namespace TradeSystem.Domain.Enums;

public enum SaleStatus
{
    Draft = 0, // Черновик
    Completed = 1, // Завершено
    Cancelled = 2 // Отменено
}