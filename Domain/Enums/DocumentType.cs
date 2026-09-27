// Domain/Enums/DocumentType.cs

// Файл содержит перечисление DocumentType, 
// которое представляет различные типы документов,
// используемые в торговой системе.

namespace TradeSystem.Domain.Enums;

public enum DocumentType
{
    None = 0, // Нет типа

    PurchaseOrder = 1, // Заказ на покупку
    GoodsReceipt = 2, // Поступление товаров
    Sale = 3, // Продажа
    InventoryCorrection = 4 // Коррекция запасов
}