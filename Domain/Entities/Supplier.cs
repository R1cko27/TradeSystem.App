// Domain/Entities/Supplier.cs

using TradeSystem.Domain.ValueObjects;

// Файл содержит класс Supplier,
// который представляет поставщика в торговой системе.

namespace TradeSystem.Domain.Entities;

public sealed class Supplier : Entity
{
    public string Name { get; set; } = string.Empty; // Название поставщика

    public string? TaxId { get; set; } // Идентификационный номер налогоплательщика (ИНН)

    public Address Address { get; set; } = new(); // Адрес поставщика

    public string Phone { get; set; } = string.Empty; // Телефон поставщика

    public string? Email { get; set; } // Электронная почта поставщика

    public bool IsActive { get; set; } = true; // Флаг активности поставщика

    public string? Note { get; set; } // Примечание о поставщике
}