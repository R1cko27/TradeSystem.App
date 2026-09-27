// Domain/ValueObjects/Address.cs

// Файл содержит класс Address,
// который представляет адрес в торговой системе.

namespace TradeSystem.Domain.ValueObjects;

public sealed class Address
{
    public string? Country { get; set; } // Страна
    public string? Region { get; set; } // Регион
    public string? City { get; set; } // Город
    public string? Street { get; set; } // Улица
    public string? Building { get; set; } // Здание
    public string? Apartment { get; set; } // Квартира
    public string? PostalCode { get; set; } // Почтовый индекс
}