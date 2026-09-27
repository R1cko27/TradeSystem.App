// Domain/Entities/Shelf.cs

namespace TradeSystem.Domain.Entities;

// Файл содержит класс Shelf,
// который представляет полку в торговой системе.

public sealed class Shelf : Entity
{
    public string Code { get; set; } = string.Empty; // Код полки

    public string Name { get; set; } = string.Empty; // Название полки

    public string? Location { get; set; }  // Местоположение полки (необязательное поле)

    public bool IsActive { get; set; } = true; // Флаг активности полки

    public string? Note { get; set; } // Примечание о полке
}