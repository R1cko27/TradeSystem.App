// Infrastructure/Persistence/Json/JsonFileNames.cs

// Файл содержит класс JsonFileNames,
// который хранит имена JSON-файлов, используемых для хранения данных торговой системы.

namespace TradeSystem.Infrastructure.Persistence.Json;

public static class JsonFileNames
{
    public const string Suppliers = "suppliers.json"; // Имя файла с поставщиками
    public const string Products = "products.json"; // Имя файла с товарами
    public const string Shelves = "shelves.json"; // Имя файла с полками
    public const string SupplierProducts = "supplier-products.json"; // Имя файла со связями поставщиков и товаров
    public const string StockBalances = "stock-balances.json"; // Имя файла с остатками товаров на полках
    public const string PurchaseOrders = "purchase-orders.json"; // Имя файла с заказами на покупку
    public const string Invoices = "invoices.json"; // Имя файла со счетами на оплату
    public const string GoodsReceipts = "goods-receipts.json"; // Имя файла с поступлениями товаров
    public const string Sales = "sales.json"; // Имя файла с продажами
    public const string StockMovements = "stock-movements.json"; // Имя файла с движениями товаров
}