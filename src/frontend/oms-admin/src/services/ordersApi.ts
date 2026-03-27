import axios from 'axios'
import { getMockOrderById, getMockOrders } from './mockData'
import type { DashboardFilters, OrderDetail, OrderSummary } from '../types/oms'

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
