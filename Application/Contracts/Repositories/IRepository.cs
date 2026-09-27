// Application/Contracts/Repositories/IRepository.cs

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TradeSystem.Domain.Entities;

// Файл содержит интерфейс IRepository,
// который определяет базовый контракт репозитория для сущностей торговой системы.

namespace TradeSystem.Application.Contracts.Repositories;

public interface IRepository<TEntity> where TEntity : Entity
{
    Task<IReadOnlyList<TEntity>> GetAllAsync(CancellationToken cancellationToken = default); // Получить все сущности

    Task<TEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default); // Получить сущность по идентификатору

    Task AddAsync(TEntity entity, CancellationToken cancellationToken = default); // Добавить новую сущность

    Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default); // Обновить существующую сущность

    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default); // Удалить сущность по идентификатору
}