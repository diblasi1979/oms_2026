# OMS 2026

![CI](https://github.com/diblasi1979/oms_2026/actions/workflows/ci.yml/badge.svg)

OMS 2026 es una base escalable para operación logística y gestión de pedidos, diseñada para centralizar órdenes, disponibilidad de inventario, generación de envíos e integraciones externas desde una arquitectura de servicios desacoplados.

El proyecto combina ASP.NET Core para APIs de dominio, SQL Server con EF Core para persistencia y Vue 3 para un dashboard operativo con foco visual en monitoreo, stock reservado y trazabilidad de fulfillment.

## Propuesta de valor

- Arquitectura orientada a servicios para Orders, Inventory, Shipments e Integrations.
- Dashboard administrativo en Vue 3 con look & feel oscuro, más ejecutivo y listo para operación interna.
- Reserva de stock con control de concurrencia vía RowVersion expuesto como token seguro.
- Generación de envíos y etiquetas simuladas para acelerar pruebas funcionales.
- Webhooks para carriers y marketplaces con normalización hacia un modelo canónico.
- Base técnica preparada para evolucionar a entornos enterprise o despliegues productivos.

## Stack tecnológico

- Backend: ASP.NET Core Web API, JWT, Swagger, EF Core.
- Base de datos: SQL Server.
- Frontend: Vue 3, TypeScript, Vite, Pinia, PrimeVue.
- CI: GitHub Actions para build automático de backend y frontend.

## Arquitectura

La solución está organizada sobre una solución .NET y un frontend independiente:

- Orders API: alta, consulta y ruteo inicial de pedidos.
- Inventory API: consulta, reserva y liberación de stock con concurrencia optimista.
- Shipments API: creación de envíos, tracking y generación de etiquetas.
- Integrations API: recepción y normalización de órdenes externas y webhooks.
- OMS Admin: panel web para login, dashboard operativo y detalle de orden.

## Estructura principal

- src/services/orders/Orders.Api
- src/services/inventory/Inventory.Api
- src/services/shipments/Shipments.Api
- src/services/integrations/Integrations.Api
- src/building-blocks/Oms.Persistence
- src/frontend/oms-admin
- database/schema
- docs

## Funcionalidades incluidas

- Creación y consulta de órdenes.
- Asignación de warehouse por disponibilidad y proximidad.
- Reserva automática de stock al crear una orden.
- Endpoints de inventory con concurrencyToken en Base64.
- Creación de envíos y etiqueta por shipment o por order.
- Historial operativo en detalle de orden.
- Dashboard con filtros, métricas y navegación al detalle.
- Integración base para Shopify, MercadoLibre, WooCommerce y Amazon.

## Credenciales demo

- admin / OmsAdmin123!
- operator / OmsOperator123!

## Arranque rápido

### Requisitos

- .NET SDK 9
- Node.js 20 o superior
- SQL Server accesible en localhost:1433

### Base de datos

1. Ejecutar database/schema/00-create-database.sql con un usuario administrador de SQL Server.
2. Ejecutar database/schema/01-core-tables.sql sobre oms_test.
3. Ejecutar database/schema/02-indexes-and-seed.sql sobre oms_test.

Configuración actual de base:

- Servidor: localhost,1433
- Base: oms_test
- Usuario: api_oms
- Clave: api2468

### Backend

1. Restaurar dependencias y compilar:

```powershell
dotnet build OMS.sln
```

2. Ejecutar los servicios necesarios:

```powershell
dotnet run --project src/services/orders/Orders.Api/Orders.Api.csproj --urls http://localhost:7011
dotnet run --project src/services/inventory/Inventory.Api/Inventory.Api.csproj --urls http://localhost:7012
dotnet run --project src/services/shipments/Shipments.Api/Shipments.Api.csproj --urls http://localhost:7013
dotnet run --project src/services/integrations/Integrations.Api/Integrations.Api.csproj --urls http://localhost:7014
```

### Frontend

1. Instalar dependencias:

```powershell
cd src/frontend/oms-admin
npm.cmd install
```

2. Ejecutar en desarrollo o compilar:

```powershell
npm.cmd run build
```

Variables opcionales del frontend:

- VITE_ORDERS_API_BASE_URL
- VITE_SHIPMENTS_API_BASE_URL

Nota: en este workspace local, la ruta contiene el carácter # y puede afectar vite dev. La build y vite preview funcionan correctamente.

## API y documentación

- Arquitectura: docs/architecture.md
- Modelo ERD: docs/erd.md
- Endpoints y overview: docs/api-overview.md
- Bootstrap SQL: database/schema/00-create-database.sql
- Core schema: database/schema/01-core-tables.sql
- Seed e índices: database/schema/02-indexes-and-seed.sql

## Calidad y CI

El repositorio incluye GitHub Actions para validar automáticamente:

- Build de la solución .NET.
- Build del frontend Vue.
- Ejecución sobre push y pull request a main.

## Próximos pasos sugeridos

- Incorporar tests automatizados por servicio.
- Agregar Docker Compose para entorno local completo.
- Publicar artefactos o imágenes para despliegue.
- Sumar observabilidad, health checks y métricas operativas.
