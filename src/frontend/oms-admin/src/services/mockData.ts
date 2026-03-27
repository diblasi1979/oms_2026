import type { AuthSession, OrderDetail, OrderSummary, ShippingLabel } from '../types/oms'

const mockOrders: OrderDetail[] = [
  {
    id: 'c71ab4d9-09a9-4dc0-bf89-e7614ed4b801',
    customer: 'Distribuidora Norte',
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
