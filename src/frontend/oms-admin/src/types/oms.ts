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

export interface CreateShipmentPayload {
  orderId: string
  customer: string
  carrierId: string
  recipientName: string
  recipientPhone: string
  recipientEmail: string
  destinationAddress: string
  weightKg: number
  heightCm: number
  widthCm: number
  lengthCm: number
  includeInsurance: boolean
}

export interface ShipmentEvent {
  timestamp: string
  status: string
  notes: string
}

export interface ShipmentRecord {
  id: string
  orderId: string
  carrierId?: string | null
  customer: string
  recipientName: string
  recipientPhone: string
  recipientEmail: string
  carrier: string
  trackingNumber: string
  status: string
  weightKg: number
  heightCm: number
  widthCm: number
  lengthCm: number
  baseShippingCost: number
  insuranceCost: number
  shippingCost: number
  destinationPostalCode: string
  destinationAddress: string
  events: ShipmentEvent[]
}

export interface CarrierRecord {
  id: string
  code: string
  name: string
  serviceLevel: string
  trackingUrlTemplate: string
  supportEmail: string
  supportPhone: string
  insuranceSupported: boolean
  isActive: boolean
  notes: string
}

export interface CarrierUpsertPayload {
  code: string
  name: string
  serviceLevel: string
  trackingUrlTemplate: string
  supportEmail: string
  supportPhone: string
  insuranceSupported: boolean
  isActive: boolean
  notes: string
}

export interface ShipmentPricingRule {
  id?: string
  ruleName: string
  postalCodePrefix: string
  baseCost: number
}

export interface ShipmentPricingSettings {
  defaultBaseCost: number
  insuranceFlatCost: number
  rules: ShipmentPricingRule[]
}

export interface ShipmentPricingQuote {
  destinationPostalCode: string
  matchedRuleName?: string | null
  matchedPostalCodePrefix?: string | null
  usedDefaultRate: boolean
  baseShippingCost: number
  insuranceCost: number
  totalShippingCost: number
}
