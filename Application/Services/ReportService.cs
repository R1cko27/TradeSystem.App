// Application/Services/ReportService.cs

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TradeSystem.Application.Contracts.Repositories;
using TradeSystem.Application.Contracts.Services;
using TradeSystem.Application.Dto;
using TradeSystem.Application.Mappers;
using TradeSystem.Domain.Entities;

// Файл содержит класс ReportService,
// который реализует сервис формирования справочной информации в торговой системе.

namespace TradeSystem.Application.Services;

public sealed class ReportService : IReportService
{
    private readonly IProductRepository _products; // Репозиторий товаров
    private readonly IShelfRepository _shelves; // Репозиторий полок
    private readonly ISupplierRepository _suppliers; // Репозиторий поставщиков
    private readonly ISupplierProductRepository _supplierProducts; // Репозиторий связей поставщиков и товаров
    private readonly IStockService _stock; // Сервис складских остатков

    public ReportService(
        IProductRepository products,
        IShelfRepository shelves,
        ISupplierRepository suppliers,
        ISupplierProductRepository supplierProducts,
        IStockService stock)
    {
        _products = products;
        _shelves = shelves;
        _suppliers = suppliers;
        _supplierProducts = supplierProducts;
        _stock = stock;
    }

    public async Task<IReadOnlyList<ProductDto>> GetAllProductsReportAsync(CancellationToken ct = default) // Отчёт по всем товарам
    {
        var products = (await _products.GetAllAsync(ct)).Where(p => p.IsActive).ToList();
        return await BuildAsync(products, ct);
    }

    public async Task<IReadOnlyList<ProductDto>> GetAvailableProductsReportAsync(CancellationToken ct = default) // Отчёт по товарам в наличии
    {
        var products = (await _products.GetAllAsync(ct)).Where(p => p.IsActive).ToList();
        var dtos = await BuildAsync(products, ct);
        return dtos.Where(d => d.CurrentStock > 0).ToList();
    }

    public Task<IReadOnlyList<LowStockProductDto>> GetToReplenishReportAsync(CancellationToken ct = default) // Отчёт по товарам к пополнению
        => _stock.GetLowStockProductsAsync(ct);

    public async Task<IReadOnlyList<SupplierProductDto>> GetSupplierProductsReportAsync(Guid supplierId, CancellationToken ct = default) // Отчёт по товарам поставщика
    {
        var links = (await _supplierProducts.GetAllAsync(ct))
            .Where(l => l.SupplierId == supplierId && l.IsActive)
            .ToList();

        var products = (await _products.GetAllAsync(ct)).ToDictionary(p => p.Id, p => p.Name);
        var supplierName = (await _suppliers.GetByIdAsync(supplierId, ct))?.Name ?? "?";

        return links
            .Select(l => DtoMapper.ToDto(l, products.GetValueOrDefault(l.ProductId, "?"), supplierName))
            .OrderBy(x => x.ProductName, StringComparer.CurrentCultureIgnoreCase)
            .ToList();
    }

    private async Task<IReadOnlyList<ProductDto>> BuildAsync(List<Product> products, CancellationToken ct) // Построить DTO товаров для отчёта
    {
        if (products.Count == 0) return Array.Empty<ProductDto>();

        var stockMap = await _stock.GetTotalStocksAsync(ct);
        var shelfNames = (await _shelves.GetAllAsync(ct)).ToDictionary(s => s.Id, s => s.Name);
        var supplierCounts = (await _supplierProducts.GetAllAsync(ct))
            .Where(l => l.IsActive)
            .GroupBy(l => l.ProductId)
            .ToDictionary(g => g.Key, g => g.Count());

        return products
            .OrderBy(p => p.Name, StringComparer.CurrentCultureIgnoreCase)
            .Select(p => DtoMapper.ToDto(
                p,
                stockMap.GetValueOrDefault(p.Id),
                supplierCounts.GetValueOrDefault(p.Id),
                p.DefaultShelfId is { } sid ? shelfNames.GetValueOrDefault(sid) : null))
            .ToList();
    }
}