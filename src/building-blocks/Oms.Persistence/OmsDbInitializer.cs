using Microsoft.EntityFrameworkCore;
using Oms.Persistence.Entities;

namespace Oms.Persistence;

public static class OmsDbInitializer
{
    public static async Task InitializeAsync(OmsDbContext dbContext, CancellationToken cancellationToken = default)
    {
        try
        {
            await dbContext.Database.EnsureCreatedAsync(cancellationToken);
        }
        catch (Exception exception) when (exception.Message.Contains("CREATE DATABASE permission denied", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "El usuario configurado no puede crear la base configurada. Crea primero la base ejecutando database/schema/00-create-database.sql con un usuario administrador y luego reinicia el servicio.",
                exception);
        }

        if (!await dbContext.Warehouses.AnyAsync(cancellationToken))
        {
            var warehouses = new[]
            {
                new WarehouseEntity { WarehouseId = Guid.Parse("f5e10d20-8e72-4a9a-8c24-5b8a6b001001"), Name = "Buenos Aires Hub", City = "Buenos Aires", State = "Buenos Aires", PostalCode = "1000", Latitude = -34.603700m, Longitude = -58.381600m, CreatedAt = DateTime.UtcNow },
                new WarehouseEntity { WarehouseId = Guid.Parse("f5e10d20-8e72-4a9a-8c24-5b8a6b001002"), Name = "Cordoba Fulfillment", City = "Cordoba", State = "Cordoba", PostalCode = "5000", Latitude = -31.420100m, Longitude = -64.188800m, CreatedAt = DateTime.UtcNow },
                new WarehouseEntity { WarehouseId = Guid.Parse("f5e10d20-8e72-4a9a-8c24-5b8a6b001003"), Name = "Rosario Cross Dock", City = "Rosario", State = "Santa Fe", PostalCode = "2000", Latitude = -32.944200m, Longitude = -60.650500m, CreatedAt = DateTime.UtcNow }
            };

            await dbContext.Warehouses.AddRangeAsync(warehouses, cancellationToken);
        }

        if (!await dbContext.Inventory.AnyAsync(cancellationToken))
        {
            var inventory = new[]
            {
                new InventoryEntity { InventoryId = Guid.NewGuid(), Sku = "SKU-CHAIR-001", WarehouseId = Guid.Parse("f5e10d20-8e72-4a9a-8c24-5b8a6b001001"), PhysicalStock = 120, ReservedStock = 12, LocationCode = "BA-01-A-04" },
                new InventoryEntity { InventoryId = Guid.NewGuid(), Sku = "SKU-DESK-002", WarehouseId = Guid.Parse("f5e10d20-8e72-4a9a-8c24-5b8a6b001001"), PhysicalStock = 80, ReservedStock = 9, LocationCode = "BA-02-C-02" },
                new InventoryEntity { InventoryId = Guid.NewGuid(), Sku = "SKU-LABEL-003", WarehouseId = Guid.Parse("f5e10d20-8e72-4a9a-8c24-5b8a6b001001"), PhysicalStock = 200, ReservedStock = 20, LocationCode = "BA-05-B-01" },
                new InventoryEntity { InventoryId = Guid.NewGuid(), Sku = "SKU-CHAIR-001", WarehouseId = Guid.Parse("f5e10d20-8e72-4a9a-8c24-5b8a6b001002"), PhysicalStock = 60, ReservedStock = 10, LocationCode = "CB-03-A-02" },
                new InventoryEntity { InventoryId = Guid.NewGuid(), Sku = "SKU-DESK-002", WarehouseId = Guid.Parse("f5e10d20-8e72-4a9a-8c24-5b8a6b001002"), PhysicalStock = 140, ReservedStock = 18, LocationCode = "CB-01-D-08" },
                new InventoryEntity { InventoryId = Guid.NewGuid(), Sku = "SKU-LABEL-003", WarehouseId = Guid.Parse("f5e10d20-8e72-4a9a-8c24-5b8a6b001002"), PhysicalStock = 150, ReservedStock = 30, LocationCode = "CB-07-F-01" },
                new InventoryEntity { InventoryId = Guid.NewGuid(), Sku = "SKU-CHAIR-001", WarehouseId = Guid.Parse("f5e10d20-8e72-4a9a-8c24-5b8a6b001003"), PhysicalStock = 40, ReservedStock = 2, LocationCode = "RS-02-A-01" },
                new InventoryEntity { InventoryId = Guid.NewGuid(), Sku = "SKU-DESK-002", WarehouseId = Guid.Parse("f5e10d20-8e72-4a9a-8c24-5b8a6b001003"), PhysicalStock = 90, ReservedStock = 11, LocationCode = "RS-03-D-03" },
                new InventoryEntity { InventoryId = Guid.NewGuid(), Sku = "SKU-LABEL-003", WarehouseId = Guid.Parse("f5e10d20-8e72-4a9a-8c24-5b8a6b001003"), PhysicalStock = 180, ReservedStock = 25, LocationCode = "RS-05-C-07" }
            };

            await dbContext.Inventory.AddRangeAsync(inventory, cancellationToken);
        }

        if (!await dbContext.Orders.AnyAsync(cancellationToken))
        {
            var orderId = Guid.Parse("c71ab4d9-09a9-4dc0-bf89-e7614ed4b801");
            var shipmentId = Guid.Parse("d91ab4d9-09a9-4dc0-bf89-e7614ed4b802");
            var createdAt = DateTime.UtcNow.AddHours(-6);
            var tracking = "TRK-20260327-48291";

            var order = new OrderEntity
            {
                OrderId = orderId,
                Customer = "Distribuidora Norte",
                Status = "Preparing",
                Origin = "Web",
                Total = 683.60m,
                DestinationCity = "Mendoza",
                DestinationState = "Mendoza",
                DestinationPostalCode = "5500",
                DestinationLatitude = -32.889500m,
                DestinationLongitude = -68.845800m,
                AssignedWarehouseId = Guid.Parse("f5e10d20-8e72-4a9a-8c24-5b8a6b001002"),
                ShipmentTrackingNumber = tracking,
                CreatedAt = createdAt,
                UpdatedAt = createdAt.AddMinutes(20)
            };

            order.Items.Add(new OrderItemEntity { OrderItemId = Guid.NewGuid(), OrderId = orderId, Sku = "SKU-CHAIR-001", Quantity = 4, UnitPrice = 149.90m });
            order.Items.Add(new OrderItemEntity { OrderItemId = Guid.NewGuid(), OrderId = orderId, Sku = "SKU-LABEL-003", Quantity = 20, UnitPrice = 4.20m });
            order.Logs.Add(new OrderLogEntity { OrderLogId = Guid.NewGuid(), OrderId = orderId, EventName = "OrderCreated", Details = "Orden creada desde Web.", EventTimestamp = createdAt });
            order.Logs.Add(new OrderLogEntity { OrderLogId = Guid.NewGuid(), OrderId = orderId, EventName = "StockReserved", Details = "Stock reservado para evitar overselling.", EventTimestamp = createdAt.AddMinutes(5) });
            order.Logs.Add(new OrderLogEntity { OrderLogId = Guid.NewGuid(), OrderId = orderId, EventName = "WarehouseAssigned", Details = "Asignado a Cordoba Fulfillment por cercania y disponibilidad.", EventTimestamp = createdAt.AddMinutes(6) });
            order.Logs.Add(new OrderLogEntity { OrderLogId = Guid.NewGuid(), OrderId = orderId, EventName = "OrderStatusChanged", Details = "Estado actualizado a Preparing.", EventTimestamp = createdAt.AddMinutes(20) });

            var shipment = new ShipmentEntity
            {
                ShipmentId = shipmentId,
                OrderId = orderId,
                Carrier = "Andreani",
                TrackingNumber = tracking,
                Status = "InTransit",
                WeightKg = 8.400m,
                HeightCm = 45m,
                WidthCm = 40m,
                LengthCm = 55m,
                ShippingCost = 18.50m,
                DestinationAddress = "Av. San Martin 123, Mendoza",
                CreatedAt = createdAt.AddMinutes(30),
                UpdatedAt = createdAt.AddHours(1)
            };

            shipment.Events.Add(new ShipmentEventEntity { ShipmentEventId = Guid.NewGuid(), ShipmentId = shipmentId, Status = "LabelCreated", Notes = "Etiqueta simulada generada en OMS.", EventTimestamp = createdAt.AddMinutes(30) });
            shipment.Events.Add(new ShipmentEventEntity { ShipmentEventId = Guid.NewGuid(), ShipmentId = shipmentId, Status = "InTransit", Notes = "Unidad despachada desde hub regional.", EventTimestamp = createdAt.AddHours(1) });

            await dbContext.Orders.AddAsync(order, cancellationToken);
            await dbContext.Shipments.AddAsync(shipment, cancellationToken);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
