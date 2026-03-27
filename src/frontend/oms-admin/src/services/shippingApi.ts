import axios from 'axios'
import { getMockLabel } from './mockData'
import type { ShippingLabel } from '../types/oms'

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
