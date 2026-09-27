// Application/Services/SaleService.cs

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

// Файл содержит класс SaleService,
// который реализует сервис управления продажами товаров в торговой системе.

namespace TradeSystem.Application.Services;

public sealed class SaleService : ISaleService
{
    private readonly ISaleRepository _sales; // Репозиторий продаж
    private readonly IProductRepository _products; // Репозиторий товаров
    private readonly IShelfRepository _shelves; // Репозиторий полок
    private readonly IStockService _stock; // Сервис складских остатков

    public SaleService(
        ISaleRepository sales,
        IProductRepository products,
        IShelfRepository shelves,
        IStockService stock)
    {
        _sales = sales;
        _products = products;
        _shelves = shelves;
        _stock = stock;
    }

    public async Task<IReadOnlyList<SaleDto>> GetSalesAsync(CancellationToken ct = default) // Получить список продаж
    {
        var all = await _sales.GetAllAsync(ct);
        return await BuildDtosAsync(all.ToList(), ct);
    }

    public async Task<SaleDto?> GetSaleAsync(Guid id, CancellationToken ct = default) // Получить продажу по идентификатору
    {
        var s = await _sales.GetByIdAsync(id, ct);
        if (s is null) return null;
        var list = await BuildDtosAsync(new List<Sale> { s }, ct);
        return list.FirstOrDefault();
    }

    public async Task<SaleDto> CreateDraftAsync(SaleInput input, CancellationToken ct = default) // Создать черновик продажи
    {
        if (input.Lines.Count == 0) throw new BusinessException("Продажа не содержит позиций.");

        var products = (await _products.GetAllAsync(ct)).ToDictionary(p => p.Id);
        var shelves = (await _shelves.GetAllAsync(ct)).ToDictionary(s => s.Id);

        var sale = new Sale
        {
            Number = await GenerateNumberAsync("SALE", ct),
            SoldAtUtc = DateTimeOffset.UtcNow,
            Status = SaleStatus.Draft,
            Comment = input.Comment
        };

        foreach (var li in input.Lines)
        {
            if (li.Quantity <= 0) throw new BusinessException("Количество должно быть больше нуля.");
            if (!products.ContainsKey(li.ProductId)) throw new BusinessException("Товар не найден.");
            if (!shelves.ContainsKey(li.ShelfId)) throw new BusinessException("Полка не найдена.");

            sale.Lines.Add(new SaleLine
            {
                ProductId = li.ProductId,
                ProductName = products[li.ProductId].Name,
                ShelfId = li.ShelfId,
                Quantity = li.Quantity,
                UnitPrice = li.UnitPrice,
                TotalPrice = li.UnitPrice * li.Quantity
            });
        }

        await _sales.AddAsync(sale, ct);
        var dtos = await BuildDtosAsync(new List<Sale> { sale }, ct);
        return dtos.First();
    }

    public async Task<SaleDto> PostAsync(Guid saleId, CancellationToken ct = default) // Провести продажу
    {
        var sale = await _sales.GetByIdAsync(saleId, ct)
                   ?? throw new BusinessException("Продажа не найдена.");

        if (sale.Status != SaleStatus.Draft)
            throw new BusinessException("Провести можно только продажу в статусе «Черновик».");

        foreach (var line in sale.Lines)
        {
            await _stock.ShipAsync(
                line.ProductId, line.ShelfId, line.Quantity,
                StockMovementType.Sale, DocumentType.Sale,
                sale.Id, line.Id, null, ct);
        }

        sale.Status = SaleStatus.Completed;
        await _sales.UpdateAsync(sale, ct);

        var dtos = await BuildDtosAsync(new List<Sale> { sale }, ct);
        return dtos.First();
    }

    public async Task DeleteDraftAsync(Guid saleId, CancellationToken ct = default) // Удалить черновик продажи
    {
        var sale = await _sales.GetByIdAsync(saleId, ct)
                   ?? throw new BusinessException("Продажа не найдена.");
        if (sale.Status != SaleStatus.Draft)
            throw new BusinessException("Удалять можно только черновик продажи.");
        await _sales.DeleteAsync(saleId, ct);
    }

    private async Task<string> GenerateNumberAsync(string prefix, CancellationToken ct) // Сгенерировать номер продажи
    {
        var all = await _sales.GetAllAsync(ct);
        var today = DateTime.UtcNow.ToString("yyyyMMdd");
        var n = all.Count(s => s.Number.StartsWith($"{prefix}-{today}-", StringComparison.Ordinal));
        return $"{prefix}-{today}-{n + 1:D4}";
    }

    private async Task<IReadOnlyList<SaleDto>> BuildDtosAsync(List<Sale> sales, CancellationToken ct) // Построить DTO продаж
    {
        if (sales.Count == 0) return Array.Empty<SaleDto>();

        var products = (await _products.GetAllAsync(ct)).ToDictionary(p => p.Id, p => p.Name);
        var shelves = (await _shelves.GetAllAsync(ct)).ToDictionary(s => s.Id, s => s.Name);

        return sales
            .OrderByDescending(s => s.SoldAtUtc)
            .Select(s => DtoMapper.ToDto(s, line =>
                (products.GetValueOrDefault(line.ProductId, "?"),
                 shelves.GetValueOrDefault(line.ShelfId, "?"))))
            .ToList();
    }
}