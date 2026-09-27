// Application/Services/StockService.cs

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TradeSystem.Application.Contracts.Repositories;
using TradeSystem.Application.Contracts.Services;
using TradeSystem.Application.Dto;
using TradeSystem.Application.Exceptions;
using TradeSystem.Application.Mappers;
using TradeSystem.Domain.Entities;
using TradeSystem.Domain.Enums;

// Файл содержит класс StockService,
// который реализует сервис управления складскими остатками и движениями товаров торговой системы.

namespace TradeSystem.Application.Services;

public sealed class StockService : IStockService
{
    private readonly IProductRepository _products; // Репозиторий товаров
    private readonly IShelfRepository _shelves; // Репозиторий полок
    private readonly IStockBalanceRepository _balances; // Репозиторий остатков товаров на полках
    private readonly IStockMovementRepository _movements; // Репозиторий движений товаров

    public StockService(
        IProductRepository products,
        IShelfRepository shelves,
        IStockBalanceRepository balances,
        IStockMovementRepository movements)
    {
        _products = products;
        _shelves = shelves;
        _balances = balances;
        _movements = movements;
    }

    public async Task<int> GetTotalStockAsync(Guid productId, CancellationToken ct = default) // Получить суммарный остаток товара по всем полкам
    {
        var balances = await _balances.GetAllAsync(ct);
        return balances.Where(b => b.ProductId == productId).Sum(b => b.Quantity);
    }

    public async Task<IReadOnlyDictionary<Guid, int>> GetTotalStocksAsync(CancellationToken ct = default) // Получить суммарные остатки всех товаров
    {
        var balances = await _balances.GetAllAsync(ct);
        return balances
            .GroupBy(b => b.ProductId)
            .ToDictionary(g => g.Key, g => g.Sum(b => b.Quantity));
    }

    public async Task<IReadOnlyList<StockBalanceDto>> GetBalancesByProductAsync(Guid productId, CancellationToken ct = default) // Получить остатки товара по полкам
    {
        var balances = (await _balances.GetAllAsync(ct)).Where(b => b.ProductId == productId).ToList();
        return await BuildBalanceDtosAsync(balances, ct);
    }

    public async Task<IReadOnlyList<StockBalanceDto>> GetBalancesByShelfAsync(Guid shelfId, CancellationToken ct = default) // Получить остатки товаров на полке
    {
        var balances = (await _balances.GetAllAsync(ct)).Where(b => b.ShelfId == shelfId).ToList();
        return await BuildBalanceDtosAsync(balances, ct);
    }

    public async Task<IReadOnlyList<StockBalance>> GetAllBalancesRawAsync(CancellationToken ct = default) // Получить сырые данные остатков товаров
        => await _balances.GetAllAsync(ct);

    public async Task RemoveZeroBalanceAsync(Guid balanceId, CancellationToken ct = default) // Удалить нулевой остаток товара
    {
        var b = await _balances.GetByIdAsync(balanceId, ct);
        if (b is null) return;
        if (b.Quantity != 0)
            throw new BusinessException("Нельзя удалить ненулевой остаток.");
        await _balances.DeleteAsync(balanceId, ct);
    }

    public async Task ReceiveAsync( // Оприходовать товар на полку
        Guid productId, Guid shelfId, int quantity,
        StockMovementType type, DocumentType sourceType,
        Guid? sourceId, Guid? sourceLineId, string? comment,
        CancellationToken ct = default)
    {
        if (quantity <= 0) throw new BusinessException("Количество прихода должно быть больше нуля.");
        await EnsureProductAndShelfAsync(productId, shelfId, ct);

        var balances = await _balances.GetAllAsync(ct);
        var balance = balances.FirstOrDefault(b => b.ProductId == productId && b.ShelfId == shelfId);

        if (balance is null)
        {
            balance = new StockBalance { ProductId = productId, ShelfId = shelfId, Quantity = quantity };
            await _balances.AddAsync(balance, ct);
        }
        else
        {
            balance.Quantity += quantity;
            await _balances.UpdateAsync(balance, ct);
        }

        await WriteMovementAsync(productId, shelfId, +quantity, type, sourceType, sourceId, sourceLineId, comment, ct);
    }

    public async Task ShipAsync( // Списать товар с полки
        Guid productId, Guid shelfId, int quantity,
        StockMovementType type, DocumentType sourceType,
        Guid? sourceId, Guid? sourceLineId, string? comment,
        CancellationToken ct = default)
    {
        if (quantity <= 0) throw new BusinessException("Количество расхода должно быть больше нуля.");
        await EnsureProductAndShelfAsync(productId, shelfId, ct);

        var balances = await _balances.GetAllAsync(ct);
        var balance = balances.FirstOrDefault(b => b.ProductId == productId && b.ShelfId == shelfId)
                      ?? throw new BusinessException("На этой полке нет данного товара.");

        if (balance.Quantity < quantity)
            throw new BusinessException(
                $"Недостаточно товара на полке: есть {balance.Quantity}, требуется {quantity}.");

        balance.Quantity -= quantity;
        await _balances.UpdateAsync(balance, ct);

        await WriteMovementAsync(productId, shelfId, -quantity, type, sourceType, sourceId, sourceLineId, comment, ct);
    }

    public async Task<IReadOnlyList<LowStockProductDto>> GetLowStockProductsAsync(CancellationToken ct = default) // Получить список товаров с недостаточным остатком
    {
        var products = (await _products.GetAllAsync(ct)).Where(p => p.IsActive && p.MinimumStockQuantity > 0).ToList();
        if (products.Count == 0) return Array.Empty<LowStockProductDto>();

        var stockMap = await GetTotalStocksAsync(ct);

        var result = new List<LowStockProductDto>();
        foreach (var p in products)
        {
            var current = stockMap.GetValueOrDefault(p.Id);
            if (current >= p.MinimumStockQuantity) continue;

            var target = p.TargetStockQuantity ?? (p.MinimumStockQuantity * 2);
            var recommended = Math.Max(1, target - current);

            result.Add(new LowStockProductDto
            {
                ProductId = p.Id,
                ProductName = p.Name,
                UnitLabel = p.Unit.Of(),
                CurrentStock = current,
                MinimumStockQuantity = p.MinimumStockQuantity,
                RecommendedOrderQuantity = recommended
            });
        }

        return result.OrderBy(x => x.ProductName, StringComparer.CurrentCultureIgnoreCase).ToList();
    }

    private async Task EnsureProductAndShelfAsync(Guid productId, Guid shelfId, CancellationToken ct) // Проверить существование товара и полки
    {
        if (await _products.GetByIdAsync(productId, ct) is null)
            throw new BusinessException("Товар не найден.");
        if (await _shelves.GetByIdAsync(shelfId, ct) is null)
            throw new BusinessException("Полка не найдена.");
    }

    private async Task WriteMovementAsync( // Записать движение товара
        Guid productId, Guid shelfId, int delta,
        StockMovementType type, DocumentType sourceType,
        Guid? sourceId, Guid? sourceLineId, string? comment,
        CancellationToken ct)
    {
        var movement = new StockMovement
        {
            ProductId = productId,
            ShelfId = shelfId,
            QuantityDelta = delta,
            Type = type,
            SourceType = sourceType,
            SourceId = sourceId,
            SourceLineId = sourceLineId,
            OccurredAtUtc = DateTimeOffset.UtcNow,
            Comment = comment
        };
        await _movements.AddAsync(movement, ct);
    }

    private async Task<IReadOnlyList<StockBalanceDto>> BuildBalanceDtosAsync(List<StockBalance> balances, CancellationToken ct) // Построить DTO остатков товаров
    {
        if (balances.Count == 0) return Array.Empty<StockBalanceDto>();

        var products = (await _products.GetAllAsync(ct)).ToDictionary(p => p.Id, p => p.Name);
        var shelves = (await _shelves.GetAllAsync(ct)).ToDictionary(s => s.Id, s => s);

        return balances.Select(b =>
        {
            shelves.TryGetValue(b.ShelfId, out var sh);
            return new StockBalanceDto
            {
                ProductId = b.ProductId,
                ProductName = products.GetValueOrDefault(b.ProductId, "?"),
                ShelfId = b.ShelfId,
                ShelfCode = sh?.Code ?? "?",
                ShelfName = sh?.Name ?? "?",
                Quantity = b.Quantity
            };
        }).ToList();
    }
}