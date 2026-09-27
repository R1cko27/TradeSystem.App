// Infrastructure/Persistence/Json/JsonFileRepository.cs

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TradeSystem.Application.Contracts.Repositories;
using TradeSystem.Domain.Entities;

// Файл содержит класс JsonFileRepository,
// который реализует базовый репозиторий для сущностей торговой системы на основе JSON-файла.

namespace TradeSystem.Infrastructure.Persistence.Json;

public class JsonFileRepository<TEntity> : IRepository<TEntity> where TEntity : Entity
{
    protected JsonFileRepository(IJsonFileStore<TEntity> store)
    {
        Store = store ?? throw new ArgumentNullException(nameof(store));
    }

    protected IJsonFileStore<TEntity> Store { get; } // Хранилище JSON-файла для сущностей

    public virtual async Task<IReadOnlyList<TEntity>> GetAllAsync(CancellationToken cancellationToken = default) // Получить все сущности
    {
        return await Store.ReadAsync(cancellationToken).ConfigureAwait(false);
    }

    public virtual async Task<TEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) // Получить сущность по идентификатору
    {
        if (id == Guid.Empty)
        {
            return null;
        }

        var items = await Store.ReadAsync(cancellationToken).ConfigureAwait(false);

        return items.FirstOrDefault(item => item.Id == id);
    }

    public virtual async Task AddAsync(TEntity entity, CancellationToken cancellationToken = default) // Добавить новую сущность
    {
        ArgumentNullException.ThrowIfNull(entity);

        if (entity.Id == Guid.Empty)
        {
            entity.Id = Guid.NewGuid();
        }

        await Store.UpdateAsync(items =>
        {
            if (items.Any(item => item.Id == entity.Id))
            {
                throw new InvalidOperationException(
                    $"{typeof(TEntity).Name} with Id {entity.Id} already exists.");
            }

            if (entity.CreatedAtUtc == default)
            {
                entity.CreatedAtUtc = DateTimeOffset.UtcNow;
            }

            items.Add(entity);

            return Task.CompletedTask;
        }, cancellationToken).ConfigureAwait(false);
    }

    public virtual async Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default) // Обновить существующую сущность
    {
        ArgumentNullException.ThrowIfNull(entity);

        await Store.UpdateAsync(items =>
        {
            var index = items.FindIndex(item => item.Id == entity.Id);

            if (index < 0)
            {
                throw new KeyNotFoundException(
                    $"{typeof(TEntity).Name} with Id {entity.Id} was not found.");
            }

            entity.CreatedAtUtc = items[index].CreatedAtUtc; // Сохранить исходную дату создания
            entity.ModifiedAtUtc = DateTimeOffset.UtcNow; // Установить дату последнего изменения

            items[index] = entity;

            return Task.CompletedTask;
        }, cancellationToken).ConfigureAwait(false);
    }

    public virtual async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default) // Удалить сущность по идентификатору
    {
        if (id == Guid.Empty)
        {
            return false;
        }

        var deleted = false;

        await Store.UpdateAsync(items =>
        {
            deleted = items.RemoveAll(item => item.Id == id) > 0;

            return Task.CompletedTask;
        }, cancellationToken).ConfigureAwait(false);

        return deleted;
    }
}