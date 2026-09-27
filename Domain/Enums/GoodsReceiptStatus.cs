// Domain/Enums/GoodsReceiptStatus.cs

// Файл содержит перечисление GoodsReceiptStatus, 
// которое представляет различные статусы поступления товаров,
// используемые в торговой системе.

namespace TradeSystem.Domain.Enums;

public enum GoodsReceiptStatus
{
    Draft = 0, // Черновик  
    Posted = 1, // Размещен
    Cancelled = 2 // Отменен
}