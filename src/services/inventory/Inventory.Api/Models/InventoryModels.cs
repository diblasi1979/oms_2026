namespace Inventory.Api.Models;

public sealed class InventoryItemResponse
{
    public string Sku { get; init; } = string.Empty;
    public Guid WarehouseId { get; init; }
    public string WarehouseName { get; init; } = string.Empty;
    public int PhysicalStock { get; init; }
    public int ReservedStock { get; init; }
    public int AvailableStock { get; init; }
    public string LocationCode { get; init; } = string.Empty;
    public string ConcurrencyToken { get; init; } = string.Empty;
}

public sealed class ReserveStockRequest
{
    public Guid WarehouseId { get; set; }
    public List<ReserveStockLine> Items { get; set; } = new();
}

public sealed class ReserveStockLine
{
    public string Sku { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public string ConcurrencyToken { get; set; } = string.Empty;
}

public sealed class ReleaseStockRequest
{
    public Guid WarehouseId { get; set; }
    public List<ReserveStockLine> Items { get; set; } = new();
}

