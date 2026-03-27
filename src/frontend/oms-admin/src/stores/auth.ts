import { computed, ref } from 'vue'
import { defineStore } from 'pinia'
import { login } from '../services/authApi'
import type { AuthSession, LoginPayload } from '../types/oms'

const STORAGE_KEY = 'oms-admin-session'

export const useAuthStore = defineStore('auth', () => {
  const session = ref<AuthSession | null>(readInitialSession())
  const isSubmitting = ref(false)

  const isAuthenticated = computed(() => Boolean(session.value?.accessToken))
  const token = computed(() => session.value?.accessToken ?? '')
  const username = computed(() => session.value?.user.username ?? '')
  const role = computed(() => session.value?.user.role ?? '')

  async function signIn(payload: LoginPayload) {
    isSubmitting.value = true

    try {
      session.value = await login(payload)
      localStorage.setItem(STORAGE_KEY, JSON.stringify(session.value))
    } finally {
      isSubmitting.value = false
    }
  }

  function signOut() {
    session.value = null
    localStorage.removeItem(STORAGE_KEY)
  }

  return {
    isSubmitting,
    isAuthenticated,
    role,
    session,
    signIn,
    signOut,
    token,
    username,
  }
})

function readInitialSession(): AuthSession | null {
  const raw = localStorage.getItem(STORAGE_KEY)
  if (!raw) {
    return null
  }

  try {
    return JSON.parse(raw) as AuthSession
  } catch {
    localStorage.removeItem(STORAGE_KEY)
    return null
  }
}
