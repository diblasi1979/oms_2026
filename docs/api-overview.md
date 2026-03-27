# API Overview

Antes de iniciar los servicios, crea oms_test con database/schema/00-create-database.sql y luego ejecuta database/schema/01-core-tables.sql y database/schema/02-indexes-and-seed.sql.

## Auth
### POST /api/auth/token
Servicio: Orders.Api
Body:
```json
{
  "username": "admin",
  "password": "OmsAdmin123!"
}
```
Respuesta:
- accessToken
- expiresIn
- user

## Orders
### GET /api/orders
Query params:
- status
- search

### GET /api/orders/{orderId}
Devuelve detalle, items y logs.

### POST /api/orders
Crea una orden, reserva stock y asigna warehouse.
Body:
```json
{
  "customer": "Cliente Demo",
  "destinationCity": "Mendoza",
  "destinationState": "Mendoza",
  "destinationPostalCode": "5500",
  "destinationLatitude": -32.8895,
  "destinationLongitude": -68.8458,
  "origin": "Web",
  "items": [
    {
      "sku": "SKU-CHAIR-001",
      "quantity": 2,
      "unitPrice": 149.9
    }
  ]
}
```

### PUT /api/orders/{orderId}/status?status=Preparing
Actualiza estado de la orden y registra log.

## Inventory
### GET /api/inventory
Query params:
- sku
- warehouseId

### POST /api/inventory/reserve
Reserva stock en un warehouse.

### POST /api/inventory/release
Libera stock previamente reservado.

## Shipments
### GET /api/shipments
Lista envios.

### POST /api/shipments
Crea un envio.

### POST /api/shipments/{shipmentId}/label
Genera una etiqueta simulada.

### POST /api/webhooks/carriers/status-update
Recibe actualizaciones del carrier.
Body:
```json
{
  "carrier": "Andreani",
  "trackingNumber": "TRK-20260327-48291",
  "status": "InTransit",
  "notes": "Unidad despachada desde hub regional."
}
```

## Integrations
### POST /api/external-orders/{provider}
Providers soportados:
- Shopify
- MercadoLibre
- WooCommerce
- Amazon

La respuesta devuelve el payload normalizado a un contrato canonico para persistencia y posterior creacion de orden interna.
