export type OrderStatus = 'Pending' | 'Preparing' | 'Shipped' | 'Delivered' | 'Cancelled'
export type OrderOrigin = 'Marketplace' | 'Web' | 'Manual'

export interface LoginPayload {
  username: string
  password: string
}

export interface AuthSession {
  accessToken: string
  expiresIn: number
  user: {
    username: string
    role: string
  }
}

export interface OrderSummary {
  id: string
  customer: string
  status: OrderStatus
  origin: OrderOrigin
  total: number
  timestamp: string
  assignedWarehouse: string
  destinationCity: string
}

export interface OrderItem {
  sku: string
  quantity: number
  unitPrice: number
  subtotal: number
}

export interface OrderLogEntry {
  timestamp: string
  event: string
  details: string
}

export interface OrderDetail extends OrderSummary {
  destinationState: string
  destinationPostalCode: string
  shipmentTrackingNumber?: string | null
  items: OrderItem[]
  logs: OrderLogEntry[]
}

export interface DashboardFilters {
  status?: OrderStatus | ''
  search?: string
}

export interface ShippingLabel {
  orderId: string
  format: string
  content: string
}
