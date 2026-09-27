// Application/Mappers/DtoMapper.cs

using System;
using System.Collections.Generic;
using System.Linq;
using TradeSystem.Application.Dto;
using TradeSystem.Domain.Entities;
using TradeSystem.Domain.Enums;
using TradeSystem.Domain.ValueObjects;

// Файл содержит класс DtoMapper,
// который выполняет преобразование доменных сущностей торговой системы в DTO-объекты.

namespace TradeSystem.Application.Mappers;

public static class DtoMapper
{
    public static string Format(Address address) // Сформировать строковое представление адреса
    {
        if (address is null) return string.Empty;

        var parts = new List<string?>
        {
            address.Country, address.Region, address.City,
            string.IsNullOrWhiteSpace(address.Street) ? null : $"ул. {address.Street}",
            string.IsNullOrWhiteSpace(address.Building) ? null : $"д. {address.Building}",
            string.IsNullOrWhiteSpace(address.Apartment) ? null : $"кв. {address.Apartment}",
            string.IsNullOrWhiteSpace(address.PostalCode) ? null : address.PostalCode
        };

        return string.Join(", ", parts.Where(p => !string.IsNullOrWhiteSpace(p)));
    }

    public static SupplierDto ToDto(Supplier s, int productsCount)
    {
        var a = s.Address ?? new Address();
        return new SupplierDto
        {
            Id = s.Id,
            Name = s.Name,
            TaxId = s.TaxId,
            Country = a.Country,
            Region = a.Region,
            City = a.City,
            Street = a.Street,
            Building = a.Building,
            Apartment = a.Apartment,
            PostalCode = a.PostalCode,
            FullAddress = Format(a),
            Phone = s.Phone,
            Email = s.Email,
            IsActive = s.IsActive,
            Note = s.Note,
            ProductsCount = productsCount
        };
    }

    public static ShelfDto ToDto(Shelf sh, int productsCount) => new() // Преобразовать полку в DTO
    {
        Id = sh.Id,
        Code = sh.Code,
        Name = sh.Name,
        Location = sh.Location,
        IsActive = sh.IsActive,
        Note = sh.Note,
        ProductsCount = productsCount
    };

    public static ProductDto ToDto( // Преобразовать товар в DTO
        Product p,
        int currentStock,
        int suppliersCount,
        string? defaultShelfName)
    {
        var needs = p.MinimumStockQuantity > 0 && currentStock < p.MinimumStockQuantity;

        return new ProductDto
        {
            Id = p.Id,
            Name = p.Name,
            Article = p.Article,
            Unit = p.Unit,
            UnitLabel = p.Unit.Of(),
            MinimumStockQuantity = p.MinimumStockQuantity,
            TargetStockQuantity = p.TargetStockQuantity,
            IsActive = p.IsActive,
            Description = p.Description,
            DefaultShelfId = p.DefaultShelfId,
            DefaultShelfName = defaultShelfName,
            CurrentStock = currentStock,
            SuppliersCount = suppliersCount,
            NeedsReplenish = needs
        };
    }

    public static SupplierProductDto ToDto( // Преобразовать связь поставщика и товара в DTO
        SupplierProduct sp,
        string productName,
        string supplierName) => new()
    {
        Id = sp.Id,
        ProductId = sp.ProductId,
        ProductName = productName,
        SupplierId = sp.SupplierId,
        SupplierName = supplierName,
        SupplierArticle = sp.SupplierArticle,
        PurchasePrice = sp.PurchasePrice,
        LeadTimeDays = sp.LeadTimeDays,
        MinimumOrderQuantity = sp.MinimumOrderQuantity,
        OrderMultiple = sp.OrderMultiple,
        IsPreferred = sp.IsPreferred,
        IsActive = sp.IsActive,
        Note = sp.Note
    };

    public static PurchaseOrderLineDto ToDto(PurchaseOrderLine l) => new() // Преобразовать позицию заказа в DTO
    {
        Id = l.Id,
        ProductId = l.ProductId,
        ProductName = l.ProductName,
        UnitLabel = l.Unit.Of(),
        OrderedQuantity = l.OrderedQuantity,
        ReceivedQuantity = l.ReceivedQuantity,
        RemainingQuantity = Math.Max(0, l.OrderedQuantity - l.ReceivedQuantity),
        UnitPrice = l.UnitPrice,
        TotalPrice = l.TotalPrice,
        IsClosed = l.IsClosed
    };

    public static PurchaseOrderDto ToDto( // Преобразовать заказ на покупку в DTO
        PurchaseOrder o,
        string supplierName,
        PaymentStatus paymentStatus) => new()
    {
        Id = o.Id,
        Number = o.Number,
        SupplierId = o.SupplierId,
        SupplierName = supplierName,
        OrderDateUtc = o.OrderDateUtc,
        Status = o.Status,
        StatusLabel = o.Status.Of(),
        TotalAmount = o.TotalAmount,
        PaymentStatus = paymentStatus,
        PaymentStatusLabel = paymentStatus.Of(),
        Comment = o.Comment,
        Lines = o.Lines.Select(ToDto).ToList()
    };

    public static InvoiceDto ToDto(Invoice i, string? orderNumber) => new() // Преобразовать счёт на оплату в DTO
    {
        Id = i.Id,
        OrderId = i.OrderId,
        OrderNumber = orderNumber,
        Number = i.Number,
        IssueDateUtc = i.IssueDateUtc,
        DueDateUtc = i.DueDateUtc,
        Amount = i.Amount,
        PaymentStatus = i.PaymentStatus,
        PaymentStatusLabel = i.PaymentStatus.Of(),
        PaidAtUtc = i.PaidAtUtc,
        Comment = i.Comment
    };

    public static GoodsReceiptLineDto ToDto(GoodsReceiptLine l, string productName, string shelfName) => new() // Преобразовать позицию поступления в DTO
    {
        Id = l.Id,
        OrderLineId = l.OrderLineId,
        ProductId = l.ProductId,
        ProductName = productName,
        ShelfId = l.ShelfId,
        ShelfName = shelfName,
        Quantity = l.Quantity,
        UnitCost = l.UnitCost
    };

    public static GoodsReceiptDto ToDto( // Преобразовать поступление товара в DTO
        GoodsReceipt r,
        string supplierName,
        string? orderNumber,
        Func<GoodsReceiptLine, (string product, string shelf)> resolver) => new()
    {
        Id = r.Id,
        Number = r.Number,
        SupplierId = r.SupplierId,
        SupplierName = supplierName,
        OrderId = r.OrderId,
        OrderNumber = orderNumber,
        ReceivedAtUtc = r.ReceivedAtUtc,
        Status = r.Status,
        StatusLabel = r.Status.Of(),
        Comment = r.Comment,
        Lines = r.Lines.Select(l =>
        {
            var (product, shelf) = resolver(l);
            return ToDto(l, product, shelf);
        }).ToList()
    };

    public static SaleLineDto ToDto(SaleLine l, string productName, string shelfName) => new() // Преобразовать позицию продажи в DTO
    {
        Id = l.Id,
        ProductId = l.ProductId,
        ProductName = productName,
        ShelfId = l.ShelfId,
        ShelfName = shelfName,
        Quantity = l.Quantity,
        UnitPrice = l.UnitPrice,
        TotalPrice = l.TotalPrice
    };

    public static SaleDto ToDto(Sale s, Func<SaleLine, (string product, string shelf)> resolver) => new() // Преобразовать продажу в DTO
    {
        Id = s.Id,
        Number = s.Number,
        SoldAtUtc = s.SoldAtUtc,
        Status = s.Status,
        StatusLabel = s.Status.Of(),
        TotalAmount = s.Lines.Sum(l => l.TotalPrice),
        Comment = s.Comment,
        Lines = s.Lines.Select(l =>
        {
            var (product, shelf) = resolver(l);
            return ToDto(l, product, shelf);
        }).ToList()
    };
}