// Domain/Enums/MeasurementUnit.cs

// Файл содержит перечисление MeasurementUnit, 
// которое представляет различные единицы измерения, 
// используемые в торговой системе. 
// Каждое значение перечисления имеет уникальный целочисленный идентификатор, 
// который может быть использован для хранения и обработки данных о единицах 
// измерения в базе данных или других структурах данных.

namespace TradeSystem.Domain.Enums;

public enum MeasurementUnit
{
    NotSpecified = 0,
    Piece = 1,
    Kilogram = 2,
    Liter = 3,
    Meter = 4,
    Pack = 5,
    Box = 6,
    Other = 99
}