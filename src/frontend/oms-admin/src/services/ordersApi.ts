import axios from 'axios'
import { getMockCustomerById, getMockCustomerTypeById, getMockCustomerTypes, getMockCustomers, getMockOrderById, getMockOrders } from './mockData'
import type { CustomerRecord, CustomerTypeRecord, CustomerTypeUpsertPayload, CustomerUpsertPayload, DashboardFilters, OrderDetail, OrderSummary } from '../types/oms'

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

export async function fetchCustomers(token: string, includeInactive = false, search = ''): Promise<CustomerRecord[]> {
  try {
    const response = await axios.get<CustomerRecord[]>(`${ORDERS_API_BASE_URL}/api/customers`, {
      params: {
        includeInactive,
        search: search || undefined,
      },
      headers: {
        Authorization: `Bearer ${token}`,
      },
      timeout: 5000,
    })

    return response.data
  } catch {
    const normalizedSearch = search.trim().toLowerCase()
    return getMockCustomers(includeInactive).filter((customer) => {
      if (!normalizedSearch) {
        return true
      }

      return customer.name.toLowerCase().includes(normalizedSearch) || customer.code.toLowerCase().includes(normalizedSearch)
    })
  }
}

export async function createCustomer(token: string, payload: CustomerUpsertPayload): Promise<CustomerRecord> {
  try {
    const response = await axios.post<CustomerRecord>(`${ORDERS_API_BASE_URL}/api/customers`, payload, {
      headers: {
        Authorization: `Bearer ${token}`,
      },
      timeout: 5000,
    })

    return response.data
  } catch {
    const customerType = getMockCustomerTypeById(payload.customerTypeId)
    return {
      id: crypto.randomUUID(),
      code: payload.code,
      name: payload.name,
      customerTypeId: payload.customerTypeId,
      customerTypeCode: customerType?.code ?? '',
      customerTypeName: customerType?.name ?? '',
      assignedPriceListName: payload.assignedPriceListName,
      insuranceRatePercentage: payload.insuranceRatePercentage,
      isActive: payload.isActive,
    }
  }
}

export async function updateCustomer(token: string, customerId: string, payload: CustomerUpsertPayload): Promise<CustomerRecord> {
  try {
    const response = await axios.put<CustomerRecord>(`${ORDERS_API_BASE_URL}/api/customers/${customerId}`, payload, {
      headers: {
        Authorization: `Bearer ${token}`,
      },
      timeout: 5000,
    })

    return response.data
  } catch {
    const customerType = getMockCustomerTypeById(payload.customerTypeId)
    return {
      ...(getMockCustomerById(customerId) ?? { id: customerId }),
      code: payload.code,
      name: payload.name,
      customerTypeId: payload.customerTypeId,
      customerTypeCode: customerType?.code ?? '',
      customerTypeName: customerType?.name ?? '',
      assignedPriceListName: payload.assignedPriceListName,
      insuranceRatePercentage: payload.insuranceRatePercentage,
      isActive: payload.isActive,
    } as CustomerRecord
  }
}
