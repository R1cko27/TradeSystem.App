// Infrastructure/Persistence/Json/IJsonFileStore.cs

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

// Файл содержит интерфейс IJsonFileStore,
// который определяет контракт для чтения и записи данных в JSON-файл.

namespace TradeSystem.Infrastructure.Persistence.Json;

public interface IJsonFileStore<T>
{
    Task<IReadOnlyList<T>> ReadAsync(CancellationToken cancellationToken = default); // Прочитать все записи из JSON-файла

    Task WriteAsync(IReadOnlyList<T> items, CancellationToken cancellationToken = default); // Записать все записи в JSON-файл

    Task UpdateAsync(Func<List<T>, Task> mutateAsync, CancellationToken cancellationToken = default); // Изменить содержимое JSON-файла через делегат
}