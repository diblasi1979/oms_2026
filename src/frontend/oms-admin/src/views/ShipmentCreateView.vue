<script setup lang="ts">
import { computed, onMounted, reactive, ref } from 'vue'
import { useRouter } from 'vue-router'
import { fetchOrderDetail } from '../services/ordersApi'
import { calculateShipmentQuote, createShipment, fetchCarriers } from '../services/shippingApi'
import { useAuthStore } from '../stores/auth'
import type { CarrierRecord, CreateShipmentPayload, OrderDetail, ShipmentPricingQuote, ShipmentRecord } from '../types/oms'

const authStore = useAuthStore()
const router = useRouter()

const orderIdSearch = ref('')
const order = ref<OrderDetail | null>(null)
const quote = ref<ShipmentPricingQuote | null>(null)
const createdShipment = ref<ShipmentRecord | null>(null)
const carriers = ref<CarrierRecord[]>([])
const isLoadingOrder = ref(false)
const isLoadingCarriers = ref(false)
const isCalculatingQuote = ref(false)
const isSubmitting = ref(false)
const errorMessage = ref('')
const successMessage = ref('')

const carrierOptions = computed(() => carriers.value.map((carrier) => ({
  label: `${carrier.name} · ${carrier.serviceLevel}`,
  value: carrier.id,
})))

const selectedCarrier = computed(() => carriers.value.find((carrier) => carrier.id === form.carrierId) ?? null)

const form = reactive<CreateShipmentPayload>({
  orderId: '',
  customer: '',
  carrierId: '',
  recipientName: '',
  recipientPhone: '',
  recipientEmail: '',
  destinationAddress: '',
  weightKg: 1,
  heightCm: 10,
  widthCm: 20,
  lengthCm: 30,
  includeInsurance: true,
})

const formattedQuoteTotal = computed(() => {
  if (!quote.value) {
    return null
  }

  return new Intl.NumberFormat('es-AR', { style: 'currency', currency: 'ARS' }).format(quote.value.totalShippingCost)
})

const canQuote = computed(() => Boolean(order.value?.destinationPostalCode))
const canSubmit = computed(() => Boolean(form.orderId && form.customer && form.carrierId && form.recipientName && form.recipientPhone && form.destinationAddress && quote.value))

async function loadCarriers() {
  isLoadingCarriers.value = true

  try {
    carriers.value = await fetchCarriers(authStore.token)
    if (!form.carrierId && carriers.value.length > 0) {
      form.carrierId = carriers.value[0].id
    }
  } catch (error) {
    errorMessage.value = error instanceof Error ? error.message : 'No fue posible cargar los carriers.'
  } finally {
    isLoadingCarriers.value = false
  }
}

async function loadOrder() {
  if (!orderIdSearch.value.trim()) {
    errorMessage.value = 'Ingresá un ID de orden para continuar.'
    return
  }

  isLoadingOrder.value = true
  errorMessage.value = ''
  successMessage.value = ''
  createdShipment.value = null

  try {
    const loadedOrder = await fetchOrderDetail(authStore.token, orderIdSearch.value.trim())
    order.value = loadedOrder
    form.orderId = loadedOrder.id
    form.customer = loadedOrder.customer
    form.recipientName = loadedOrder.customer
    form.destinationAddress = `${loadedOrder.destinationCity}, ${loadedOrder.destinationState} (${loadedOrder.destinationPostalCode})`
    await loadQuote()
  } catch (error) {
    order.value = null
    quote.value = null
    errorMessage.value = error instanceof Error ? error.message : 'No fue posible cargar la orden.'
  } finally {
    isLoadingOrder.value = false
  }
}

async function loadQuote() {
  if (!order.value) {
    return
  }

  isCalculatingQuote.value = true
  errorMessage.value = ''

  try {
    quote.value = await calculateShipmentQuote(authStore.token, order.value.destinationPostalCode, form.includeInsurance)
  } catch (error) {
    quote.value = null
    errorMessage.value = error instanceof Error ? error.message : 'No fue posible calcular la cotización del envío.'
  } finally {
    isCalculatingQuote.value = false
  }
}

async function submitShipment() {
  if (!canSubmit.value) {
    errorMessage.value = 'Completá la orden, el formulario y la cotización antes de crear el envío.'
    return
  }

  isSubmitting.value = true
  errorMessage.value = ''
  successMessage.value = ''

  try {
    createdShipment.value = await createShipment(authStore.token, { ...form })
    successMessage.value = 'El envío se creó correctamente y ya tiene tracking asignado.'
  } catch (error) {
    createdShipment.value = null
    errorMessage.value = error instanceof Error ? error.message : 'No fue posible crear el envío.'
  } finally {
    isSubmitting.value = false
  }
}

onMounted(loadCarriers)
</script>

<template>
  <section class="detail-page shipment-create-page">
    <div class="detail-toolbar">
      <Button label="Volver" icon="pi pi-arrow-left" text @click="router.push({ name: 'orders' })" />
      <Button label="Crear envío" icon="pi pi-send" :loading="isSubmitting" @click="submitShipment" />
    </div>

    <header class="detail-header">
      <div class="detail-hero-copy">
        <p class="eyebrow">Operación logística</p>
        <h1>Alta de orden de envío</h1>
        <p class="lede">Buscá una orden existente, completá los datos físicos del paquete y generá el envío con costo calculado por código postal y seguro.</p>
      </div>
      <div class="detail-hero-side">
        <div class="detail-kicker">
          <span>Usuario activo</span>
          <strong>{{ authStore.role }}</strong>
        </div>
        <div v-if="selectedCarrier" class="detail-kicker">
          <span>Carrier seleccionado</span>
          <strong>{{ selectedCarrier.name }}</strong>
        </div>
      </div>
    </header>

    <Message v-if="errorMessage" severity="error" :closable="false">{{ errorMessage }}</Message>
    <Message v-if="successMessage" severity="success" :closable="false">{{ successMessage }}</Message>

    <div class="detail-grid shipment-create-grid">
      <Card class="panel-card detail-span-2">
        <template #title>Buscar orden</template>
        <template #content>
          <div class="shipment-order-search-row">
            <label class="shipment-form-field shipment-grow-field">
              <span>ID de orden</span>
              <InputText v-model="orderIdSearch" placeholder="Pegá el GUID de la orden" />
            </label>
            <Button label="Cargar orden" icon="pi pi-search" :loading="isLoadingOrder" @click="loadOrder" />
          </div>
        </template>
      </Card>

      <Card class="panel-card">
        <template #title>Orden vinculada</template>
        <template #content>
          <div v-if="order" class="quote-card shipment-order-card">
            <div>
              <span>Cliente</span>
              <strong>{{ order.customer }}</strong>
            </div>
            <div>
              <span>Warehouse</span>
              <strong>{{ order.assignedWarehouse }}</strong>
            </div>
            <div>
              <span>Destino</span>
              <strong>{{ order.destinationCity }}, {{ order.destinationState }}</strong>
            </div>
            <div>
              <span>Código postal</span>
              <strong>{{ order.destinationPostalCode }}</strong>
            </div>
          </div>
          <p v-else class="empty-state-copy">Todavía no hay una orden cargada para preparar el envío.</p>
        </template>
      </Card>

      <Card class="panel-card">
        <template #title>Cotización logística</template>
        <template #content>
          <div class="settings-form-grid shipment-form-grid-tight">
            <label class="settings-check-row shipment-check-row">
              <span>Incluir seguro</span>
              <input v-model="form.includeInsurance" type="checkbox" @change="loadQuote" />
            </label>
          </div>

          <div class="settings-actions-row shipment-quote-actions">
            <Button label="Recalcular" icon="pi pi-calculator" severity="secondary" outlined :loading="isCalculatingQuote" :disabled="!canQuote" @click="loadQuote" />
          </div>

          <div v-if="quote" class="quote-card">
            <div>
              <span>Regla aplicada</span>
              <strong>{{ quote.matchedRuleName || 'Tarifa por defecto' }}</strong>
            </div>
            <div>
              <span>Base</span>
              <strong>{{ new Intl.NumberFormat('es-AR', { style: 'currency', currency: 'ARS' }).format(quote.baseShippingCost) }}</strong>
            </div>
            <div>
              <span>Seguro</span>
              <strong>{{ new Intl.NumberFormat('es-AR', { style: 'currency', currency: 'ARS' }).format(quote.insuranceCost) }}</strong>
            </div>
            <div>
              <span>Total estimado</span>
              <strong>{{ formattedQuoteTotal }}</strong>
            </div>
          </div>
          <p v-else class="empty-state-copy">Cargá una orden para calcular el costo estimado del envío.</p>
        </template>
      </Card>

      <Card class="panel-card detail-span-2">
        <template #title>Datos del envío</template>
        <template #content>
          <div class="shipment-form-grid">
            <label class="shipment-form-field">
              <span>Cliente</span>
              <InputText v-model="form.customer" placeholder="Nombre del cliente" />
            </label>
            <label class="shipment-form-field">
              <span>Carrier</span>
              <Dropdown v-model="form.carrierId" :options="carrierOptions" option-label="label" option-value="value" placeholder="Seleccionar carrier" :loading="isLoadingCarriers" />
            </label>
            <label class="shipment-form-field shipment-span-2">
              <span>Dirección de entrega</span>
              <InputText v-model="form.destinationAddress" placeholder="Calle, altura, localidad y referencias" />
            </label>
            <label class="shipment-form-field">
              <span>Recibe</span>
              <InputText v-model="form.recipientName" placeholder="Nombre y apellido" />
            </label>
            <label class="shipment-form-field">
              <span>Teléfono</span>
              <InputText v-model="form.recipientPhone" placeholder="+54 11 5555 5555" />
            </label>
            <label class="shipment-form-field shipment-span-2">
              <span>Email</span>
              <InputText v-model="form.recipientEmail" placeholder="destinatario@cliente.com" />
            </label>
            <div v-if="selectedCarrier" class="quote-card shipment-carrier-card shipment-span-2">
              <div>
                <span>Servicio</span>
                <strong>{{ selectedCarrier.serviceLevel }}</strong>
              </div>
              <div>
                <span>Seguro</span>
                <strong>{{ selectedCarrier.insuranceSupported ? 'Disponible' : 'No disponible' }}</strong>
              </div>
              <div>
                <span>Contacto</span>
                <strong>{{ selectedCarrier.supportEmail || 'Sin email' }}</strong>
              </div>
              <div>
                <span>Tracking URL</span>
                <strong>{{ selectedCarrier.trackingUrlTemplate }}</strong>
              </div>
            </div>
          </div>
        </template>
      </Card>

      <Card class="panel-card shipment-volume-card">
        <template #title>Volumetría</template>
        <template #content>
          <div class="shipment-volume-grid">
            <label class="shipment-form-field">
              <span>Peso kg</span>
              <input v-model.number="form.weightKg" class="p-inputtext" type="number" min="0.1" step="0.1" />
            </label>
            <label class="shipment-form-field">
              <span>Alto cm</span>
              <input v-model.number="form.heightCm" class="p-inputtext" type="number" min="1" step="1" />
            </label>
            <label class="shipment-form-field">
              <span>Ancho cm</span>
              <input v-model.number="form.widthCm" class="p-inputtext" type="number" min="1" step="1" />
            </label>
            <label class="shipment-form-field">
              <span>Largo cm</span>
              <input v-model.number="form.lengthCm" class="p-inputtext" type="number" min="1" step="1" />
            </label>
          </div>
        </template>
      </Card>

      <Card v-if="createdShipment" class="panel-card detail-span-2">
        <template #title>Envío generado</template>
        <template #content>
          <div class="quote-card shipment-success-grid">
            <div>
              <span>Tracking</span>
              <strong>{{ createdShipment.trackingNumber }}</strong>
            </div>
            <div>
              <span>Estado</span>
              <strong>{{ createdShipment.status }}</strong>
            </div>
            <div>
              <span>Costo final</span>
              <strong>{{ new Intl.NumberFormat('es-AR', { style: 'currency', currency: 'ARS' }).format(createdShipment.shippingCost) }}</strong>
            </div>
            <div>
              <span>Destino</span>
              <strong>{{ createdShipment.destinationAddress }}</strong>
            </div>
            <div>
              <span>Recibe</span>
              <strong>{{ createdShipment.recipientName }}</strong>
            </div>
            <div>
              <span>Contacto</span>
              <strong>{{ createdShipment.recipientPhone }}</strong>
            </div>
          </div>
        </template>
      </Card>
    </div>
  </section>
</template>
