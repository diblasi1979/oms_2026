<script setup lang="ts">
import { onMounted, ref } from 'vue'
import { useRouter } from 'vue-router'
import { calculateShipmentQuote, fetchShipmentPricingSettings, updateShipmentPricingSettings } from '../services/shippingApi'
import { useAuthStore } from '../stores/auth'
import type { ShipmentPricingQuote, ShipmentPricingSettings } from '../types/oms'

const authStore = useAuthStore()
const router = useRouter()

const settings = ref<ShipmentPricingSettings>({
  defaultBaseCost: 0,
  insuranceFlatCost: 0,
  rules: [],
})
const quotePostalCode = ref('1001')
const includeInsurance = ref(true)
const quote = ref<ShipmentPricingQuote | null>(null)
const isLoading = ref(false)
const isSaving = ref(false)
const isQuoting = ref(false)
const errorMessage = ref('')
const successMessage = ref('')

async function loadSettings() {
  isLoading.value = true
  errorMessage.value = ''

  try {
    settings.value = await fetchShipmentPricingSettings(authStore.token)
    await loadQuote()
  } catch (error) {
    errorMessage.value = error instanceof Error ? error.message : 'No fue posible cargar la configuración de tarifas.'
  } finally {
    isLoading.value = false
  }
}

function addRule() {
  settings.value.rules.push({
    ruleName: '',
    postalCodePrefix: '',
    baseCost: 0,
  })
}

function removeRule(index: number) {
  settings.value.rules.splice(index, 1)
}

async function saveSettings() {
  isSaving.value = true
  errorMessage.value = ''
  successMessage.value = ''

  try {
    settings.value = await updateShipmentPricingSettings(authStore.token, settings.value)
    successMessage.value = 'Las tarifas y el seguro se actualizaron correctamente.'
    await loadQuote()
  } catch (error) {
    errorMessage.value = error instanceof Error ? error.message : 'No fue posible guardar la configuración.'
  } finally {
    isSaving.value = false
  }
}

async function loadQuote() {
  isQuoting.value = true

  try {
    quote.value = await calculateShipmentQuote(authStore.token, quotePostalCode.value, includeInsurance.value)
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
      <Button label="Guardar configuración" icon="pi pi-save" :loading="isSaving" @click="saveSettings" />
    </div>

    <header class="detail-header">
      <div class="detail-hero-copy">
        <p class="eyebrow">Administración logística</p>
        <h1>Tarifas de envío y seguro</h1>
        <p class="lede">Define la tarifa base por prefijo postal y el cargo de seguro que se sumará al crear cada envío.</p>
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
      <Message v-if="successMessage" severity="success" :closable="false">{{ successMessage }}</Message>

      <div class="detail-grid">
        <Card class="panel-card">
          <template #title>Costos globales</template>
          <template #content>
            <div class="settings-form-grid">
              <label>
                <span>Tarifa base por defecto</span>
                <input v-model.number="settings.defaultBaseCost" class="p-inputtext" type="number" min="0" step="0.01" />
              </label>
              <label>
                <span>Costo fijo de seguro</span>
                <input v-model.number="settings.insuranceFlatCost" class="p-inputtext" type="number" min="0" step="0.01" />
              </label>
            </div>
          </template>
        </Card>

        <Card class="panel-card">
          <template #title>Simulador de cotización</template>
          <template #content>
            <div class="settings-form-grid">
              <label>
                <span>Código postal destino</span>
                <input v-model="quotePostalCode" class="p-inputtext" type="text" placeholder="1001" />
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
                <span>Regla aplicada</span>
                <strong>{{ quote.matchedRuleName || 'Tarifa por defecto' }}</strong>
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
          </template>
        </Card>

        <Card class="panel-card detail-span-2">
          <template #title>Reglas por código postal</template>
          <template #content>
            <div class="settings-actions-row">
              <Button label="Agregar regla" icon="pi pi-plus" @click="addRule" />
            </div>

            <div class="rules-table-wrap">
              <table class="rules-table">
                <thead>
                  <tr>
                    <th>Zona</th>
                    <th>Prefijo postal</th>
                    <th>Tarifa base</th>
                    <th></th>
                  </tr>
                </thead>
                <tbody>
                  <tr v-for="(rule, index) in settings.rules" :key="rule.id ?? `new-${index}`">
                    <td>
                      <input v-model="rule.ruleName" class="p-inputtext" type="text" placeholder="AMBA" />
                    </td>
                    <td>
                      <input v-model="rule.postalCodePrefix" class="p-inputtext" type="text" placeholder="1" />
                    </td>
                    <td>
                      <input v-model.number="rule.baseCost" class="p-inputtext" type="number" min="0" step="0.01" />
                    </td>
                    <td>
                      <Button label="Quitar" severity="secondary" outlined icon="pi pi-trash" @click="removeRule(index)" />
                    </td>
                  </tr>
                </tbody>
              </table>
            </div>
          </template>
        </Card>
      </div>
    </template>
  </section>
</template>