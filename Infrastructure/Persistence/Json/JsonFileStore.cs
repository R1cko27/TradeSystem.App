// Infrastructure/Persistence/Json/JsonFileStore.cs

// Infrastructure/Persistence/Json/JsonFileStore.cs

using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

// Файл содержит класс JsonFileStore,
// который реализует чтение и запись данных в JSON-файл с потокобезопасным доступом.

namespace TradeSystem.Infrastructure.Persistence.Json;

public sealed class JsonFileStore<T> : IJsonFileStore<T>, IDisposable
{
    private readonly string _filePath; // Полный путь к JSON-файлу
    private readonly JsonSerializerOptions _options; // Настройки сериализации JSON
    private readonly SemaphoreSlim _gate = new(1, 1); // Семафор для синхронизации доступа к файлу

    public JsonFileStore(string filePath, bool writeIndented = true)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentException("File path is required.", nameof(filePath));
        }

        _filePath = Path.GetFullPath(filePath);

        _options = new JsonSerializerOptions(JsonSerializerDefaults.Web)
        {
            WriteIndented = writeIndented, // Форматирование JSON с отступами
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull, // Игнорировать null-значения при записи
            Converters =
            {
                new JsonStringEnumConverter() // Сериализация перечислений в виде строк
            }
        };
    }

    public async Task<IReadOnlyList<T>> ReadAsync(CancellationToken cancellationToken = default) // Прочитать все записи из JSON-файла
    {
        await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);

        try
        {
            return await ReadCoreAsync(cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task WriteAsync(IReadOnlyList<T> items, CancellationToken cancellationToken = default) // Записать все записи в JSON-файл
    {
        ArgumentNullException.ThrowIfNull(items);

        await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);

        try
        {
            await WriteCoreAsync(items, cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task UpdateAsync(Func<List<T>, Task> mutateAsync, CancellationToken cancellationToken = default) // Изменить содержимое JSON-файла через делегат
    {
        ArgumentNullException.ThrowIfNull(mutateAsync);

        await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);

        try
        {
            var items = await ReadCoreAsync(cancellationToken).ConfigureAwait(false);

            await mutateAsync(items).ConfigureAwait(false);

            await WriteCoreAsync(items, cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            _gate.Release();
        }
    }

    private async Task<List<T>> ReadCoreAsync(CancellationToken cancellationToken) // Внутреннее чтение данных из файла без блокировки
    {
        if (!File.Exists(_filePath))
        {
            return new List<T>();
        }

        var json = await File.ReadAllTextAsync(_filePath, cancellationToken).ConfigureAwait(false);

        if (string.IsNullOrWhiteSpace(json))
        {
            return new List<T>();
        }

        return JsonSerializer.Deserialize<List<T>>(json, _options) ?? new List<T>();
    }

    private async Task WriteCoreAsync(IReadOnlyList<T> items, CancellationToken cancellationToken) // Внутренняя запись данных в файл без блокировки
    {
        var directory = Path.GetDirectoryName(_filePath);

        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var tempPath = _filePath + ".tmp"; // Временный файл для атомарной записи

        var json = JsonSerializer.Serialize(items, _options);

        await File.WriteAllTextAsync(tempPath, json, cancellationToken).ConfigureAwait(false);

        File.Move(tempPath, _filePath, overwrite: true); // Атомарная замена основного файла
    }

    public void Dispose() // Освобождение ресурсов семафора
    {
        _gate.Dispose();
    }
}