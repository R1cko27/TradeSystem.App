// Application/Contracts/Repositories/Repositories.cs

using TradeSystem.Domain.Entities;

// Файл содержит набор интерфейсов репозиториев,
// которые определяют контракты доступа к данным сущностей торговой системы.

namespace TradeSystem.Application.Contracts.Repositories;

public interface ISupplierRepository : IRepository<Supplier> // Репозиторий поставщиков
{
}

public interface IProductRepository : IRepository<Product> // Репозиторий товаров
{
}

public interface IShelfRepository : IRepository<Shelf> // Репозиторий полок
{
}

public interface ISupplierProductRepository : IRepository<SupplierProduct> // Репозиторий связей поставщиков и товаров
{
}

public interface IStockBalanceRepository : IRepository<StockBalance> // Репозиторий остатков товаров на полках
{
}

public interface IPurchaseOrderRepository : IRepository<PurchaseOrder> // Репозиторий заказов на покупку
{
}

public interface IInvoiceRepository : IRepository<Invoice> // Репозиторий счетов на оплату
{
}

public interface IGoodsReceiptRepository : IRepository<GoodsReceipt> // Репозиторий поступлений товаров
{
}

public interface ISaleRepository : IRepository<Sale> // Репозиторий продаж
{
}

public interface IStockMovementRepository : IRepository<StockMovement> // Репозиторий движений товаров
{
}