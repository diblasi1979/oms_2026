<script setup lang="ts">
import { onMounted, ref } from 'vue'
import { useRouter } from 'vue-router'
import { fetchCustomers } from '../services/ordersApi'
import { calculateShipmentQuote, fetchCarriers, fetchPostalCodePriceLists } from '../services/shippingApi'
import { useAuthStore } from '../stores/auth'
import type { CarrierRecord, CustomerRecord, PostalCodePriceListRecord, ShipmentPricingQuote } from '../types/oms'

const authStore = useAuthStore()
const router = useRouter()

const priceLists = ref<PostalCodePriceListRecord[]>([])
const carriers = ref<CarrierRecord[]>([])
const customers = ref<CustomerRecord[]>([])
const quoteCustomerId = ref('')
const quoteCarrierId = ref('')
const quotePostalCode = ref('1000')
const quoteDeclaredValue = ref(1000)
const includeInsurance = ref(true)
const quote = ref<ShipmentPricingQuote | null>(null)
const isLoading = ref(false)
const isQuoting = ref(false)
const errorMessage = ref('')

async function loadSettings() {
  isLoading.value = true
  errorMessage.value = ''

  try {
    customers.value = await fetchCustomers(authStore.token, true)
    carriers.value = await fetchCarriers(authStore.token, true)
    priceLists.value = await fetchPostalCodePriceLists(authStore.token)
    if (!quoteCustomerId.value && customers.value.length > 0) {
      quoteCustomerId.value = customers.value[0].id
    }
    if (!quoteCarrierId.value && carriers.value.length > 0) {
      quoteCarrierId.value = carriers.value[0].id
    }
    await loadQuote()
  } catch (error) {
    errorMessage.value = error instanceof Error ? error.message : 'No fue posible cargar la configuración de tarifas.'
  } finally {
    isLoading.value = false
  }
}

async function loadQuote() {
  isQuoting.value = true

  try {
    if (!quoteCustomerId.value || !quoteCarrierId.value) {
      quote.value = null
      return
    }

    quote.value = await calculateShipmentQuote(authStore.token, quoteCustomerId.value, quoteCarrierId.value, quotePostalCode.value, quoteDeclaredValue.value, includeInsurance.value)
  } finally {
    isQuoting.value = false
  }
}

onMounted(loadSettings)
</script>

<template>
  <section class="detail-page settings-page">
    <div class="detail-toolbar">
      <Button label="Volver" icon="pi pi-arrow-left" text @click="router.push({ name: 'orders' })" />
      <Button label="Ir a listas" icon="pi pi-list" severity="secondary" outlined @click="router.push({ name: 'postal-code-price-lists-settings' })" />
    </div>

    <header class="detail-header">
      <div class="detail-hero-copy">
        <p class="eyebrow">Administración logística</p>
        <h1>Tarifas de envío y seguro</h1>
        <p class="lede">Simulá la cotización final usando la lista asignada al cliente, el código postal exacto y el porcentaje de seguro acordado para esa cuenta.</p>
      </div>
      <div class="detail-hero-side">
        <div class="detail-kicker">
          <span>Acceso</span>
          <strong>{{ authStore.role }}</strong>
        </div>
      </div>
    </header>

    <ProgressSpinner v-if="isLoading" stroke-width="4" class="center-spinner" />
    <template v-else>
      <Message v-if="errorMessage" severity="error" :closable="false">{{ errorMessage }}</Message>
      <div class="detail-grid">
        <Card class="panel-card">
          <template #title>Resumen tarifario</template>
          <template #content>
            <div class="quote-card">
              <div>
                <span>Tarifas configuradas</span>
                <strong>{{ priceLists.length }}</strong>
              </div>
              <div>
                <span>Listas únicas</span>
                <strong>{{ new Set(priceLists.map((priceList) => priceList.listName)).size }}</strong>
              </div>
              <div>
                <span>Clientes disponibles</span>
                <strong>{{ customers.length }}</strong>
              </div>
            </div>
          </template>
        </Card>

        <Card class="panel-card">
          <template #title>Simulador de cotización</template>
          <template #content>
            <div class="settings-form-grid">
              <label>
                <span>Cliente</span>
                <Dropdown v-model="quoteCustomerId" :options="customers.map((customer) => ({ label: `${customer.name} · ${customer.customerTypeName}`, value: customer.id }))" option-label="label" option-value="value" placeholder="Seleccionar cliente" />
              </label>
              <label>
                <span>Carrier</span>
                <Dropdown v-model="quoteCarrierId" :options="carriers.map((carrier) => ({ label: carrier.name, value: carrier.id }))" option-label="label" option-value="value" placeholder="Seleccionar carrier" />
              </label>
              <label>
                <span>Código postal destino</span>
                <input v-model="quotePostalCode" class="p-inputtext" type="text" placeholder="1001" />
              </label>
              <label>
                <span>Valor declarado</span>
                <input v-model.number="quoteDeclaredValue" class="p-inputtext" type="number" min="0" step="0.01" />
              </label>
              <label class="settings-check-row">
                <span>Incluir seguro</span>
                <input v-model="includeInsurance" type="checkbox" />
              </label>
            </div>
            <div class="settings-actions-row">
              <Button label="Calcular" icon="pi pi-calculator" :loading="isQuoting" @click="loadQuote" />
            </div>

            <div v-if="quote" class="quote-card">
              <div>
                <span>Lista aplicada</span>
                <strong>{{ quote.assignedPriceListName }}</strong>
              </div>
              <div>
                <span>Cliente / carrier</span>
                <strong>{{ quote.customerName }} · {{ quote.carrierName }}</strong>
              </div>
              <div>
                <span>Zona</span>
                <strong>{{ quote.matchedZone }}</strong>
              </div>
              <div>
                <span>% seguro</span>
                <strong>{{ quote.insuranceRatePercentage.toFixed(2) }}%</strong>
              </div>
              <div>
                <span>Tarifa base</span>
                <strong>{{ new Intl.NumberFormat('es-AR', { style: 'currency', currency: 'ARS' }).format(quote.baseShippingCost) }}</strong>
              </div>
              <div>
                <span>Seguro</span>
                <strong>{{ new Intl.NumberFormat('es-AR', { style: 'currency', currency: 'ARS' }).format(quote.insuranceCost) }}</strong>
              </div>
              <div>
                <span>Total cotizado</span>
                <strong>{{ new Intl.NumberFormat('es-AR', { style: 'currency', currency: 'ARS' }).format(quote.totalShippingCost) }}</strong>
              </div>
            </div>
            <p v-else class="empty-state-copy">Indicá cliente, carrier, código postal y valor declarado para simular.</p>
          </template>
        </Card>
      </div>
    </template>
  </section>
</template>