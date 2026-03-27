using Microsoft.EntityFrameworkCore;
using Oms.Persistence.Entities;

namespace Oms.Persistence;

public sealed class OmsDbContext : DbContext
{
    public OmsDbContext(DbContextOptions<OmsDbContext> options) : base(options)
    {
    }

    public DbSet<WarehouseEntity> Warehouses => Set<WarehouseEntity>();
    public DbSet<OrderEntity> Orders => Set<OrderEntity>();
    public DbSet<OrderItemEntity> OrderItems => Set<OrderItemEntity>();
    public DbSet<OrderLogEntity> OrderLogs => Set<OrderLogEntity>();
    public DbSet<InventoryEntity> Inventory => Set<InventoryEntity>();
    public DbSet<ShipmentEntity> Shipments => Set<ShipmentEntity>();
    public DbSet<ShipmentEventEntity> ShipmentEvents => Set<ShipmentEventEntity>();
    public DbSet<ExternalWebhookEventEntity> ExternalWebhookEvents => Set<ExternalWebhookEventEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var warehouses = modelBuilder.Entity<WarehouseEntity>();
        warehouses.ToTable("Warehouses");
        warehouses.HasKey(entity => entity.WarehouseId);
        warehouses.Property(entity => entity.Name).HasMaxLength(120).IsRequired();
        warehouses.Property(entity => entity.City).HasMaxLength(100).IsRequired();
        warehouses.Property(entity => entity.State).HasMaxLength(100).IsRequired();
        warehouses.Property(entity => entity.PostalCode).HasMaxLength(20).IsRequired();
        warehouses.Property(entity => entity.Latitude).HasColumnType("decimal(9,6)");
        warehouses.Property(entity => entity.Longitude).HasColumnType("decimal(9,6)");
        warehouses.Property(entity => entity.CreatedAt).HasColumnType("datetime2");

        var orders = modelBuilder.Entity<OrderEntity>();
        orders.ToTable("Orders", table =>
        {
            table.HasCheckConstraint("CK_Orders_Status", "Status IN ('Pending', 'Preparing', 'Shipped', 'Delivered', 'Cancelled')");
            table.HasCheckConstraint("CK_Orders_Origin", "Origin IN ('Marketplace', 'Web', 'Manual')");
        });
        orders.HasKey(entity => entity.OrderId);
        orders.Property(entity => entity.Customer).HasMaxLength(160).IsRequired();
        orders.Property(entity => entity.Status).HasMaxLength(30).IsRequired();
        orders.Property(entity => entity.Origin).HasMaxLength(30).IsRequired();
        orders.Property(entity => entity.Total).HasColumnType("decimal(18,2)");
        orders.Property(entity => entity.DestinationCity).HasMaxLength(100).IsRequired();
        orders.Property(entity => entity.DestinationState).HasMaxLength(100).IsRequired();
        orders.Property(entity => entity.DestinationPostalCode).HasMaxLength(20).IsRequired();
        orders.Property(entity => entity.DestinationLatitude).HasColumnType("decimal(9,6)");
        orders.Property(entity => entity.DestinationLongitude).HasColumnType("decimal(9,6)");
        orders.Property(entity => entity.ShipmentTrackingNumber).HasMaxLength(80);
        orders.Property(entity => entity.CreatedAt).HasColumnType("datetime2");
        orders.Property(entity => entity.UpdatedAt).HasColumnType("datetime2");
        orders.HasIndex(entity => new { entity.Status, entity.CreatedAt }).HasDatabaseName("IX_Orders_Status_CreatedAt");
        orders.HasIndex(entity => new { entity.Origin, entity.CreatedAt }).HasDatabaseName("IX_Orders_Origin_CreatedAt");
        orders.HasOne(entity => entity.AssignedWarehouse)
            .WithMany(entity => entity.Orders)
            .HasForeignKey(entity => entity.AssignedWarehouseId)
            .OnDelete(DeleteBehavior.Restrict);

        var orderItems = modelBuilder.Entity<OrderItemEntity>();
        orderItems.ToTable("OrderItems", table =>
        {
            table.HasCheckConstraint("CK_OrderItems_Quantity", "Quantity > 0");
            table.HasCheckConstraint("CK_OrderItems_UnitPrice", "UnitPrice > 0");
        });
        orderItems.HasKey(entity => entity.OrderItemId);
        orderItems.Property(entity => entity.Sku).HasMaxLength(80).IsRequired();
        orderItems.Property(entity => entity.UnitPrice).HasColumnType("decimal(18,2)");
        orderItems.HasIndex(entity => entity.OrderId).HasDatabaseName("IX_OrderItems_OrderId");
        orderItems.HasOne(entity => entity.Order)
            .WithMany(entity => entity.Items)
            .HasForeignKey(entity => entity.OrderId);

        var orderLogs = modelBuilder.Entity<OrderLogEntity>();
        orderLogs.ToTable("OrderLogs");
        orderLogs.HasKey(entity => entity.OrderLogId);
        orderLogs.Property(entity => entity.EventName).HasMaxLength(80).IsRequired();
        orderLogs.Property(entity => entity.Details).HasMaxLength(500).IsRequired();
        orderLogs.Property(entity => entity.EventTimestamp).HasColumnType("datetime2");
        orderLogs.HasIndex(entity => new { entity.OrderId, entity.EventTimestamp }).HasDatabaseName("IX_OrderLogs_OrderId_EventTimestamp");
        orderLogs.HasOne(entity => entity.Order)
            .WithMany(entity => entity.Logs)
            .HasForeignKey(entity => entity.OrderId);

        var inventory = modelBuilder.Entity<InventoryEntity>();
        inventory.ToTable("Inventory", table =>
        {
            table.HasCheckConstraint("CK_Inventory_PhysicalStock", "PhysicalStock >= 0");
            table.HasCheckConstraint("CK_Inventory_ReservedStock", "ReservedStock >= 0");
            table.HasCheckConstraint("CK_Inventory_ReservedVsPhysical", "ReservedStock <= PhysicalStock");
        });
        inventory.HasKey(entity => entity.InventoryId);
        inventory.Property(entity => entity.Sku).HasMaxLength(80).IsRequired();
        inventory.Property(entity => entity.LocationCode).HasMaxLength(60).IsRequired();
        inventory.Property(entity => entity.AvailableStock)
            .HasComputedColumnSql("[PhysicalStock] - [ReservedStock]", stored: true);
        inventory.Property(entity => entity.RowVersion).IsRowVersion().IsConcurrencyToken();
        inventory.HasIndex(entity => new { entity.Sku, entity.WarehouseId }).HasDatabaseName("IX_Inventory_Sku_WarehouseId");
        inventory.HasIndex(entity => new { entity.WarehouseId, entity.Sku }).IsUnique().HasDatabaseName("UQ_Inventory_WarehouseSku");
        inventory.HasOne(entity => entity.Warehouse)
            .WithMany(entity => entity.InventoryItems)
            .HasForeignKey(entity => entity.WarehouseId);

        var shipments = modelBuilder.Entity<ShipmentEntity>();
        shipments.ToTable("Shipments");
        shipments.HasKey(entity => entity.ShipmentId);
        shipments.Property(entity => entity.Carrier).HasMaxLength(100).IsRequired();
        shipments.Property(entity => entity.TrackingNumber).HasMaxLength(80).IsRequired();
        shipments.Property(entity => entity.Status).HasMaxLength(40).IsRequired();
        shipments.Property(entity => entity.WeightKg).HasColumnType("decimal(12,3)");
        shipments.Property(entity => entity.HeightCm).HasColumnType("decimal(12,2)");
        shipments.Property(entity => entity.WidthCm).HasColumnType("decimal(12,2)");
        shipments.Property(entity => entity.LengthCm).HasColumnType("decimal(12,2)");
        shipments.Property(entity => entity.ShippingCost).HasColumnType("decimal(18,2)");
        shipments.Property(entity => entity.DestinationAddress).HasMaxLength(250).IsRequired();
        shipments.Property(entity => entity.CreatedAt).HasColumnType("datetime2");
        shipments.Property(entity => entity.UpdatedAt).HasColumnType("datetime2");
        shipments.HasIndex(entity => entity.OrderId).HasDatabaseName("IX_Shipments_OrderId");
        shipments.HasIndex(entity => entity.TrackingNumber).IsUnique().HasDatabaseName("UQ_Shipments_Tracking");
        shipments.HasOne(entity => entity.Order)
            .WithMany(entity => entity.Shipments)
            .HasForeignKey(entity => entity.OrderId);

        var shipmentEvents = modelBuilder.Entity<ShipmentEventEntity>();
        shipmentEvents.ToTable("ShipmentEvents");
        shipmentEvents.HasKey(entity => entity.ShipmentEventId);
        shipmentEvents.Property(entity => entity.Status).HasMaxLength(40).IsRequired();
        shipmentEvents.Property(entity => entity.Notes).HasMaxLength(500).IsRequired();
        shipmentEvents.Property(entity => entity.EventTimestamp).HasColumnType("datetime2");
        shipmentEvents.HasIndex(entity => new { entity.ShipmentId, entity.EventTimestamp }).HasDatabaseName("IX_ShipmentEvents_ShipmentId_EventTimestamp");
        shipmentEvents.HasOne(entity => entity.Shipment)
            .WithMany(entity => entity.Events)
            .HasForeignKey(entity => entity.ShipmentId);

        var externalWebhookEvents = modelBuilder.Entity<ExternalWebhookEventEntity>();
        externalWebhookEvents.ToTable("ExternalWebhookEvents");
        externalWebhookEvents.HasKey(entity => entity.ExternalWebhookEventId);
        externalWebhookEvents.Property(entity => entity.Provider).HasMaxLength(40).IsRequired();
        externalWebhookEvents.Property(entity => entity.ExternalOrderId).HasMaxLength(100).IsRequired();
        externalWebhookEvents.Property(entity => entity.Payload).HasColumnType("nvarchar(max)").IsRequired();
        externalWebhookEvents.Property(entity => entity.CanonicalSnapshot).HasColumnType("nvarchar(max)").IsRequired();
        externalWebhookEvents.Property(entity => entity.ReceivedAt).HasColumnType("datetime2");
        externalWebhookEvents.Property(entity => entity.ProcessedAt).HasColumnType("datetime2");
        externalWebhookEvents.HasIndex(entity => new { entity.Provider, entity.ExternalOrderId }).IsUnique().HasDatabaseName("UQ_ExternalWebhookEvents");
    }
}
