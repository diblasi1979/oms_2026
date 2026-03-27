IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Orders_Status_CreatedAt' AND object_id = OBJECT_ID('Orders'))
CREATE INDEX IX_Orders_Status_CreatedAt ON Orders (Status, CreatedAt DESC);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Orders_Origin_CreatedAt' AND object_id = OBJECT_ID('Orders'))
CREATE INDEX IX_Orders_Origin_CreatedAt ON Orders (Origin, CreatedAt DESC);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_OrderItems_OrderId' AND object_id = OBJECT_ID('OrderItems'))
CREATE INDEX IX_OrderItems_OrderId ON OrderItems (OrderId);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Inventory_Sku_WarehouseId' AND object_id = OBJECT_ID('Inventory'))
CREATE INDEX IX_Inventory_Sku_WarehouseId ON Inventory (Sku, WarehouseId);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Shipments_OrderId' AND object_id = OBJECT_ID('Shipments'))
CREATE INDEX IX_Shipments_OrderId ON Shipments (OrderId);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Shipments_CarrierId' AND object_id = OBJECT_ID('Shipments'))
CREATE INDEX IX_Shipments_CarrierId ON Shipments (CarrierId);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Carriers_IsActive_Name' AND object_id = OBJECT_ID('Carriers'))
CREATE INDEX IX_Carriers_IsActive_Name ON Carriers (IsActive, Name);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ShipmentEvents_ShipmentId_EventTimestamp' AND object_id = OBJECT_ID('ShipmentEvents'))
CREATE INDEX IX_ShipmentEvents_ShipmentId_EventTimestamp ON ShipmentEvents (ShipmentId, EventTimestamp DESC);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_OrderLogs_OrderId_EventTimestamp' AND object_id = OBJECT_ID('OrderLogs'))
CREATE INDEX IX_OrderLogs_OrderId_EventTimestamp ON OrderLogs (OrderId, EventTimestamp DESC);
GO

IF NOT EXISTS (SELECT 1 FROM Warehouses)
INSERT INTO Warehouses (WarehouseId, Name, City, State, PostalCode, Latitude, Longitude)
VALUES
('F5E10D20-8E72-4A9A-8C24-5B8A6B001001', 'Buenos Aires Hub', 'Buenos Aires', 'Buenos Aires', '1000', -34.603700, -58.381600),
('F5E10D20-8E72-4A9A-8C24-5B8A6B001002', 'Cordoba Fulfillment', 'Cordoba', 'Cordoba', '5000', -31.420100, -64.188800),
('F5E10D20-8E72-4A9A-8C24-5B8A6B001003', 'Rosario Cross Dock', 'Rosario', 'Santa Fe', '2000', -32.944200, -60.650500);
GO

IF NOT EXISTS (SELECT 1 FROM Inventory)
INSERT INTO Inventory (InventoryId, Sku, WarehouseId, PhysicalStock, ReservedStock, LocationCode)
VALUES
(NEWID(), 'SKU-CHAIR-001', 'F5E10D20-8E72-4A9A-8C24-5B8A6B001001', 120, 12, 'BA-01-A-04'),
(NEWID(), 'SKU-DESK-002', 'F5E10D20-8E72-4A9A-8C24-5B8A6B001001', 80, 9, 'BA-02-C-02'),
(NEWID(), 'SKU-LABEL-003', 'F5E10D20-8E72-4A9A-8C24-5B8A6B001001', 200, 20, 'BA-05-B-01'),
(NEWID(), 'SKU-CHAIR-001', 'F5E10D20-8E72-4A9A-8C24-5B8A6B001002', 60, 10, 'CB-03-A-02'),
(NEWID(), 'SKU-DESK-002', 'F5E10D20-8E72-4A9A-8C24-5B8A6B001002', 140, 18, 'CB-01-D-08'),
(NEWID(), 'SKU-LABEL-003', 'F5E10D20-8E72-4A9A-8C24-5B8A6B001002', 150, 30, 'CB-07-F-01'),
(NEWID(), 'SKU-CHAIR-001', 'F5E10D20-8E72-4A9A-8C24-5B8A6B001003', 40, 2, 'RS-02-A-01'),
(NEWID(), 'SKU-DESK-002', 'F5E10D20-8E72-4A9A-8C24-5B8A6B001003', 90, 11, 'RS-03-D-03'),
(NEWID(), 'SKU-LABEL-003', 'F5E10D20-8E72-4A9A-8C24-5B8A6B001003', 180, 25, 'RS-05-C-07');
GO

IF NOT EXISTS (SELECT 1 FROM Orders WHERE OrderId = 'C71AB4D9-09A9-4DC0-BF89-E7614ED4B801')
INSERT INTO Orders (OrderId, Customer, Status, Origin, Total, DestinationCity, DestinationState, DestinationPostalCode, DestinationLatitude, DestinationLongitude, AssignedWarehouseId, ShipmentTrackingNumber, CreatedAt, UpdatedAt)
VALUES
('C71AB4D9-09A9-4DC0-BF89-E7614ED4B801', 'Distribuidora Norte', 'Preparing', 'Web', 683.60, 'Mendoza', 'Mendoza', '5500', -32.889500, -68.845800, 'F5E10D20-8E72-4A9A-8C24-5B8A6B001002', 'TRK-20260327-48291', DATEADD(HOUR, -6, SYSUTCDATETIME()), DATEADD(HOUR, -5, SYSUTCDATETIME()));
GO

IF NOT EXISTS (SELECT 1 FROM OrderItems WHERE OrderId = 'C71AB4D9-09A9-4DC0-BF89-E7614ED4B801')
INSERT INTO OrderItems (OrderItemId, OrderId, Sku, Quantity, UnitPrice)
VALUES
(NEWID(), 'C71AB4D9-09A9-4DC0-BF89-E7614ED4B801', 'SKU-CHAIR-001', 4, 149.90),
(NEWID(), 'C71AB4D9-09A9-4DC0-BF89-E7614ED4B801', 'SKU-LABEL-003', 20, 4.20);
GO

IF NOT EXISTS (SELECT 1 FROM OrderLogs WHERE OrderId = 'C71AB4D9-09A9-4DC0-BF89-E7614ED4B801')
INSERT INTO OrderLogs (OrderLogId, OrderId, EventName, Details, EventTimestamp)
VALUES
(NEWID(), 'C71AB4D9-09A9-4DC0-BF89-E7614ED4B801', 'OrderCreated', 'Orden creada desde Web.', DATEADD(HOUR, -6, SYSUTCDATETIME())),
(NEWID(), 'C71AB4D9-09A9-4DC0-BF89-E7614ED4B801', 'StockReserved', 'Stock reservado para evitar overselling.', DATEADD(HOUR, -6, DATEADD(MINUTE, 5, SYSUTCDATETIME()))),
(NEWID(), 'C71AB4D9-09A9-4DC0-BF89-E7614ED4B801', 'WarehouseAssigned', 'Asignado a Cordoba Fulfillment por cercania y disponibilidad.', DATEADD(HOUR, -6, DATEADD(MINUTE, 6, SYSUTCDATETIME()))),
(NEWID(), 'C71AB4D9-09A9-4DC0-BF89-E7614ED4B801', 'OrderStatusChanged', 'Estado actualizado a Preparing.', DATEADD(HOUR, -6, DATEADD(MINUTE, 20, SYSUTCDATETIME())));
GO

IF NOT EXISTS (SELECT 1 FROM Carriers)
INSERT INTO Carriers (CarrierId, Code, Name, ServiceLevel, TrackingUrlTemplate, SupportEmail, SupportPhone, InsuranceSupported, IsActive, Notes)
VALUES
('6C1A2F12-0C19-4F23-9FB2-000000000001', 'ANDREANI', 'Andreani', 'Standard', 'https://www.andreani.com/#!/informacionEnvio/{trackingNumber}', 'soporte@andreani.com', '+54 800 122 1111', 1, 1, 'Carrier principal para operación nacional.'),
('6C1A2F12-0C19-4F23-9FB2-000000000002', 'OCA', 'OCA', 'Express', 'https://www.oca.com.ar/Busquedas/Envios?numero={trackingNumber}', 'clientes@oca.com.ar', '+54 800 999 7700', 1, 1, 'Carrier alternativo para zonas urbanas.'),
('6C1A2F12-0C19-4F23-9FB2-000000000003', 'CORREOAR', 'Correo Argentino', 'Economy', 'https://www.correoargentino.com.ar/formularios/e-commerce?id={trackingNumber}', 'empresas@correoargentino.com.ar', '+54 11 4891 9191', 0, 1, 'Cobertura nacional con costo moderado.');
GO

IF NOT EXISTS (SELECT 1 FROM Shipments WHERE ShipmentId = 'D91AB4D9-09A9-4DC0-BF89-E7614ED4B802')
INSERT INTO Shipments (ShipmentId, OrderId, CarrierId, RecipientName, RecipientPhone, RecipientEmail, Carrier, TrackingNumber, Status, WeightKg, HeightCm, WidthCm, LengthCm, ShippingCost, DestinationAddress, CreatedAt, UpdatedAt)
VALUES
('D91AB4D9-09A9-4DC0-BF89-E7614ED4B802', 'C71AB4D9-09A9-4DC0-BF89-E7614ED4B801', '6C1A2F12-0C19-4F23-9FB2-000000000001', 'Marcela Gomez', '+54 261 555 0101', 'marcela.gomez@example.com', 'Andreani', 'TRK-20260327-48291', 'InTransit', 8.400, 45.00, 40.00, 55.00, 18.50, 'Av. San Martin 123, Mendoza', DATEADD(HOUR, -6, DATEADD(MINUTE, 30, SYSUTCDATETIME())), DATEADD(HOUR, -5, SYSUTCDATETIME()));
GO

IF NOT EXISTS (SELECT 1 FROM ShipmentEvents WHERE ShipmentId = 'D91AB4D9-09A9-4DC0-BF89-E7614ED4B802')
INSERT INTO ShipmentEvents (ShipmentEventId, ShipmentId, Status, Notes, EventTimestamp)
VALUES
(NEWID(), 'D91AB4D9-09A9-4DC0-BF89-E7614ED4B802', 'LabelCreated', 'Etiqueta simulada generada en OMS.', DATEADD(HOUR, -6, DATEADD(MINUTE, 30, SYSUTCDATETIME()))),
(NEWID(), 'D91AB4D9-09A9-4DC0-BF89-E7614ED4B802', 'InTransit', 'Unidad despachada desde hub regional.', DATEADD(HOUR, -5, SYSUTCDATETIME()));
GO
