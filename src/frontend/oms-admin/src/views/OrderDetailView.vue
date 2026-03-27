<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import StatusPill from '../components/StatusPill.vue'
import { fetchOrderDetail } from '../services/ordersApi'
import { generateShippingLabel } from '../services/shippingApi'
import { useAuthStore } from '../stores/auth'
import type { OrderDetail, ShippingLabel } from '../types/oms'

const authStore = useAuthStore()
const route = useRoute()
const router = useRouter()
const order = ref<OrderDetail | null>(null)
const label = ref<ShippingLabel | null>(null)
const isLoading = ref(false)
const isGeneratingLabel = ref(false)
const errorMessage = ref('')

const totalUnits = computed(() => order.value?.items.reduce((accumulator: number, item) => accumulator + item.quantity, 0) ?? 0)

async function loadOrder() {
  isLoading.value = true
  errorMessage.value = ''

  try {
    order.value = await fetchOrderDetail(authStore.token, route.params.orderId as string)
  } catch (error) {
    errorMessage.value = error instanceof Error ? error.message : 'No fue posible cargar el detalle de la orden.'
  } finally {
    isLoading.value = false
  }
}

async function createLabel() {
  if (!order.value) {
    return
  }

  isGeneratingLabel.value = true

  try {
    label.value = await generateShippingLabel(authStore.token, order.value.id)
  } finally {
    isGeneratingLabel.value = false
  }
}

onMounted(loadOrder)
</script>

<template>
  <section class="detail-page">
    <div class="detail-toolbar">
      <Button label="Volver" icon="pi pi-arrow-left" text @click="router.push({ name: 'orders' })" />
      <Button label="Generar etiqueta" icon="pi pi-tag" :loading="isGeneratingLabel" @click="createLabel" />
    </div>

    <ProgressSpinner v-if="isLoading" stroke-width="4" class="center-spinner" />
    <Message v-else-if="errorMessage" severity="error" :closable="false">{{ errorMessage }}</Message>

    <template v-else-if="order">
      <header class="detail-header">
        <div class="detail-hero-copy">
          <p class="eyebrow">Detalle de orden</p>
          <h1>{{ order.customer }}</h1>
          <p class="lede">{{ order.id }} · {{ order.destinationCity }}, {{ order.destinationState }}</p>
        </div>
        <div class="detail-hero-side">
          <StatusPill :status="order.status" />
          <div class="detail-kicker">
            <span>Total orden</span>
            <strong>{{ new Intl.NumberFormat('es-AR', { style: 'currency', currency: 'ARS' }).format(order.total) }}</strong>
          </div>
        </div>
      </header>

      <div class="detail-grid">
        <Card class="panel-card">
          <template #title>Resumen</template>
          <template #content>
            <div class="summary-grid">
              <div>
                <span>Origen</span>
                <strong>{{ order.origin }}</strong>
              </div>
              <div>
                <span>Warehouse asignado</span>
                <strong>{{ order.assignedWarehouse }}</strong>
              </div>
              <div>
                <span>Total</span>
                <strong>{{ new Intl.NumberFormat('es-AR', { style: 'currency', currency: 'ARS' }).format(order.total) }}</strong>
              </div>
              <div>
                <span>Unidades</span>
                <strong>{{ totalUnits }}</strong>
              </div>
              <div>
                <span>Tracking</span>
                <strong>{{ order.shipmentTrackingNumber || 'Pendiente de asignacion' }}</strong>
              </div>
              <div>
                <span>CP destino</span>
                <strong>{{ order.destinationPostalCode }}</strong>
              </div>
            </div>
          </template>
        </Card>

        <Card class="panel-card">
          <template #title>Items</template>
          <template #content>
            <DataTable :value="order.items" data-key="sku" responsive-layout="scroll">
              <Column field="sku" header="SKU" />
              <Column field="quantity" header="Cantidad" />
              <Column field="unitPrice" header="Precio unitario">
                <template #body="slotProps">
                  {{ new Intl.NumberFormat('es-AR', { style: 'currency', currency: 'ARS' }).format(slotProps.data.unitPrice) }}
                </template>
              </Column>
              <Column field="subtotal" header="Subtotal">
                <template #body="slotProps">
                  {{ new Intl.NumberFormat('es-AR', { style: 'currency', currency: 'ARS' }).format(slotProps.data.subtotal) }}
                </template>
              </Column>
            </DataTable>
          </template>
        </Card>

        <Card class="panel-card detail-span-2">
          <template #title>Historial de movimientos</template>
          <template #content>
            <Timeline :value="order.logs" align="alternate" class="movement-timeline">
              <template #content="slotProps">
                <div class="timeline-card">
                  <strong>{{ slotProps.item.event }}</strong>
                  <p>{{ slotProps.item.details }}</p>
                  <small>{{ new Date(slotProps.item.timestamp).toLocaleString('es-AR') }}</small>
                </div>
              </template>
            </Timeline>
          </template>
        </Card>

        <Card v-if="label" class="panel-card detail-span-2">
          <template #title>Etiqueta simulada</template>
          <template #content>
            <pre class="label-preview">{{ label.content }}</pre>
          </template>
        </Card>
      </div>
    </template>
  </section>
</template>
