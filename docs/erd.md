# OMS ERD

## Entidades principales

Warehouses
- WarehouseId PK
- Name
- City
- State
- PostalCode
- Latitude
- Longitude
- CreatedAt

Orders
- OrderId PK
- Customer
- Status
- Origin
- Total
- DestinationCity
- DestinationState
- DestinationPostalCode
- DestinationLatitude
- DestinationLongitude
- AssignedWarehouseId FK -> Warehouses.WarehouseId
- ShipmentTrackingNumber
- CreatedAt
- UpdatedAt

OrderItems
- OrderItemId PK
- OrderId FK -> Orders.OrderId
- Sku
- Quantity
- UnitPrice

Inventory
- InventoryId PK
- Sku
- WarehouseId FK -> Warehouses.WarehouseId
- PhysicalStock
- ReservedStock
- AvailableStock computed
- LocationCode
- RowVersion

Shipments
- ShipmentId PK
- OrderId FK -> Orders.OrderId
- Carrier
- TrackingNumber
- Status
- WeightKg
- HeightCm
- WidthCm
- LengthCm
- ShippingCost
- DestinationAddress
- CreatedAt
- UpdatedAt

ShipmentEvents
- ShipmentEventId PK
- ShipmentId FK -> Shipments.ShipmentId
- Status
- Notes
- EventTimestamp

OrderLogs
- OrderLogId PK
- OrderId FK -> Orders.OrderId
- EventName
- Details
- EventTimestamp

ExternalWebhookEvents
- ExternalWebhookEventId PK
- Provider
- ExternalOrderId
- Payload
- CanonicalSnapshot
- ReceivedAt
- ProcessedAt

## Relaciones

- Un Warehouse tiene muchos registros de Inventory.
- Un Warehouse puede estar asignado a muchas Orders.
- Una Order tiene muchos OrderItems.
- Una Order tiene muchos OrderLogs.
- Una Order puede tener cero o un Shipment activo en la base inicial.
- Un Shipment tiene muchos ShipmentEvents.
- ExternalWebhookEvents conserva la traza del pedido recibido y su forma canónica para idempotencia.

## Vista textual

Warehouses 1 --- * Inventory
Warehouses 1 --- * Orders
Orders 1 --- * OrderItems
Orders 1 --- * OrderLogs
Orders 1 --- 0..1 Shipments
Shipments 1 --- * ShipmentEvents
