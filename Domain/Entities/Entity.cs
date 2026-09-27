// Domain/Entities/Entity.cs

using System;

// Файл содержит абстрактный класс Entity,
// который представляет базовую сущность в торговой системе.

namespace TradeSystem.Domain.Entities;

public abstract class Entity
{
    public Guid Id { get; set; } = Guid.NewGuid(); // Уникальный идентификатор сущности

    public DateTimeOffset CreatedAtUtc { get; set; } = DateTimeOffset.UtcNow; // Дата и время создания сущности в формате UTC

    public DateTimeOffset? ModifiedAtUtc { get; set; } // Дата и время последнего изменения сущности в формате UTC
}