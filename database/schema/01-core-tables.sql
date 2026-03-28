CREATE TABLE Warehouses (
    WarehouseId UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    Name NVARCHAR(120) NOT NULL,
    City NVARCHAR(100) NOT NULL,
    State NVARCHAR(100) NOT NULL,
    PostalCode NVARCHAR(20) NOT NULL,
    Latitude DECIMAL(9,6) NOT NULL,
    Longitude DECIMAL(9,6) NOT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME()
);
GO

CREATE TABLE CustomerTypes (
    CustomerTypeId UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    Code NVARCHAR(30) NOT NULL,
    Name NVARCHAR(120) NOT NULL,
    Description NVARCHAR(300) NOT NULL DEFAULT N'',
    AssignedPriceListName NVARCHAR(120) NOT NULL DEFAULT N'',
    InsuranceRatePercentage DECIMAL(9,4) NOT NULL DEFAULT 0,
    IsActive BIT NOT NULL DEFAULT 1,
    UpdatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT UQ_CustomerTypes_Code UNIQUE (Code)
);
GO

CREATE TABLE PostalCodes (
    PostalCodeId UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    Country NVARCHAR(80) NOT NULL,
    Province NVARCHAR(120) NOT NULL,
    Locality NVARCHAR(120) NOT NULL,
    PostalCode NVARCHAR(20) NOT NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    Zone NVARCHAR(60) NOT NULL
);
GO

CREATE TABLE PostalCodePriceLists (
    PostalCodePriceListId UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    ListName NVARCHAR(120) NOT NULL,
    PostalCode NVARCHAR(20) NOT NULL,
    Value DECIMAL(18,2) NOT NULL,
    Zone NVARCHAR(60) NOT NULL,
    CONSTRAINT CK_PostalCodePriceLists_Value CHECK (Value >= 0)
);
GO

CREATE TABLE Orders (
    OrderId UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    Customer NVARCHAR(160) NOT NULL,
    CustomerTypeId UNIQUEIDENTIFIER NOT NULL,
    Status NVARCHAR(30) NOT NULL,
    Origin NVARCHAR(30) NOT NULL,
    Total DECIMAL(18,2) NOT NULL,
    DestinationCity NVARCHAR(100) NOT NULL,
    DestinationState NVARCHAR(100) NOT NULL,
    DestinationPostalCode NVARCHAR(20) NOT NULL,
    DestinationLatitude DECIMAL(9,6) NOT NULL,
    DestinationLongitude DECIMAL(9,6) NOT NULL,
    AssignedWarehouseId UNIQUEIDENTIFIER NULL,
    ShipmentTrackingNumber NVARCHAR(80) NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    UpdatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_Orders_Warehouses FOREIGN KEY (AssignedWarehouseId) REFERENCES Warehouses (WarehouseId),
    CONSTRAINT FK_Orders_CustomerTypes FOREIGN KEY (CustomerTypeId) REFERENCES CustomerTypes (CustomerTypeId),
    CONSTRAINT CK_Orders_Status CHECK (Status IN ('Pending', 'Preparing', 'Shipped', 'Delivered', 'Cancelled')),
    CONSTRAINT CK_Orders_Origin CHECK (Origin IN ('Marketplace', 'Web', 'Manual'))
);
GO

CREATE TABLE OrderItems (
    OrderItemId UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    OrderId UNIQUEIDENTIFIER NOT NULL,
    Sku NVARCHAR(80) NOT NULL,
    Quantity INT NOT NULL,
    UnitPrice DECIMAL(18,2) NOT NULL,
    CONSTRAINT FK_OrderItems_Orders FOREIGN KEY (OrderId) REFERENCES Orders (OrderId),
    CONSTRAINT CK_OrderItems_Quantity CHECK (Quantity > 0),
    CONSTRAINT CK_OrderItems_UnitPrice CHECK (UnitPrice > 0)
);
GO

CREATE TABLE Inventory (
    InventoryId UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    Sku NVARCHAR(80) NOT NULL,
    WarehouseId UNIQUEIDENTIFIER NOT NULL,
    PhysicalStock INT NOT NULL,
    ReservedStock INT NOT NULL,
    AvailableStock AS (PhysicalStock - ReservedStock) PERSISTED,
    LocationCode NVARCHAR(60) NOT NULL,
    RowVersion ROWVERSION NOT NULL,
    CONSTRAINT FK_Inventory_Warehouses FOREIGN KEY (WarehouseId) REFERENCES Warehouses (WarehouseId),
    CONSTRAINT UQ_Inventory_WarehouseSku UNIQUE (WarehouseId, Sku),
    CONSTRAINT CK_Inventory_PhysicalStock CHECK (PhysicalStock >= 0),
    CONSTRAINT CK_Inventory_ReservedStock CHECK (ReservedStock >= 0),
    CONSTRAINT CK_Inventory_ReservedVsPhysical CHECK (ReservedStock <= PhysicalStock)
);
GO

CREATE TABLE Carriers (
    CarrierId UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    Code NVARCHAR(30) NOT NULL,
    Name NVARCHAR(120) NOT NULL,
    ServiceLevel NVARCHAR(80) NOT NULL,
    TrackingUrlTemplate NVARCHAR(250) NOT NULL,
    SupportEmail NVARCHAR(160) NOT NULL DEFAULT N'',
    SupportPhone NVARCHAR(40) NOT NULL DEFAULT N'',
    InsuranceSupported BIT NOT NULL DEFAULT 0,
    IsActive BIT NOT NULL DEFAULT 1,
    Notes NVARCHAR(500) NOT NULL DEFAULT N'',
    UpdatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT UQ_Carriers_Code UNIQUE (Code)
);
GO

CREATE TABLE Shipments (
    ShipmentId UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    OrderId UNIQUEIDENTIFIER NOT NULL,
    CarrierId UNIQUEIDENTIFIER NULL,
    CustomerTypeId UNIQUEIDENTIFIER NOT NULL,
    RecipientName NVARCHAR(160) NOT NULL,
    RecipientPhone NVARCHAR(40) NOT NULL,
    RecipientEmail NVARCHAR(160) NOT NULL DEFAULT N'',
    Carrier NVARCHAR(100) NOT NULL,
    TrackingNumber NVARCHAR(80) NOT NULL,
    Status NVARCHAR(40) NOT NULL,
    WeightKg DECIMAL(12,3) NOT NULL,
    HeightCm DECIMAL(12,2) NOT NULL,
    WidthCm DECIMAL(12,2) NOT NULL,
    LengthCm DECIMAL(12,2) NOT NULL,
    DeclaredMerchandiseValue DECIMAL(18,2) NOT NULL DEFAULT 0,
    BaseShippingCost DECIMAL(18,2) NOT NULL DEFAULT 0,
    InsuranceCost DECIMAL(18,2) NOT NULL DEFAULT 0,
    ShippingCost DECIMAL(18,2) NOT NULL,
    AppliedPriceListName NVARCHAR(120) NOT NULL DEFAULT N'',
    AppliedZone NVARCHAR(60) NOT NULL DEFAULT N'',
    DestinationAddress NVARCHAR(250) NOT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    UpdatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_Shipments_Orders FOREIGN KEY (OrderId) REFERENCES Orders (OrderId),
    CONSTRAINT FK_Shipments_Carriers FOREIGN KEY (CarrierId) REFERENCES Carriers (CarrierId),
    CONSTRAINT FK_Shipments_CustomerTypes FOREIGN KEY (CustomerTypeId) REFERENCES CustomerTypes (CustomerTypeId),
    CONSTRAINT UQ_Shipments_Tracking UNIQUE (TrackingNumber)
);
GO

CREATE TABLE ShipmentEvents (
    ShipmentEventId UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    ShipmentId UNIQUEIDENTIFIER NOT NULL,
    Status NVARCHAR(40) NOT NULL,
    Notes NVARCHAR(500) NOT NULL,
    EventTimestamp DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_ShipmentEvents_Shipments FOREIGN KEY (ShipmentId) REFERENCES Shipments (ShipmentId)
);
GO

CREATE TABLE OrderLogs (
    OrderLogId UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    OrderId UNIQUEIDENTIFIER NOT NULL,
    EventName NVARCHAR(80) NOT NULL,
    Details NVARCHAR(500) NOT NULL,
    EventTimestamp DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_OrderLogs_Orders FOREIGN KEY (OrderId) REFERENCES Orders (OrderId)
);
GO

CREATE TABLE ExternalWebhookEvents (
    ExternalWebhookEventId UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    Provider NVARCHAR(40) NOT NULL,
    ExternalOrderId NVARCHAR(100) NOT NULL,
    Payload NVARCHAR(MAX) NOT NULL,
    CanonicalSnapshot NVARCHAR(MAX) NOT NULL,
    ReceivedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    ProcessedAt DATETIME2 NULL,
    CONSTRAINT UQ_ExternalWebhookEvents UNIQUE (Provider, ExternalOrderId)
);
GO

CREATE TABLE ShipmentPricingSettings (
    ShipmentPricingSettingsId INT NOT NULL PRIMARY KEY,
    DefaultBaseCost DECIMAL(18,2) NOT NULL,
    InsuranceFlatCost DECIMAL(18,2) NOT NULL,
    UpdatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME()
);
GO

CREATE TABLE ShipmentPricingRules (
    ShipmentPricingRuleId UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    ShipmentPricingSettingsId INT NOT NULL,
    RuleName NVARCHAR(120) NOT NULL,
    CustomerTypeId UNIQUEIDENTIFIER NOT NULL,
    PostalCodePrefix NVARCHAR(12) NOT NULL,
    CarrierId UNIQUEIDENTIFIER NOT NULL,
    BaseCost DECIMAL(18,2) NOT NULL,
    UpdatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT CK_ShipmentPricingRules_BaseCost CHECK (BaseCost >= 0),
    CONSTRAINT FK_ShipmentPricingRules_ShipmentPricingSettings FOREIGN KEY (ShipmentPricingSettingsId) REFERENCES ShipmentPricingSettings (ShipmentPricingSettingsId),
    CONSTRAINT FK_ShipmentPricingRules_CustomerTypes FOREIGN KEY (CustomerTypeId) REFERENCES CustomerTypes (CustomerTypeId),
    CONSTRAINT FK_ShipmentPricingRules_Carriers FOREIGN KEY (CarrierId) REFERENCES Carriers (CarrierId)
);
GO
