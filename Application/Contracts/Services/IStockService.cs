// Application/Contracts/Services/IStockService.cs

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TradeSystem.Application.Dto;
using TradeSystem.Domain.Entities;
using TradeSystem.Domain.Enums;

// Файл содержит интерфейс IStockService,
// который определяет контракт сервиса управления складскими остатками торговой системы.

namespace TradeSystem.Application.Contracts.Services;

public interface IStockService
{
    Task<int> GetTotalStockAsync(Guid productId, CancellationToken ct = default); // Получить суммарный остаток товара по всем полкам

    Task<IReadOnlyDictionary<Guid, int>> GetTotalStocksAsync(CancellationToken ct = default); // Получить суммарные остатки всех товаров

    Task<IReadOnlyList<StockBalanceDto>> GetBalancesByProductAsync(Guid productId, CancellationToken ct = default); // Получить остатки товара по полкам

    Task<IReadOnlyList<StockBalanceDto>> GetBalancesByShelfAsync(Guid shelfId, CancellationToken ct = default); // Получить остатки товаров на полке

    /// <summary>Приход на полку. Пишет движение товара.</summary>
    Task ReceiveAsync( // Оприходовать товар на полку
        Guid productId,
        Guid shelfId,
        int quantity,
        StockMovementType type,
        DocumentType sourceType,
        Guid? sourceId,
        Guid? sourceLineId,
        string? comment,
        CancellationToken ct = default);

    /// <summary>Расход с полки. Бросает BusinessException, если недостаточно.</summary>
    Task ShipAsync( // Списать товар с полки
        Guid productId,
        Guid shelfId,
        int quantity,
        StockMovementType type,
        DocumentType sourceType,
        Guid? sourceId,
        Guid? sourceLineId,
        string? comment,
        CancellationToken ct = default);

    Task<IReadOnlyList<LowStockProductDto>> GetLowStockProductsAsync(CancellationToken ct = default); // Получить список товаров с недостаточным остатком

    /// <summary>Сырые балансы для служебных подсчётов внутри Application.</summary>
    Task<IReadOnlyList<StockBalance>> GetAllBalancesRawAsync(CancellationToken ct = default); // Получить сырые данные остатков товаров

    /// <summary>Удалить нулевой остаток (используется при удалении полки).</summary>
    Task RemoveZeroBalanceAsync(Guid balanceId, CancellationToken ct = default); // Удалить нулевой остаток товара
}