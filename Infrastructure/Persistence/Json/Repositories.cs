// Infrastructure/Persistence/Json/Repositories.cs

using TradeSystem.Application.Contracts.Repositories;
using TradeSystem.Domain.Entities;

// Файл содержит набор реализаций репозиториев,
// которые обеспечивают доступ к данным сущностей торговой системы через JSON-хранилище.

namespace TradeSystem.Infrastructure.Persistence.Json;

public sealed class SupplierRepository : JsonFileRepository<Supplier>, ISupplierRepository // Репозиторий поставщиков
{
    public SupplierRepository(IJsonFileStore<Supplier> store) : base(store)
    {
    }
}

public sealed class ProductRepository : JsonFileRepository<Product>, IProductRepository // Репозиторий товаров
{
    public ProductRepository(IJsonFileStore<Product> store) : base(store)
    {
    }
}

public sealed class ShelfRepository : JsonFileRepository<Shelf>, IShelfRepository // Репозиторий полок
{
    public ShelfRepository(IJsonFileStore<Shelf> store) : base(store)
    {
    }
}

public sealed class SupplierProductRepository : JsonFileRepository<SupplierProduct>, ISupplierProductRepository // Репозиторий связей поставщиков и товаров
{
    public SupplierProductRepository(IJsonFileStore<SupplierProduct> store) : base(store)
    {
    }
}

public sealed class StockBalanceRepository : JsonFileRepository<StockBalance>, IStockBalanceRepository // Репозиторий остатков товаров на полках
{
    public StockBalanceRepository(IJsonFileStore<StockBalance> store) : base(store)
    {
    }
}

public sealed class PurchaseOrderRepository : JsonFileRepository<PurchaseOrder>, IPurchaseOrderRepository // Репозиторий заказов на покупку
{
    public PurchaseOrderRepository(IJsonFileStore<PurchaseOrder> store) : base(store)
    {
    }
}

public sealed class InvoiceRepository : JsonFileRepository<Invoice>, IInvoiceRepository // Репозиторий счетов на оплату
{
    public InvoiceRepository(IJsonFileStore<Invoice> store) : base(store)
    {
    }
}

public sealed class GoodsReceiptRepository : JsonFileRepository<GoodsReceipt>, IGoodsReceiptRepository // Репозиторий поступлений товаров
{
    public GoodsReceiptRepository(IJsonFileStore<GoodsReceipt> store) : base(store)
    {
    }
}

public sealed class SaleRepository : JsonFileRepository<Sale>, ISaleRepository // Репозиторий продаж
{
    public SaleRepository(IJsonFileStore<Sale> store) : base(store)
    {
    }
}

public sealed class StockMovementRepository : JsonFileRepository<StockMovement>, IStockMovementRepository // Репозиторий движений товаров
{
    public StockMovementRepository(IJsonFileStore<StockMovement> store) : base(store)
    {
    }
}