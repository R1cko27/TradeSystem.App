// Application/Exceptions/BusinessException.cs

using System;

// Файл содержит класс BusinessException,
// который представляет исключение нарушения бизнес-правила предметной области торговой системы.

namespace TradeSystem.Application.Exceptions;

/// <summary>
/// Ошибка, нарушающая бизнес-правило предметной области
/// (недостаточный остаток, товар не у этого поставщика и т.п.).
/// </summary>
public sealed class BusinessException : Exception
{
    public BusinessException(string message) : base(message) // Создать исключение с сообщением
    {
    }

    public BusinessException(string message, Exception innerException)
        : base(message, innerException) // Создать исключение с сообщением и вложенным исключением
    {
    }
}