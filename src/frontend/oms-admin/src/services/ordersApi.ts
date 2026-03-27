import axios from 'axios'
import { getMockCustomerTypeById, getMockCustomerTypes, getMockOrderById, getMockOrders } from './mockData'
import type { CustomerTypeRecord, CustomerTypeUpsertPayload, DashboardFilters, OrderDetail, OrderSummary } from '../types/oms'

const ORDERS_API_BASE_URL = import.meta.env.VITE_ORDERS_API_BASE_URL ?? 'https://localhost:7001'

export async function fetchOrders(token: string, filters: DashboardFilters): Promise<OrderSummary[]> {
  try {
    const response = await axios.get<OrderSummary[]>(`${ORDERS_API_BASE_URL}/api/orders`, {
      params: {
        status: filters.status || undefined,
        search: filters.search || undefined,
      },
      headers: {
        Authorization: `Bearer ${token}`,
      },
      timeout: 5000,
    })

    return response.data
  } catch {
    const search = filters.search?.trim().toLowerCase()

    return getMockOrders().filter((order) => {
      const matchesStatus = filters.status ? order.status === filters.status : true
      const matchesSearch = search
        ? order.customer.toLowerCase().includes(search) ||
          order.id.toLowerCase().includes(search) ||
          order.destinationCity.toLowerCase().includes(search)
        : true

      return matchesStatus && matchesSearch
    })
  }
}

export async function fetchOrderDetail(token: string, orderId: string): Promise<OrderDetail> {
  try {
    const response = await axios.get<OrderDetail>(`${ORDERS_API_BASE_URL}/api/orders/${orderId}`, {
      headers: {
        Authorization: `Bearer ${token}`,
      },
      timeout: 5000,
    })

    return response.data
  } catch {
    const mockOrder = getMockOrderById(orderId)
    if (!mockOrder) {
      throw new Error('La orden solicitada no existe.')
    }

    return mockOrder
  }
}

export async function fetchCustomerTypes(token: string, includeInactive = false): Promise<CustomerTypeRecord[]> {
  try {
    const response = await axios.get<CustomerTypeRecord[]>(`${ORDERS_API_BASE_URL}/api/customer-types`, {
      params: { includeInactive },
      headers: {
        Authorization: `Bearer ${token}`,
      },
      timeout: 5000,
    })

    return response.data
  } catch {
    return getMockCustomerTypes(includeInactive)
  }
}

export async function createCustomerType(token: string, payload: CustomerTypeUpsertPayload): Promise<CustomerTypeRecord> {
  try {
    const response = await axios.post<CustomerTypeRecord>(`${ORDERS_API_BASE_URL}/api/customer-types`, payload, {
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

export async function updateCustomerType(token: string, customerTypeId: string, payload: CustomerTypeUpsertPayload): Promise<CustomerTypeRecord> {
  try {
    const response = await axios.put<CustomerTypeRecord>(`${ORDERS_API_BASE_URL}/api/customer-types/${customerTypeId}`, payload, {
      headers: {
        Authorization: `Bearer ${token}`,
      },
      timeout: 5000,
    })

    return response.data
  } catch {
    return {
      ...(getMockCustomerTypeById(customerTypeId) ?? { id: customerTypeId }),
      ...payload,
    } as CustomerTypeRecord
  }
}
