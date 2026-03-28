import type { AuthSession, CarrierRecord, CreateShipmentPayload, CustomerTypeRecord, OrderDetail, OrderSummary, PostalCodePriceListRecord, PostalCodeRecord, ShipmentPricingQuote, ShipmentPricingSettings, ShipmentRecord, ShippingLabel } from '../types/oms'

const mockCustomerTypes: CustomerTypeRecord[] = [
  {
    id: '7f3fbc62-b77f-4d2e-9c4c-000000000001',
    code: 'STANDARD',
    name: 'Standard',
    description: 'Clientes estándar con tarifa general.',
    assignedPriceListName: 'Lista General',
    insuranceRatePercentage: 2,
    isActive: true,
  },
  {
    id: '7f3fbc62-b77f-4d2e-9c4c-000000000002',
    code: 'PREMIUM',
    name: 'Premium',
    description: 'Clientes preferenciales con acuerdos comerciales específicos.',
    assignedPriceListName: 'Lista Premium',
    insuranceRatePercentage: 1.5,
    isActive: true,
  },
  {
    id: '7f3fbc62-b77f-4d2e-9c4c-000000000003',
    code: 'WHOLESALE',
    name: 'Mayorista',
    description: 'Clientes mayoristas o cuentas corporativas de alto volumen.',
    assignedPriceListName: 'Lista Mayorista',
    insuranceRatePercentage: 1,
    isActive: true,
  },
]

const mockCarriers: CarrierRecord[] = [
  {
    id: '6c1a2f12-0c19-4f23-9fb2-000000000001',
    code: 'ANDREANI',
    name: 'Andreani',
    serviceLevel: 'Standard',
    trackingUrlTemplate: 'https://www.andreani.com/#!/informacionEnvio/{trackingNumber}',
    supportEmail: 'soporte@andreani.com',
    supportPhone: '+54 800 122 1111',
    insuranceSupported: true,
    isActive: true,
    notes: 'Carrier principal para operación nacional.',
  },
  {
    id: '6c1a2f12-0c19-4f23-9fb2-000000000002',
    code: 'OCA',
    name: 'OCA',
    serviceLevel: 'Express',
    trackingUrlTemplate: 'https://www.oca.com.ar/Busquedas/Envios?numero={trackingNumber}',
    supportEmail: 'clientes@oca.com.ar',
    supportPhone: '+54 800 999 7700',
    insuranceSupported: true,
    isActive: true,
    notes: 'Carrier alternativo para zonas urbanas.',
  },
  {
    id: '6c1a2f12-0c19-4f23-9fb2-000000000003',
    code: 'CORREOAR',
    name: 'Correo Argentino',
    serviceLevel: 'Economy',
    trackingUrlTemplate: 'https://www.correoargentino.com.ar/formularios/e-commerce?id={trackingNumber}',
    supportEmail: 'empresas@correoargentino.com.ar',
    supportPhone: '+54 11 4891 9191',
    insuranceSupported: false,
    isActive: true,
    notes: 'Cobertura nacional con costo moderado.',
  },
]

const mockPostalCodes: PostalCodeRecord[] = [
  {
    id: '8a5ab5d2-f35d-4ef5-9775-000000000001',
    country: 'Argentina',
    province: 'Buenos Aires',
    locality: 'Buenos Aires',
    postalCode: '1000',
    isActive: true,
    zone: 'AMBA',
  },
  {
    id: '8a5ab5d2-f35d-4ef5-9775-000000000004',
    country: 'Argentina',
    province: 'Buenos Aires',
    locality: 'La Plata',
    postalCode: '1900',
    isActive: true,
    zone: 'AMBA',
  },
  {
    id: '8a5ab5d2-f35d-4ef5-9775-000000000003',
    country: 'Argentina',
    province: 'Santa Fe',
    locality: 'Rosario',
    postalCode: '2000',
    isActive: true,
    zone: 'Litoral',
  },
  {
    id: '8a5ab5d2-f35d-4ef5-9775-000000000002',
    country: 'Argentina',
    province: 'Mendoza',
    locality: 'Mendoza',
    postalCode: '5500',
    isActive: true,
    zone: 'Centro',
  },
]

const mockPostalCodePriceLists: PostalCodePriceListRecord[] = [
  { id: 'c6dbfa0b-35e7-48f2-bf80-000000000001', listName: 'Lista General', postalCode: '1000', zone: 'AMBA', value: 9.5 },
  { id: 'c6dbfa0b-35e7-48f2-bf80-000000000002', listName: 'Lista General', postalCode: '1900', zone: 'AMBA', value: 11.2 },
  { id: 'c6dbfa0b-35e7-48f2-bf80-000000000003', listName: 'Lista General', postalCode: '2000', zone: 'Litoral', value: 13.2 },
  { id: 'c6dbfa0b-35e7-48f2-bf80-000000000004', listName: 'Lista General', postalCode: '5500', zone: 'Centro', value: 15.75 },
  { id: 'c6dbfa0b-35e7-48f2-bf80-000000000005', listName: 'Lista Premium', postalCode: '1000', zone: 'AMBA', value: 8.9 },
  { id: 'c6dbfa0b-35e7-48f2-bf80-000000000006', listName: 'Lista Premium', postalCode: '1900', zone: 'AMBA', value: 10.5 },
  { id: 'c6dbfa0b-35e7-48f2-bf80-000000000007', listName: 'Lista Premium', postalCode: '2000', zone: 'Litoral', value: 12 },
  { id: 'c6dbfa0b-35e7-48f2-bf80-000000000008', listName: 'Lista Premium', postalCode: '5500', zone: 'Centro', value: 14.25 },
]

const mockOrders: OrderDetail[] = [
  {
    id: 'c71ab4d9-09a9-4dc0-bf89-e7614ed4b801',
    customer: 'Distribuidora Norte',
    customerTypeId: '7f3fbc62-b77f-4d2e-9c4c-000000000001',
    customerTypeCode: 'STANDARD',
    customerTypeName: 'Standard',
    status: 'Preparing',
    origin: 'Web',
    total: 683.6,
    timestamp: '2026-03-27T09:20:00Z',
    assignedWarehouse: 'Cordoba Fulfillment',
    destinationCity: 'Mendoza',
    destinationState: 'Mendoza',
    destinationPostalCode: '5500',
    shipmentTrackingNumber: 'TRK-20260327-48291',
    items: [
      { sku: 'SKU-CHAIR-001', quantity: 4, unitPrice: 149.9, subtotal: 599.6 },
      { sku: 'SKU-LABEL-003', quantity: 20, unitPrice: 4.2, subtotal: 84 },
    ],
    logs: [
      {
        timestamp: '2026-03-27T10:00:00Z',
        event: 'WarehouseAssigned',
        details: 'Asignado a Cordoba Fulfillment por disponibilidad y cercania.',
      },
      {
        timestamp: '2026-03-27T09:40:00Z',
        event: 'StockReserved',
        details: 'Stock reservado para los SKU comprometidos.',
      },
      {
        timestamp: '2026-03-27T09:20:00Z',
        event: 'OrderCreated',
        details: 'Orden ingresada desde canal Web.',
      },
    ],
  },
  {
    id: '85a2c699-96e2-43b5-b0f0-f5d8da5a7921',
    customer: 'Marketplace Center',
    customerTypeId: '7f3fbc62-b77f-4d2e-9c4c-000000000002',
    customerTypeCode: 'PREMIUM',
    customerTypeName: 'Premium',
    status: 'Pending',
    origin: 'Marketplace',
    total: 1220,
    timestamp: '2026-03-27T11:45:00Z',
    assignedWarehouse: 'Buenos Aires Hub',
    destinationCity: 'La Plata',
    destinationState: 'Buenos Aires',
    destinationPostalCode: '1900',
    shipmentTrackingNumber: null,
    items: [
      { sku: 'SKU-DESK-002', quantity: 5, unitPrice: 244, subtotal: 1220 },
    ],
    logs: [
      {
        timestamp: '2026-03-27T11:55:00Z',
        event: 'StockReserved',
        details: 'Reserva creada con control de stock disponible.',
      },
      {
        timestamp: '2026-03-27T11:45:00Z',
        event: 'OrderCreated',
        details: 'Webhook de marketplace procesado exitosamente.',
      },
    ],
  },
]

export function getMockSession(username: string): AuthSession {
  return {
    accessToken: 'mock-oms-token',
    expiresIn: 28800,
    user: {
      username,
      role: username === 'admin' ? 'Admin' : 'Operator',
    },
  }
}

export function getMockOrders(): OrderSummary[] {
  return mockOrders.map(({ items, logs, destinationState, destinationPostalCode, shipmentTrackingNumber, ...summary }) => summary)
}

export function getMockOrderById(orderId: string): OrderDetail | undefined {
  return mockOrders.find((order: OrderDetail) => order.id === orderId)
}

export function getMockLabel(orderId: string): ShippingLabel {
  const order = getMockOrderById(orderId)

  return {
    orderId,
    format: 'PLAINTEXT',
    content: [
      'OMS SHIPPING LABEL',
      `Order: ${orderId}`,
      `Customer: ${order?.customer ?? 'N/A'}`,
      `Warehouse: ${order?.assignedWarehouse ?? 'N/A'}`,
      `Tracking: ${order?.shipmentTrackingNumber ?? 'PENDING'}`,
      `Destination: ${order?.destinationCity ?? 'N/A'} ${order?.destinationPostalCode ?? ''}`,
    ].join('\n'),
  }
}

export function getMockShipmentPricingSettings(): ShipmentPricingSettings {
  return {
    priceLists: getMockPostalCodePriceLists(),
  }
}

export function getMockCustomerTypes(includeInactive = false): CustomerTypeRecord[] {
  return includeInactive ? [...mockCustomerTypes] : mockCustomerTypes.filter((customerType) => customerType.isActive)
}

export function getMockCustomerTypeById(customerTypeId: string): CustomerTypeRecord | undefined {
  return mockCustomerTypes.find((customerType) => customerType.id === customerTypeId)
}

export function getMockCarriers(includeInactive = false): CarrierRecord[] {
  return includeInactive ? [...mockCarriers] : mockCarriers.filter((carrier) => carrier.isActive)
}

export function getMockCarrierById(carrierId: string): CarrierRecord | undefined {
  return mockCarriers.find((carrier) => carrier.id === carrierId)
}

export function getMockPostalCodes(includeInactive = false): PostalCodeRecord[] {
  return includeInactive ? [...mockPostalCodes] : mockPostalCodes.filter((postalCode) => postalCode.isActive)
}

export function getMockPostalCodeById(postalCodeId: string): PostalCodeRecord | undefined {
  return mockPostalCodes.find((postalCode) => postalCode.id === postalCodeId)
}

export function getMockPostalCodePriceLists(): PostalCodePriceListRecord[] {
  return [...mockPostalCodePriceLists]
}

export function getMockPostalCodePriceListById(postalCodePriceListId: string): PostalCodePriceListRecord | undefined {
  return mockPostalCodePriceLists.find((priceList) => priceList.id === postalCodePriceListId)
}

export function getMockShipmentPricingQuote(customerTypeId: string, carrierId: string, destinationPostalCode: string, declaredValue: number, includeInsurance = true): ShipmentPricingQuote {
  const settings = getMockShipmentPricingSettings()
  const normalized = destinationPostalCode.replace(/[^a-z0-9]/gi, '').toUpperCase()
  const customerType = getMockCustomerTypeById(customerTypeId)
  const carrier = getMockCarrierById(carrierId)
  const match = settings.priceLists.find((priceList) =>
    priceList.listName === customerType?.assignedPriceListName && priceList.postalCode === normalized,
  )

  const baseShippingCost = match?.value ?? 0
  const insuranceCost = includeInsurance
    ? Math.round(declaredValue * ((customerType?.insuranceRatePercentage ?? 0) / 100) * 100) / 100
    : 0

  return {
    customerTypeId,
    customerTypeCode: customerType?.code ?? 'STANDARD',
    customerTypeName: customerType?.name ?? 'Standard',
    carrierId,
    carrierName: carrier?.name ?? 'Carrier no definido',
    destinationPostalCode: normalized,
    assignedPriceListName: customerType?.assignedPriceListName ?? '',
    matchedZone: match?.zone ?? '',
    insuranceRatePercentage: customerType?.insuranceRatePercentage ?? 0,
    declaredValue,
    baseShippingCost,
    insuranceCost,
    totalShippingCost: baseShippingCost + insuranceCost,
  }
}

export function getMockCreatedShipment(payload: CreateShipmentPayload): ShipmentRecord {
  const order = getMockOrderById(payload.orderId)
  const carrier = getMockCarrierById(payload.carrierId)
  const quote = getMockShipmentPricingQuote(order?.customerTypeId ?? '7f3fbc62-b77f-4d2e-9c4c-000000000001', payload.carrierId, order?.destinationPostalCode ?? '1000', order?.total ?? 0, payload.includeInsurance)

  return {
    id: `mock-shipment-${payload.orderId}`,
    orderId: payload.orderId,
    carrierId: payload.carrierId,
    customer: payload.customer,
    customerTypeId: order?.customerTypeId ?? '7f3fbc62-b77f-4d2e-9c4c-000000000001',
    customerTypeCode: order?.customerTypeCode ?? 'STANDARD',
    customerTypeName: order?.customerTypeName ?? 'Standard',
    recipientName: payload.recipientName,
    recipientPhone: payload.recipientPhone,
    recipientEmail: payload.recipientEmail,
    carrier: carrier?.name ?? 'Carrier no definido',
    trackingNumber: `TRK-MOCK-${payload.orderId.slice(0, 8).toUpperCase()}`,
    status: 'LabelCreated',
    weightKg: payload.weightKg,
    heightCm: payload.heightCm,
    widthCm: payload.widthCm,
    lengthCm: payload.lengthCm,
    declaredMerchandiseValue: order?.total ?? 0,
    baseShippingCost: quote.baseShippingCost,
    insuranceCost: quote.insuranceCost,
    shippingCost: quote.totalShippingCost,
    appliedPriceListName: quote.assignedPriceListName,
    appliedZone: quote.matchedZone,
    destinationPostalCode: order?.destinationPostalCode ?? '1001',
    destinationAddress: payload.destinationAddress,
    events: [
      {
        timestamp: new Date().toISOString(),
        status: 'LabelCreated',
        notes: 'Envío simulado creado desde el dashboard.',
      },
    ],
  }
}
