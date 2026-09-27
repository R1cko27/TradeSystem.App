// Application/Contracts/Services/ICatalogService.cs

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TradeSystem.Application.Dto;

// Файл содержит интерфейс ICatalogService,
// который определяет контракт сервиса управления справочными данными торговой системы.

namespace TradeSystem.Application.Contracts.Services;

public interface ICatalogService
{
    // Поставщики
    Task<IReadOnlyList<SupplierDto>> GetSuppliersAsync(bool includeInactive = false, CancellationToken ct = default); // Получить список поставщиков
    Task<SupplierDto?> GetSupplierAsync(Guid id, CancellationToken ct = default); // Получить поставщика по идентификатору
    Task<SupplierDto> CreateSupplierAsync(SupplierInput input, CancellationToken ct = default); // Создать поставщика
    Task<SupplierDto> UpdateSupplierAsync(SupplierInput input, CancellationToken ct = default); // Обновить поставщика
    Task DeleteSupplierAsync(Guid id, CancellationToken ct = default); // Удалить поставщика

    // Товары
    Task<IReadOnlyList<ProductDto>> GetProductsAsync(bool includeInactive = false, CancellationToken ct = default); // Получить список товаров
    Task<ProductDto?> GetProductAsync(Guid id, CancellationToken ct = default); // Получить товар по идентификатору
    Task<ProductDto> CreateProductAsync(ProductInput input, CancellationToken ct = default); // Создать товар
    Task<ProductDto> UpdateProductAsync(ProductInput input, CancellationToken ct = default); // Обновить товар
    Task DeleteProductAsync(Guid id, CancellationToken ct = default); // Удалить товар

    // Полки
    Task<IReadOnlyList<ShelfDto>> GetShelvesAsync(bool includeInactive = false, CancellationToken ct = default); // Получить список полок
    Task<ShelfDto?> GetShelfAsync(Guid id, CancellationToken ct = default); // Получить полку по идентификатору

    Task<ShelfDto> CreateShelfAsync(ShelfInput input, CancellationToken ct = default); // Создать полку
    Task<ShelfDto> UpdateShelfAsync(ShelfInput input, CancellationToken ct = default); // Обновить полку
    Task DeleteShelfAsync(Guid id, CancellationToken ct = default); // Удалить полку

    // Связи товар-поставщик
    Task<IReadOnlyList<SupplierProductDto>> GetSupplierProductsAsync(CancellationToken ct = default); // Получить все связи товаров и поставщиков
    Task<IReadOnlyList<SupplierProductDto>> GetSupplierProductsBySupplierAsync(Guid supplierId, CancellationToken ct = default); // Получить связи по поставщику
    Task<IReadOnlyList<SupplierProductDto>> GetSupplierProductsByProductAsync(Guid productId, CancellationToken ct = default); // Получить связи по товару
    Task<SupplierProductDto> LinkProductToSupplierAsync(SupplierProductInput input, CancellationToken ct = default); // Связать товар с поставщиком
    Task<SupplierProductDto> UpdateLinkAsync(SupplierProductInput input, CancellationToken ct = default); // Обновить связь товара и поставщика
    Task UnlinkAsync(Guid id, CancellationToken ct = default); // Удалить связь товара и поставщика
}