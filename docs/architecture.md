# OMS Architecture

## Stack
- Frontend: Vue 3 + TypeScript + PrimeVue
- Backend: ASP.NET Core Web API por servicio con Swagger UI y JWT basico
- Database: SQL Server + EF Core 9 con un DbContext compartido para la base inicial

## Dominios
- Orders.Api: crea ordenes, lista con filtros, devuelve detalle y aplica ruteo de warehouse con reserva inmediata persistida en SQL Server.
- Inventory.Api: expone consulta de stock, reserva y liberacion usando control transaccional en SQL Server.
- Shipments.Api: crea envios, genera etiqueta simulada y recibe webhooks de carriers persistiendo tracking y eventos.
- Integrations.Api: recibe payloads externos de Shopify, Mercado Libre, WooCommerce y Amazon y los normaliza en la tabla ExternalWebhookEvents.

## Lógica implementada
- Order Routing Logic: selecciona el warehouse mas cercano que pueda cubrir completamente la orden usando distancia geodesica simple.
- Stock Reservation: al crear una orden, aumenta stock reservado y evita usar stock no disponible.
- Carrier Webhook: actualiza estado de un shipment por tracking number.
- External Order Webhook: transforma payloads heterogeneos a una estructura canonica idempotente.

## Decisiones de arquitectura
- La base inicial ya usa EF Core contra SQL Server mediante el proyecto compartido Oms.Persistence.
- Para bootstrap local, la base configurada es oms_test; si el usuario api_oms no tiene permisos de creación, debe ejecutarse database/schema/00-create-database.sql con un usuario con permisos elevados.
- JWT se emite en Orders.Api y se valida en todos los servicios con la misma configuracion.
- Swagger UI esta habilitado en todos los servicios para inspeccion y pruebas manuales.

## Siguientes evoluciones recomendadas
- Separar la persistencia compartida en contextos o bases por servicio cuando se quiera endurecer el modelo de microservicios.
- Incorporar API Gateway/BFF para centralizar auth, CORS y agregacion para frontend.
- Añadir outbox/event bus para sincronizacion asincrona entre Orders, Inventory y Shipments.
- Añadir pruebas unitarias e integracion alrededor de routing, reserva y webhooks.
