<script setup lang="ts">
import { computed, onMounted, reactive, ref } from 'vue'
import { useRouter } from 'vue-router'
import StatusPill from '../components/StatusPill.vue'
import { fetchOrders } from '../services/ordersApi'
import { useAuthStore } from '../stores/auth'
import type { DashboardFilters, OrderSummary, OrderStatus } from '../types/oms'

const authStore = useAuthStore()
const router = useRouter()
const orders = ref<OrderSummary[]>([])
const isLoading = ref(false)
const errorMessage = ref('')
const filters = reactive<DashboardFilters>({
  status: '',
  search: '',
})

const statusOptions: Array<{ label: string; value: OrderStatus | '' }> = [
  { label: 'Todos', value: '' },
  { label: 'Pendiente', value: 'Pending' },
  { label: 'Preparando', value: 'Preparing' },
  { label: 'Enviado', value: 'Shipped' },
  { label: 'Entregado', value: 'Delivered' },
  { label: 'Cancelado', value: 'Cancelled' },
]

const stats = computed(() => ({
  total: orders.value.length,
  pending: orders.value.filter((order: OrderSummary) => order.status === 'Pending').length,
  preparing: orders.value.filter((order: OrderSummary) => order.status === 'Preparing').length,
  shipped: orders.value.filter((order: OrderSummary) => order.status === 'Shipped').length,
}))

const shippedRatio = computed(() => {
  if (stats.value.total === 0) {
    return 0
  }

  return Math.round(((stats.value.shipped + stats.value.preparing) / stats.value.total) * 100)
})

const attentionCount = computed(() => stats.value.pending + stats.value.preparing)

async function loadOrders() {
  isLoading.value = true
  errorMessage.value = ''

  try {
    orders.value = await fetchOrders(authStore.token, filters)
  } catch (error) {
    errorMessage.value = error instanceof Error ? error.message : 'No fue posible cargar las ordenes.'
  } finally {
    isLoading.value = false
  }
}

function openOrder(orderId: string) {
  router.push({ name: 'order-detail', params: { orderId } })
}

function signOut() {
  authStore.signOut()
  router.push({ name: 'login' })
}

onMounted(loadOrders)
</script>

<template>
  <section class="dashboard-page">
    <header class="page-header dashboard-hero">
      <div class="hero-copy">
        <p class="eyebrow">OMS Control Tower</p>
        <h1>Un centro operativo con foco en ritmo, stock y despacho.</h1>
        <p class="lede">Visualiza el pulso del fulfillment, detecta fricciones antes del corte logístico y actúa sobre las órdenes con una vista más ejecutiva.</p>

        <div class="hero-inline-metrics">
          <div>
            <span>Backlog activo</span>
            <strong>{{ attentionCount }}</strong>
          </div>
          <div>
            <span>Órdenes en curso</span>
            <strong>{{ shippedRatio }}%</strong>
          </div>
          <div>
            <span>Vista actual</span>
            <strong>Tiempo real</strong>
          </div>
        </div>
      </div>

      <div class="hero-panel">
        <div class="hero-panel-top">
          <div class="user-chip user-chip-hero">
            <span>{{ authStore.username }}</span>
            <small>{{ authStore.role }}</small>
          </div>
          <div class="header-actions">
            <Button label="Nuevo envío" icon="pi pi-send" @click="router.push({ name: 'shipment-create' })" />
            <Button v-if="authStore.role === 'Admin'" label="Carriers" severity="secondary" outlined icon="pi pi-truck" @click="router.push({ name: 'carriers-settings' })" />
            <Button v-if="authStore.role === 'Admin'" label="Tipos cliente" severity="secondary" outlined icon="pi pi-users" @click="router.push({ name: 'customer-types-settings' })" />
            <Button v-if="authStore.role === 'Admin'" label="Tarifas" severity="secondary" outlined icon="pi pi-cog" @click="router.push({ name: 'shipping-pricing-settings' })" />
            <Button label="Salir" severity="secondary" outlined icon="pi pi-sign-out" @click="signOut" />
          </div>
        </div>

        <div class="hero-highlight">
          <span class="hero-highlight-label">Pulso operativo</span>
          <strong>{{ stats.total }}</strong>
          <p>{{ stats.pending }} pendientes, {{ stats.preparing }} preparando y {{ stats.shipped }} en tránsito.</p>
        </div>

        <div class="hero-panel-footer">
          <span class="signal-dot"></span>
          <span>Conectado a órdenes y shipping local</span>
        </div>
      </div>
    </header>

    <div class="stats-grid">
      <Card class="metric-card metric-card-primary">
        <template #content>
          <p class="stat-label">Órdenes visibles</p>
          <strong>{{ stats.total }}</strong>
          <small class="stat-footnote">Tablero filtrado según tu vista actual</small>
        </template>
      </Card>
      <Card class="metric-card">
        <template #content>
          <p class="stat-label">Pendientes</p>
          <strong>{{ stats.pending }}</strong>
          <small class="stat-footnote">Requieren asignación o confirmación</small>
        </template>
      </Card>
      <Card class="metric-card">
        <template #content>
          <p class="stat-label">Preparando</p>
          <strong>{{ stats.preparing }}</strong>
          <small class="stat-footnote">Stock reservado y picking en curso</small>
        </template>
      </Card>
      <Card class="metric-card metric-card-accent">
        <template #content>
          <p class="stat-label">Enviadas</p>
          <strong>{{ stats.shipped }}</strong>
          <small class="stat-footnote">Órdenes ya moviéndose en carrier</small>
        </template>
      </Card>
    </div>

    <Card class="filters-card command-card">
      <template #content>
        <div class="section-heading">
          <div>
            <p class="eyebrow eyebrow-inline">Centro de consulta</p>
            <h2>Filtra el flujo operativo</h2>
          </div>
          <span class="section-badge">Live search</span>
        </div>

        <div class="filters-row">
          <span class="p-input-icon-left fluid-field">
            <i class="pi pi-search"></i>
            <InputText v-model="filters.search" placeholder="Buscar por cliente, ciudad o ID" />
          </span>
          <Dropdown v-model="filters.status" :options="statusOptions" option-label="label" option-value="value" placeholder="Estado" class="status-dropdown" />
          <Button label="Aplicar filtros" icon="pi pi-filter" @click="loadOrders" />
        </div>
      </template>
    </Card>

    <Message v-if="errorMessage" severity="error" :closable="false">{{ errorMessage }}</Message>

    <Card class="table-card">
      <template #content>
        <div class="section-heading table-heading">
          <div>
            <p class="eyebrow eyebrow-inline">Monitoreo</p>
            <h2>Órdenes activas</h2>
          </div>
          <span class="table-meta">{{ orders.length }} registros</span>
        </div>

        <DataTable :value="orders" data-key="id" :loading="isLoading" striped-rows paginator :rows="8" responsive-layout="scroll">
          <Column field="id" header="Order ID">
            <template #body="slotProps">
              <button class="link-button" @click="openOrder(slotProps.data.id)">{{ slotProps.data.id.slice(0, 8) }}...</button>
            </template>
          </Column>
          <Column field="customer" header="Cliente" />
          <Column field="customerTypeName" header="Tipo cliente" />
          <Column field="status" header="Estado">
            <template #body="slotProps">
              <StatusPill :status="slotProps.data.status" />
            </template>
          </Column>
          <Column field="origin" header="Origen" />
          <Column field="assignedWarehouse" header="Warehouse" />
          <Column field="destinationCity" header="Destino" />
          <Column field="total" header="Total">
            <template #body="slotProps">
              {{ new Intl.NumberFormat('es-AR', { style: 'currency', currency: 'ARS' }).format(slotProps.data.total) }}
            </template>
          </Column>
          <Column field="timestamp" header="Fecha">
            <template #body="slotProps">
              {{ new Date(slotProps.data.timestamp).toLocaleString('es-AR') }}
            </template>
          </Column>
        </DataTable>
      </template>
    </Card>
  </section>
</template>
