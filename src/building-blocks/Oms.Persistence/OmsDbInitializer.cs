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

        await EnsureShipmentPricingSchemaAsync(dbContext, cancellationToken);
        await EnsureCarrierSchemaAsync(dbContext, cancellationToken);

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
                CarrierId = Guid.Parse("6c1a2f12-0c19-4f23-9fb2-000000000001"),
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

        if (!await dbContext.Carriers.AnyAsync(cancellationToken))
        {
            var carriers = new[]
            {
                new CarrierEntity
                {
                    CarrierId = Guid.Parse("6c1a2f12-0c19-4f23-9fb2-000000000001"),
                    Code = "ANDREANI",
                    Name = "Andreani",
                    ServiceLevel = "Standard",
                    TrackingUrlTemplate = "https://www.andreani.com/#!/informacionEnvio/{trackingNumber}",
                    SupportEmail = "soporte@andreani.com",
                    SupportPhone = "+54 800 122 1111",
                    InsuranceSupported = true,
                    IsActive = true,
                    Notes = "Carrier principal para envíos nacionales.",
                    UpdatedAt = DateTime.UtcNow
                },
                new CarrierEntity
                {
                    CarrierId = Guid.Parse("6c1a2f12-0c19-4f23-9fb2-000000000002"),
                    Code = "OCA",
                    Name = "OCA",
                    ServiceLevel = "Express",
                    TrackingUrlTemplate = "https://www.oca.com.ar/Busquedas/Envios?numero={trackingNumber}",
                    SupportEmail = "clientes@oca.com.ar",
                    SupportPhone = "+54 800 999 7700",
                    InsuranceSupported = true,
                    IsActive = true,
                    Notes = "Opcional para entregas urbanas y express.",
                    UpdatedAt = DateTime.UtcNow
                },
                new CarrierEntity
                {
                    CarrierId = Guid.Parse("6c1a2f12-0c19-4f23-9fb2-000000000003"),
                    Code = "CORREOAR",
                    Name = "Correo Argentino",
                    ServiceLevel = "Economy",
                    TrackingUrlTemplate = "https://www.correoargentino.com.ar/formularios/e-commerce?id={trackingNumber}",
                    SupportEmail = "empresas@correoargentino.com.ar",
                    SupportPhone = "+54 11 4891 9191",
                    InsuranceSupported = false,
                    IsActive = true,
                    Notes = "Cobertura nacional con costo contenido.",
                    UpdatedAt = DateTime.UtcNow
                }
            };

            await dbContext.Carriers.AddRangeAsync(carriers, cancellationToken);
        }

        if (!await dbContext.ShipmentPricingSettings.AnyAsync(cancellationToken))
        {
            var settings = new ShipmentPricingSettingsEntity
            {
                ShipmentPricingSettingsId = 1,
                DefaultBaseCost = 14.50m,
                InsuranceFlatCost = 3.25m,
                UpdatedAt = DateTime.UtcNow,
                Rules =
                {
                    new ShipmentPricingRuleEntity
                    {
                        ShipmentPricingRuleId = Guid.NewGuid(),
                        RuleName = "AMBA",
                        PostalCodePrefix = "1",
                        BaseCost = 9.50m,
                        UpdatedAt = DateTime.UtcNow
                    },
                    new ShipmentPricingRuleEntity
                    {
                        ShipmentPricingRuleId = Guid.NewGuid(),
                        RuleName = "Centro",
                        PostalCodePrefix = "5",
                        BaseCost = 15.75m,
                        UpdatedAt = DateTime.UtcNow
                    },
                    new ShipmentPricingRuleEntity
                    {
                        ShipmentPricingRuleId = Guid.NewGuid(),
                        RuleName = "Litoral",
                        PostalCodePrefix = "2",
                        BaseCost = 13.20m,
                        UpdatedAt = DateTime.UtcNow
                    }
                }
            };

            await dbContext.ShipmentPricingSettings.AddAsync(settings, cancellationToken);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static async Task EnsureShipmentPricingSchemaAsync(OmsDbContext dbContext, CancellationToken cancellationToken)
    {
        const string createSettingsTableSql = """
IF OBJECT_ID(N'[dbo].[ShipmentPricingSettings]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[ShipmentPricingSettings]
    (
        [ShipmentPricingSettingsId] INT NOT NULL PRIMARY KEY,
        [DefaultBaseCost] DECIMAL(18,2) NOT NULL,
        [InsuranceFlatCost] DECIMAL(18,2) NOT NULL,
        [UpdatedAt] DATETIME2 NOT NULL
    );
END
""";

        const string createRulesTableSql = """
IF OBJECT_ID(N'[dbo].[ShipmentPricingRules]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[ShipmentPricingRules]
    (
        [ShipmentPricingRuleId] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        [ShipmentPricingSettingsId] INT NOT NULL,
        [RuleName] NVARCHAR(120) NOT NULL,
        [PostalCodePrefix] NVARCHAR(12) NOT NULL,
        [BaseCost] DECIMAL(18,2) NOT NULL,
        [UpdatedAt] DATETIME2 NOT NULL,
        CONSTRAINT [CK_ShipmentPricingRules_BaseCost] CHECK ([BaseCost] >= 0),
        CONSTRAINT [FK_ShipmentPricingRules_ShipmentPricingSettings] FOREIGN KEY ([ShipmentPricingSettingsId]) REFERENCES [dbo].[ShipmentPricingSettings]([ShipmentPricingSettingsId])
    );

    CREATE UNIQUE INDEX [UQ_ShipmentPricingRules_SettingsPrefix]
        ON [dbo].[ShipmentPricingRules]([ShipmentPricingSettingsId], [PostalCodePrefix]);
END
""";

        await dbContext.Database.ExecuteSqlRawAsync(createSettingsTableSql, cancellationToken);
        await dbContext.Database.ExecuteSqlRawAsync(createRulesTableSql, cancellationToken);
    }

    private static async Task EnsureCarrierSchemaAsync(OmsDbContext dbContext, CancellationToken cancellationToken)
    {
        const string createCarriersTableSql = """
IF OBJECT_ID(N'[dbo].[Carriers]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[Carriers]
    (
        [CarrierId] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        [Code] NVARCHAR(30) NOT NULL,
        [Name] NVARCHAR(120) NOT NULL,
        [ServiceLevel] NVARCHAR(80) NOT NULL,
        [TrackingUrlTemplate] NVARCHAR(250) NOT NULL,
        [SupportEmail] NVARCHAR(160) NOT NULL CONSTRAINT [DF_Carriers_SupportEmail] DEFAULT N'',
        [SupportPhone] NVARCHAR(40) NOT NULL CONSTRAINT [DF_Carriers_SupportPhone] DEFAULT N'',
        [InsuranceSupported] BIT NOT NULL CONSTRAINT [DF_Carriers_InsuranceSupported] DEFAULT 0,
        [IsActive] BIT NOT NULL CONSTRAINT [DF_Carriers_IsActive] DEFAULT 1,
        [Notes] NVARCHAR(500) NOT NULL CONSTRAINT [DF_Carriers_Notes] DEFAULT N'',
        [UpdatedAt] DATETIME2 NOT NULL,
        CONSTRAINT [UQ_Carriers_Code] UNIQUE ([Code])
    );

    CREATE INDEX [IX_Carriers_IsActive_Name]
        ON [dbo].[Carriers]([IsActive], [Name]);
END
""";

        const string alterShipmentsTableSql = """
IF COL_LENGTH(N'[dbo].[Shipments]', N'CarrierId') IS NULL
BEGIN
    ALTER TABLE [dbo].[Shipments] ADD [CarrierId] UNIQUEIDENTIFIER NULL;

    ALTER TABLE [dbo].[Shipments]
    ADD CONSTRAINT [FK_Shipments_Carriers]
        FOREIGN KEY ([CarrierId]) REFERENCES [dbo].[Carriers]([CarrierId]);

    CREATE INDEX [IX_Shipments_CarrierId] ON [dbo].[Shipments]([CarrierId]);
END
""";

        await dbContext.Database.ExecuteSqlRawAsync(createCarriersTableSql, cancellationToken);
        await dbContext.Database.ExecuteSqlRawAsync(alterShipmentsTableSql, cancellationToken);
    }
}
