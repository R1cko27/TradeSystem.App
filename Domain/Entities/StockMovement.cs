// Domain/Entities/StockMovement.cs

using System;
using TradeSystem.Domain.Enums;

// Файл содержит определение класса StockMovement,
// который представляет собой сущность движения товара на складе в системе торговли.

namespace TradeSystem.Domain.Entities;

public sealed class StockMovement : Entity
{
    public Guid ProductId { get; set; } // Идентификатор товара

    public Guid ShelfId { get; set; } // Идентификатор полки

    public int QuantityDelta { get; set; } // Изменение количества товара

    public StockMovementType Type { get; set; } // Тип движения товара

    public DocumentType SourceType { get; set; } = DocumentType.None; // Тип документа-источника движения

    public Guid? SourceId { get; set; } // Идентификатор документа-источника

    public Guid? SourceLineId { get; set; } // Идентификатор позиции документа-источника

    public DateTimeOffset OccurredAtUtc { get; set; } = DateTimeOffset.UtcNow; // Дата и время движения товара в формате UTC

    public string? Comment { get; set; } // Комментарий к движению товара
}