# OMS Base

Base inicial de un Order Management System para logistica con microservicios ASP.NET Core, frontend Vue 3, EF Core y SQL Server.

## Servicios
- src/services/orders/Orders.Api
- src/services/inventory/Inventory.Api
- src/services/shipments/Shipments.Api
- src/services/integrations/Integrations.Api
- src/frontend/oms-admin

## Credenciales demo
- admin / OmsAdmin123!
- operator / OmsOperator123!

## Arranque rapido
### Backend
1. Ejecutar database/schema/00-create-database.sql con un usuario administrador de SQL Server.
2. Ejecutar database/schema/01-core-tables.sql y database/schema/02-indexes-and-seed.sql sobre oms_test.
3. Iniciar los servicios con dotnet run dentro de cada carpeta o crear perfiles de lanzamiento independientes.

La cadena de conexión configurada usa localhost:1433, base oms_test, usuario api_oms y clave api2468.

### Frontend
1. npm.cmd install
2. npm.cmd run dev

## Variables frontend opcionales
- VITE_ORDERS_API_BASE_URL
- VITE_SHIPMENTS_API_BASE_URL

## Documentacion
- docs/architecture.md
- docs/erd.md
- docs/api-overview.md
- database/schema/00-create-database.sql
- database/schema/01-core-tables.sql
- database/schema/02-indexes-and-seed.sql
