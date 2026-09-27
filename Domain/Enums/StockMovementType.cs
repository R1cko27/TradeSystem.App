// Domain/Enums/StockMovementType.cs

// Файл содержит перечисление StockMovementType, 
// которое представляет различные типы движения запасов,
// используемые в торговой системе.

namespace TradeSystem.Domain.Enums;

public enum StockMovementType
{
    PurchaseReceipt = 1, // Приход товара

    Sale = 2, // Продажа
    WriteOff = 3, // Списание
    TransferIn = 4, // Перемещение (вход)
    TransferOut = 5, // Перемещение (выход)
    Correction = 6 // Коррекция
}