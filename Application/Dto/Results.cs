// Application/Dto/Results.cs

using System.Collections.Generic;

// Файл содержит класс AutoOrderResult,
// который представляет результат автоформирования заказов поставщикам в торговой системе.

namespace TradeSystem.Application.Dto;

/// <summary>
/// Результат автоформирования заказов: сами заказы + предупреждения
/// (например, товар без активного поставщика был пропущен).
/// </summary>
public sealed class AutoOrderResult
{
    public List<PurchaseOrderDto> Orders { get; } = new(); // Сформированные заказы
    public List<string> Warnings { get; } = new(); // Предупреждения, возникшие при формировании
}