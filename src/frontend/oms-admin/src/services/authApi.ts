import axios from 'axios'
import { getMockSession } from './mockData'
import type { AuthSession, LoginPayload } from '../types/oms'

const ORDERS_API_BASE_URL = import.meta.env.VITE_ORDERS_API_BASE_URL ?? 'https://localhost:7001'

export async function login(payload: LoginPayload): Promise<AuthSession> {
  try {
    const response = await axios.post<AuthSession>(`${ORDERS_API_BASE_URL}/api/auth/token`, payload, {
      timeout: 5000,
    })

    return response.data
  } catch {
    if (
      (payload.username === 'admin' && payload.password === 'OmsAdmin123!') ||
      (payload.username === 'operator' && payload.password === 'OmsOperator123!')
    ) {
      return getMockSession(payload.username)
    }

    throw new Error('No fue posible iniciar sesion. Verifica las credenciales o levanta Orders.Api.')
  }
}
