import axios from 'axios'
import { getMockCreatedShipment, getMockLabel, getMockShipmentPricingQuote, getMockShipmentPricingSettings } from './mockData'
import type { CarrierRecord, CarrierUpsertPayload, CreateShipmentPayload, ShipmentPricingQuote, ShipmentPricingSettings, ShipmentRecord, ShippingLabel } from '../types/oms'
import { getMockCarrierById, getMockCarriers } from './mockData'

const SHIPMENTS_API_BASE_URL = import.meta.env.VITE_SHIPMENTS_API_BASE_URL ?? 'https://localhost:7003'

export async function generateShippingLabel(token: string, orderId: string): Promise<ShippingLabel> {
  try {
    const response = await axios.post<ShippingLabel>(`${SHIPMENTS_API_BASE_URL}/api/shipments/by-order/${orderId}/label`, undefined, {
      headers: {
        Authorization: `Bearer ${token}`,
      },
      timeout: 5000,
    })

    return {
      orderId,
      format: response.data.format ?? 'PLAINTEXT',
      content: response.data.content,
    }
  } catch {
    return getMockLabel(orderId)
  }
}

export async function createShipment(token: string, payload: CreateShipmentPayload): Promise<ShipmentRecord> {
  try {
    const response = await axios.post<ShipmentRecord>(`${SHIPMENTS_API_BASE_URL}/api/shipments`, payload, {
      headers: {
        Authorization: `Bearer ${token}`,
      },
      timeout: 5000,
    })

    return response.data
  } catch {
    return getMockCreatedShipment(payload)
  }
}

export async function fetchCarriers(token: string, includeInactive = false): Promise<CarrierRecord[]> {
  try {
    const response = await axios.get<CarrierRecord[]>(`${SHIPMENTS_API_BASE_URL}/api/carriers`, {
      params: { includeInactive },
      headers: {
        Authorization: `Bearer ${token}`,
      },
      timeout: 5000,
    })

    return response.data
  } catch {
    return getMockCarriers(includeInactive)
  }
}

export async function createCarrier(token: string, payload: CarrierUpsertPayload): Promise<CarrierRecord> {
  try {
    const response = await axios.post<CarrierRecord>(`${SHIPMENTS_API_BASE_URL}/api/carriers`, payload, {
      headers: {
        Authorization: `Bearer ${token}`,
      },
      timeout: 5000,
    })

    return response.data
  } catch {
    return {
      id: crypto.randomUUID(),
      ...payload,
    }
  }
}

export async function updateCarrier(token: string, carrierId: string, payload: CarrierUpsertPayload): Promise<CarrierRecord> {
  try {
    const response = await axios.put<CarrierRecord>(`${SHIPMENTS_API_BASE_URL}/api/carriers/${carrierId}`, payload, {
      headers: {
        Authorization: `Bearer ${token}`,
      },
      timeout: 5000,
    })

    return response.data
  } catch {
    return {
      ...(getMockCarrierById(carrierId) ?? { id: carrierId }),
      ...payload,
    } as CarrierRecord
  }
}

export async function fetchShipmentPricingSettings(token: string): Promise<ShipmentPricingSettings> {
  try {
    const response = await axios.get<ShipmentPricingSettings>(`${SHIPMENTS_API_BASE_URL}/api/shipment-pricing/settings`, {
      headers: {
        Authorization: `Bearer ${token}`,
      },
      timeout: 5000,
    })

    return response.data
  } catch {
    return getMockShipmentPricingSettings()
  }
}

export async function updateShipmentPricingSettings(token: string, payload: ShipmentPricingSettings): Promise<ShipmentPricingSettings> {
  try {
    const response = await axios.put<ShipmentPricingSettings>(`${SHIPMENTS_API_BASE_URL}/api/shipment-pricing/settings`, payload, {
      headers: {
        Authorization: `Bearer ${token}`,
      },
      timeout: 5000,
    })

    return response.data
  } catch {
    return payload
  }
}

export async function calculateShipmentQuote(token: string, customerTypeId: string, carrierId: string, destinationPostalCode: string, includeInsurance = true): Promise<ShipmentPricingQuote> {
  try {
    const response = await axios.post<ShipmentPricingQuote>(`${SHIPMENTS_API_BASE_URL}/api/shipment-pricing/quote`, {
      customerTypeId,
      carrierId,
      destinationPostalCode,
      includeInsurance,
    }, {
      headers: {
        Authorization: `Bearer ${token}`,
      },
      timeout: 5000,
    })

    return response.data
  } catch {
    return getMockShipmentPricingQuote(customerTypeId, carrierId, destinationPostalCode, includeInsurance)
  }
}
