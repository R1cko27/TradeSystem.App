// Infrastructure/Persistence/Json/JsonStorageOptions.cs

// Файл содержит класс JsonStorageOptions,
// который представляет настройки JSON-хранилища торговой системы.

namespace TradeSystem.Infrastructure.Persistence.Json;

public sealed class JsonStorageOptions
{
    public string DirectoryPath { get; set; } = "data"; // Путь к каталогу для хранения JSON-файлов

    public bool WriteIndented { get; set; } = true; // Признак форматирования JSON с отступами
}