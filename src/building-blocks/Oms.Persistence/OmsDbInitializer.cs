using Microsoft.EntityFrameworkCore;
using Oms.Persistence.Entities;

namespace Oms.Persistence;

public static class OmsDbInitializer
{
    private static readonly Guid StandardCustomerTypeId = Guid.Parse("7f3fbc62-b77f-4d2e-9c4c-000000000001");
    private static readonly Guid PremiumCustomerTypeId = Guid.Parse("7f3fbc62-b77f-4d2e-9c4c-000000000002");
    private static readonly Guid WholesaleCustomerTypeId = Guid.Parse("7f3fbc62-b77f-4d2e-9c4c-000000000003");

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

        await EnsureCustomerTypeSchemaAsync(dbContext, cancellationToken);
        await EnsureCarrierSchemaAsync(dbContext, cancellationToken);
        await EnsureShipmentPricingSchemaAsync(dbContext, cancellationToken);
        await EnsureCustomerTypeSeedDataAsync(dbContext, cancellationToken);
        await EnsureCustomerTypeRelationshipsAsync(dbContext, cancellationToken);

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
                CustomerTypeId = StandardCustomerTypeId,
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
                CustomerTypeId = StandardCustomerTypeId,
                RecipientName = "Marcela Gomez",
                RecipientPhone = "+54 261 555 0101",
                RecipientEmail = "marcela.gomez@example.com",
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
                        RuleName = "STANDARD AMBA Andreani",
                        CustomerTypeId = StandardCustomerTypeId,
                        PostalCodePrefix = "1",
                        CarrierId = Guid.Parse("6c1a2f12-0c19-4f23-9fb2-000000000001"),
                        BaseCost = 9.50m,
                        UpdatedAt = DateTime.UtcNow
                    },
                    new ShipmentPricingRuleEntity
                    {
                        ShipmentPricingRuleId = Guid.NewGuid(),
                        RuleName = "STANDARD Centro Andreani",
                        CustomerTypeId = StandardCustomerTypeId,
                        PostalCodePrefix = "5",
                        CarrierId = Guid.Parse("6c1a2f12-0c19-4f23-9fb2-000000000001"),
                        BaseCost = 15.75m,
                        UpdatedAt = DateTime.UtcNow
                    },
                    new ShipmentPricingRuleEntity
                    {
                        ShipmentPricingRuleId = Guid.NewGuid(),
                        RuleName = "STANDARD Litoral Andreani",
                        CustomerTypeId = StandardCustomerTypeId,
                        PostalCodePrefix = "2",
                        CarrierId = Guid.Parse("6c1a2f12-0c19-4f23-9fb2-000000000001"),
                        BaseCost = 13.20m,
                        UpdatedAt = DateTime.UtcNow
                    },
                    new ShipmentPricingRuleEntity
                    {
                        ShipmentPricingRuleId = Guid.NewGuid(),
                        RuleName = "PREMIUM Centro OCA",
                        CustomerTypeId = PremiumCustomerTypeId,
                        PostalCodePrefix = "5",
                        CarrierId = Guid.Parse("6c1a2f12-0c19-4f23-9fb2-000000000002"),
                        BaseCost = 12.90m,
                        UpdatedAt = DateTime.UtcNow
                    }
                }
            };

            await dbContext.ShipmentPricingSettings.AddAsync(settings, cancellationToken);
        }

        await EnsureShipmentPricingSeedDataAsync(dbContext, cancellationToken);

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static async Task EnsureShipmentPricingSeedDataAsync(OmsDbContext dbContext, CancellationToken cancellationToken)
    {
        var settingsExists = await dbContext.ShipmentPricingSettings
            .AsNoTracking()
            .AnyAsync(current => current.ShipmentPricingSettingsId == 1, cancellationToken);

        if (!settingsExists)
        {
            return;
        }

        var existingRuleKeys = await dbContext.ShipmentPricingRules
            .AsNoTracking()
            .Select(current => new { current.CustomerTypeId, current.PostalCodePrefix, current.CarrierId })
            .ToListAsync(cancellationToken);

        var desiredRules = new[]
        {
            new { RuleName = "STANDARD AMBA Andreani", CustomerTypeId = StandardCustomerTypeId, PostalCodePrefix = "1", CarrierId = Guid.Parse("6c1a2f12-0c19-4f23-9fb2-000000000001"), BaseCost = 9.50m },
            new { RuleName = "STANDARD Centro Andreani", CustomerTypeId = StandardCustomerTypeId, PostalCodePrefix = "5", CarrierId = Guid.Parse("6c1a2f12-0c19-4f23-9fb2-000000000001"), BaseCost = 15.75m },
            new { RuleName = "STANDARD Litoral Andreani", CustomerTypeId = StandardCustomerTypeId, PostalCodePrefix = "2", CarrierId = Guid.Parse("6c1a2f12-0c19-4f23-9fb2-000000000001"), BaseCost = 13.20m },
            new { RuleName = "PREMIUM Centro OCA", CustomerTypeId = PremiumCustomerTypeId, PostalCodePrefix = "5", CarrierId = Guid.Parse("6c1a2f12-0c19-4f23-9fb2-000000000002"), BaseCost = 12.90m },
        };

        foreach (var desiredRule in desiredRules)
        {
            var existingRule = existingRuleKeys.SingleOrDefault(current =>
                current.CustomerTypeId == desiredRule.CustomerTypeId &&
                current.PostalCodePrefix == desiredRule.PostalCodePrefix &&
                current.CarrierId == desiredRule.CarrierId);

            if (existingRule is not null)
            {
                continue;
            }

            await dbContext.ShipmentPricingRules.AddAsync(new ShipmentPricingRuleEntity
            {
                ShipmentPricingRuleId = Guid.NewGuid(),
                ShipmentPricingSettingsId = 1,
                RuleName = desiredRule.RuleName,
                CustomerTypeId = desiredRule.CustomerTypeId,
                PostalCodePrefix = desiredRule.PostalCodePrefix,
                CarrierId = desiredRule.CarrierId,
                BaseCost = desiredRule.BaseCost,
                UpdatedAt = DateTime.UtcNow
            }, cancellationToken);
        }
    }

    private static async Task EnsureCustomerTypeSeedDataAsync(OmsDbContext dbContext, CancellationToken cancellationToken)
    {
        var desiredCustomerTypes = new[]
        {
            new CustomerTypeEntity
            {
                CustomerTypeId = StandardCustomerTypeId,
                Code = "STANDARD",
                Name = "Standard",
                Description = "Clientes estándar con tarifa general.",
                IsActive = true,
                UpdatedAt = DateTime.UtcNow
            },
            new CustomerTypeEntity
            {
                CustomerTypeId = PremiumCustomerTypeId,
                Code = "PREMIUM",
                Name = "Premium",
                Description = "Clientes preferenciales con acuerdos comerciales específicos.",
                IsActive = true,
                UpdatedAt = DateTime.UtcNow
            },
            new CustomerTypeEntity
            {
                CustomerTypeId = WholesaleCustomerTypeId,
                Code = "WHOLESALE",
                Name = "Mayorista",
                Description = "Clientes mayoristas o cuentas corporativas de alto volumen.",
                IsActive = true,
                UpdatedAt = DateTime.UtcNow
            }
        };

        var existingCustomerTypes = await dbContext.CustomerTypes
            .ToDictionaryAsync(current => current.CustomerTypeId, cancellationToken);

        foreach (var desiredCustomerType in desiredCustomerTypes)
        {
            if (existingCustomerTypes.TryGetValue(desiredCustomerType.CustomerTypeId, out var existingCustomerType))
            {
                existingCustomerType.Code = desiredCustomerType.Code;
                existingCustomerType.Name = desiredCustomerType.Name;
                existingCustomerType.Description = desiredCustomerType.Description;
                existingCustomerType.IsActive = desiredCustomerType.IsActive;
                existingCustomerType.UpdatedAt = DateTime.UtcNow;
                continue;
            }

            await dbContext.CustomerTypes.AddAsync(desiredCustomerType, cancellationToken);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static async Task EnsureCustomerTypeRelationshipsAsync(OmsDbContext dbContext, CancellationToken cancellationToken)
    {
        var standardCustomerTypeId = StandardCustomerTypeId.ToString();

        var backfillOrdersSql = $$"""
DECLARE @StandardCustomerTypeId UNIQUEIDENTIFIER = '{{standardCustomerTypeId}}';

IF COL_LENGTH(N'[dbo].[Orders]', N'CustomerTypeId') IS NOT NULL
BEGIN
    IF COL_LENGTH(N'[dbo].[Shipments]', N'CustomerType') IS NOT NULL
    BEGIN
        UPDATE [order]
        SET [order].[CustomerTypeId] = [customerType].[CustomerTypeId]
        FROM [dbo].[Orders] AS [order]
        INNER JOIN [dbo].[Shipments] AS [shipment]
            ON [shipment].[OrderId] = [order].[OrderId]
        INNER JOIN [dbo].[CustomerTypes] AS [customerType]
            ON [customerType].[Code] = UPPER(REPLACE(REPLACE(REPLACE(LTRIM(RTRIM(COALESCE([shipment].[CustomerType], N''))), N' ', N''), N'-', N''), N'_', N''))
        WHERE [order].[CustomerTypeId] IS NULL
          AND LTRIM(RTRIM(COALESCE([shipment].[CustomerType], N''))) <> N'';
    END

    UPDATE [dbo].[Orders]
    SET [CustomerTypeId] = @StandardCustomerTypeId
    WHERE [CustomerTypeId] IS NULL;
END
""";

        var backfillShipmentsSql = $$"""
DECLARE @StandardCustomerTypeId UNIQUEIDENTIFIER = '{{standardCustomerTypeId}}';

IF COL_LENGTH(N'[dbo].[Shipments]', N'CustomerTypeId') IS NOT NULL
BEGIN
    IF COL_LENGTH(N'[dbo].[Shipments]', N'CustomerType') IS NOT NULL
    BEGIN
        UPDATE [shipment]
        SET [shipment].[CustomerTypeId] = [customerType].[CustomerTypeId]
        FROM [dbo].[Shipments] AS [shipment]
        INNER JOIN [dbo].[CustomerTypes] AS [customerType]
            ON [customerType].[Code] = UPPER(REPLACE(REPLACE(REPLACE(LTRIM(RTRIM(COALESCE([shipment].[CustomerType], N''))), N' ', N''), N'-', N''), N'_', N''))
        WHERE [shipment].[CustomerTypeId] IS NULL
          AND LTRIM(RTRIM(COALESCE([shipment].[CustomerType], N''))) <> N'';
    END

    UPDATE [shipment]
    SET [shipment].[CustomerTypeId] = [order].[CustomerTypeId]
    FROM [dbo].[Shipments] AS [shipment]
    INNER JOIN [dbo].[Orders] AS [order]
        ON [order].[OrderId] = [shipment].[OrderId]
    WHERE [shipment].[CustomerTypeId] IS NULL
      AND [order].[CustomerTypeId] IS NOT NULL;

    UPDATE [dbo].[Shipments]
    SET [CustomerTypeId] = @StandardCustomerTypeId
    WHERE [CustomerTypeId] IS NULL;
END
""";

        var backfillPricingRulesSql = $$"""
DECLARE @StandardCustomerTypeId UNIQUEIDENTIFIER = '{{standardCustomerTypeId}}';

IF COL_LENGTH(N'[dbo].[ShipmentPricingRules]', N'CustomerTypeId') IS NOT NULL
BEGIN
    IF COL_LENGTH(N'[dbo].[ShipmentPricingRules]', N'CustomerType') IS NOT NULL
    BEGIN
        UPDATE [rule]
        SET [rule].[CustomerTypeId] = [customerType].[CustomerTypeId]
        FROM [dbo].[ShipmentPricingRules] AS [rule]
        INNER JOIN [dbo].[CustomerTypes] AS [customerType]
            ON [customerType].[Code] = UPPER(REPLACE(REPLACE(REPLACE(LTRIM(RTRIM(COALESCE([rule].[CustomerType], N''))), N' ', N''), N'-', N''), N'_', N''))
        WHERE [rule].[CustomerTypeId] IS NULL
          AND LTRIM(RTRIM(COALESCE([rule].[CustomerType], N''))) <> N'';
    END

    UPDATE [dbo].[ShipmentPricingRules]
    SET [CustomerTypeId] = @StandardCustomerTypeId
    WHERE [CustomerTypeId] IS NULL;
END
""";

        const string ensureOrdersNotNullSql = """
IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Orders]') AND [name] = 'CustomerTypeId' AND is_nullable = 1)
BEGIN
    ALTER TABLE [dbo].[Orders] ALTER COLUMN [CustomerTypeId] UNIQUEIDENTIFIER NOT NULL;
END
""";

        const string ensureShipmentsNotNullSql = """
IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Shipments]') AND [name] = 'CustomerTypeId' AND is_nullable = 1)
BEGIN
    ALTER TABLE [dbo].[Shipments] ALTER COLUMN [CustomerTypeId] UNIQUEIDENTIFIER NOT NULL;
END
""";

        const string ensurePricingRulesNotNullSql = """
IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[ShipmentPricingRules]') AND [name] = 'CustomerTypeId' AND is_nullable = 1)
BEGIN
    ALTER TABLE [dbo].[ShipmentPricingRules] ALTER COLUMN [CustomerTypeId] UNIQUEIDENTIFIER NOT NULL;
END
""";

        const string ensureOrdersCustomerTypeFkSql = """
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE [name] = 'FK_Orders_CustomerTypes')
BEGIN
    ALTER TABLE [dbo].[Orders]
    ADD CONSTRAINT [FK_Orders_CustomerTypes]
        FOREIGN KEY ([CustomerTypeId]) REFERENCES [dbo].[CustomerTypes]([CustomerTypeId]);
END

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE [name] = 'IX_Orders_CustomerTypeId' AND object_id = OBJECT_ID(N'[dbo].[Orders]'))
BEGIN
    CREATE INDEX [IX_Orders_CustomerTypeId] ON [dbo].[Orders]([CustomerTypeId]);
END
""";

        const string ensureShipmentsCustomerTypeFkSql = """
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE [name] = 'FK_Shipments_CustomerTypes')
BEGIN
    ALTER TABLE [dbo].[Shipments]
    ADD CONSTRAINT [FK_Shipments_CustomerTypes]
        FOREIGN KEY ([CustomerTypeId]) REFERENCES [dbo].[CustomerTypes]([CustomerTypeId]);
END

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE [name] = 'IX_Shipments_CustomerTypeId' AND object_id = OBJECT_ID(N'[dbo].[Shipments]'))
BEGIN
    CREATE INDEX [IX_Shipments_CustomerTypeId] ON [dbo].[Shipments]([CustomerTypeId]);
END
""";

        const string ensurePricingRulesCustomerTypeFkSql = """
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE [name] = 'FK_ShipmentPricingRules_CustomerTypes')
BEGIN
    ALTER TABLE [dbo].[ShipmentPricingRules]
    ADD CONSTRAINT [FK_ShipmentPricingRules_CustomerTypes]
        FOREIGN KEY ([CustomerTypeId]) REFERENCES [dbo].[CustomerTypes]([CustomerTypeId]);
END
""";

        const string ensurePricingIndexSql = """
IF EXISTS (SELECT 1 FROM sys.indexes WHERE [name] = 'UQ_ShipmentPricingRules_SettingsPrefix' AND object_id = OBJECT_ID(N'[dbo].[ShipmentPricingRules]'))
BEGIN
    DROP INDEX [UQ_ShipmentPricingRules_SettingsPrefix] ON [dbo].[ShipmentPricingRules];
END

IF EXISTS (SELECT 1 FROM sys.indexes WHERE [name] = 'UQ_ShipmentPricingRules_SettingsCustomerCarrierPrefix' AND object_id = OBJECT_ID(N'[dbo].[ShipmentPricingRules]'))
BEGIN
    DROP INDEX [UQ_ShipmentPricingRules_SettingsCustomerCarrierPrefix] ON [dbo].[ShipmentPricingRules];
END

CREATE UNIQUE INDEX [UQ_ShipmentPricingRules_SettingsCustomerCarrierPrefix]
    ON [dbo].[ShipmentPricingRules]([ShipmentPricingSettingsId], [CustomerTypeId], [PostalCodePrefix], [CarrierId]);
""";

        await dbContext.Database.ExecuteSqlRawAsync(backfillOrdersSql, cancellationToken);
        await dbContext.Database.ExecuteSqlRawAsync(backfillShipmentsSql, cancellationToken);
        await dbContext.Database.ExecuteSqlRawAsync(backfillPricingRulesSql, cancellationToken);
        await dbContext.Database.ExecuteSqlRawAsync(ensureOrdersNotNullSql, cancellationToken);
        await dbContext.Database.ExecuteSqlRawAsync(ensureShipmentsNotNullSql, cancellationToken);
        await dbContext.Database.ExecuteSqlRawAsync(ensurePricingRulesNotNullSql, cancellationToken);
        await dbContext.Database.ExecuteSqlRawAsync(ensureOrdersCustomerTypeFkSql, cancellationToken);
        await dbContext.Database.ExecuteSqlRawAsync(ensureShipmentsCustomerTypeFkSql, cancellationToken);
        await dbContext.Database.ExecuteSqlRawAsync(ensurePricingRulesCustomerTypeFkSql, cancellationToken);
        await dbContext.Database.ExecuteSqlRawAsync(ensurePricingIndexSql, cancellationToken);
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
        [CustomerTypeId] UNIQUEIDENTIFIER NOT NULL,
        [PostalCodePrefix] NVARCHAR(12) NOT NULL,
        [CarrierId] UNIQUEIDENTIFIER NOT NULL,
        [BaseCost] DECIMAL(18,2) NOT NULL,
        [UpdatedAt] DATETIME2 NOT NULL,
        CONSTRAINT [CK_ShipmentPricingRules_BaseCost] CHECK ([BaseCost] >= 0),
        CONSTRAINT [FK_ShipmentPricingRules_ShipmentPricingSettings] FOREIGN KEY ([ShipmentPricingSettingsId]) REFERENCES [dbo].[ShipmentPricingSettings]([ShipmentPricingSettingsId]),
        CONSTRAINT [FK_ShipmentPricingRules_CustomerTypes] FOREIGN KEY ([CustomerTypeId]) REFERENCES [dbo].[CustomerTypes]([CustomerTypeId]),
        CONSTRAINT [FK_ShipmentPricingRules_Carriers] FOREIGN KEY ([CarrierId]) REFERENCES [dbo].[Carriers]([CarrierId])
    );
END
""";

        const string ensureCustomerTypeIdColumnSql = """
IF COL_LENGTH(N'[dbo].[ShipmentPricingRules]', N'CustomerTypeId') IS NULL
BEGIN
    ALTER TABLE [dbo].[ShipmentPricingRules] ADD [CustomerTypeId] UNIQUEIDENTIFIER NULL;
END
""";

        const string ensureCarrierIdColumnSql = """
IF COL_LENGTH(N'[dbo].[ShipmentPricingRules]', N'CarrierId') IS NULL
BEGIN
    ALTER TABLE [dbo].[ShipmentPricingRules] ADD [CarrierId] UNIQUEIDENTIFIER NULL;
END
""";

        const string backfillCarrierIdColumnSql = """
IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[ShipmentPricingRules]') AND [name] = 'CarrierId')
AND EXISTS (SELECT 1 FROM [dbo].[ShipmentPricingRules] WHERE [CarrierId] IS NULL)
BEGIN
    DECLARE @DefaultCarrierId UNIQUEIDENTIFIER;
    SELECT TOP 1 @DefaultCarrierId = [CarrierId] FROM [dbo].[Carriers] ORDER BY [Name];

    UPDATE [dbo].[ShipmentPricingRules] SET [CarrierId] = @DefaultCarrierId WHERE [CarrierId] IS NULL;
END
""";

        const string ensureCarrierIdNotNullSql = """
IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[ShipmentPricingRules]') AND [name] = 'CarrierId' AND is_nullable = 1)
BEGIN
    ALTER TABLE [dbo].[ShipmentPricingRules] ALTER COLUMN [CarrierId] UNIQUEIDENTIFIER NOT NULL;
END
""";

        const string ensureCarrierFkSql = """
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_ShipmentPricingRules_Carriers')
BEGIN
    ALTER TABLE [dbo].[ShipmentPricingRules]
    ADD CONSTRAINT [FK_ShipmentPricingRules_Carriers] FOREIGN KEY ([CarrierId]) REFERENCES [dbo].[Carriers]([CarrierId]);
END
""";

        await dbContext.Database.ExecuteSqlRawAsync(createSettingsTableSql, cancellationToken);
        await dbContext.Database.ExecuteSqlRawAsync(createRulesTableSql, cancellationToken);
        await dbContext.Database.ExecuteSqlRawAsync(ensureCustomerTypeIdColumnSql, cancellationToken);
        await dbContext.Database.ExecuteSqlRawAsync(ensureCarrierIdColumnSql, cancellationToken);
        await dbContext.Database.ExecuteSqlRawAsync(backfillCarrierIdColumnSql, cancellationToken);
        await dbContext.Database.ExecuteSqlRawAsync(ensureCarrierIdNotNullSql, cancellationToken);
        await dbContext.Database.ExecuteSqlRawAsync(ensureCarrierFkSql, cancellationToken);
    }

    private static async Task EnsureCustomerTypeSchemaAsync(OmsDbContext dbContext, CancellationToken cancellationToken)
    {
        const string createCustomerTypesTableSql = """
IF OBJECT_ID(N'[dbo].[CustomerTypes]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[CustomerTypes]
    (
        [CustomerTypeId] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        [Code] NVARCHAR(30) NOT NULL,
        [Name] NVARCHAR(120) NOT NULL,
        [Description] NVARCHAR(300) NOT NULL CONSTRAINT [DF_CustomerTypes_Description] DEFAULT N'',
        [IsActive] BIT NOT NULL CONSTRAINT [DF_CustomerTypes_IsActive] DEFAULT 1,
        [UpdatedAt] DATETIME2 NOT NULL,
        CONSTRAINT [UQ_CustomerTypes_Code] UNIQUE ([Code])
    );

    CREATE INDEX [IX_CustomerTypes_IsActive_Name]
        ON [dbo].[CustomerTypes]([IsActive], [Name]);
END
""";

        const string ensureOrdersCustomerTypeIdColumnSql = """
IF COL_LENGTH(N'[dbo].[Orders]', N'CustomerTypeId') IS NULL
BEGIN
    ALTER TABLE [dbo].[Orders] ADD [CustomerTypeId] UNIQUEIDENTIFIER NULL;
END
""";

        const string ensureShipmentsCustomerTypeIdColumnSql = """
IF COL_LENGTH(N'[dbo].[Shipments]', N'CustomerTypeId') IS NULL
BEGIN
    ALTER TABLE [dbo].[Shipments] ADD [CustomerTypeId] UNIQUEIDENTIFIER NULL;
END
""";

        await dbContext.Database.ExecuteSqlRawAsync(createCustomerTypesTableSql, cancellationToken);
        await dbContext.Database.ExecuteSqlRawAsync(ensureOrdersCustomerTypeIdColumnSql, cancellationToken);
        await dbContext.Database.ExecuteSqlRawAsync(ensureShipmentsCustomerTypeIdColumnSql, cancellationToken);
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

IF COL_LENGTH(N'[dbo].[Shipments]', N'RecipientName') IS NULL
BEGIN
    ALTER TABLE [dbo].[Shipments] ADD [RecipientName] NVARCHAR(160) NOT NULL CONSTRAINT [DF_Shipments_RecipientName] DEFAULT N'Destinatario pendiente';
END

IF COL_LENGTH(N'[dbo].[Shipments]', N'RecipientPhone') IS NULL
BEGIN
    ALTER TABLE [dbo].[Shipments] ADD [RecipientPhone] NVARCHAR(40) NOT NULL CONSTRAINT [DF_Shipments_RecipientPhone] DEFAULT N'No informado';
END

IF COL_LENGTH(N'[dbo].[Shipments]', N'RecipientEmail') IS NULL
BEGIN
    ALTER TABLE [dbo].[Shipments] ADD [RecipientEmail] NVARCHAR(160) NOT NULL CONSTRAINT [DF_Shipments_RecipientEmail] DEFAULT N'';
END
""";

        await dbContext.Database.ExecuteSqlRawAsync(createCarriersTableSql, cancellationToken);
        await dbContext.Database.ExecuteSqlRawAsync(alterShipmentsTableSql, cancellationToken);
    }
}
