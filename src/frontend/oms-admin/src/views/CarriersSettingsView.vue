<script setup lang="ts">
import { onMounted, reactive, ref } from 'vue'
import { useRouter } from 'vue-router'
import { createCarrier, fetchCarriers, updateCarrier } from '../services/shippingApi'
import { useAuthStore } from '../stores/auth'
import type { CarrierRecord, CarrierUpsertPayload } from '../types/oms'

const authStore = useAuthStore()
const router = useRouter()

const carriers = ref<CarrierRecord[]>([])
const selectedCarrierId = ref('')
const isLoading = ref(false)
const isSaving = ref(false)
const errorMessage = ref('')
const successMessage = ref('')

const form = reactive<CarrierUpsertPayload>({
  code: '',
  name: '',
  serviceLevel: '',
  trackingUrlTemplate: '',
  supportEmail: '',
  supportPhone: '',
  insuranceSupported: true,
  isActive: true,
  notes: '',
})

async function loadCarriers() {
  isLoading.value = true
  errorMessage.value = ''

  try {
    carriers.value = await fetchCarriers(authStore.token, true)
  } catch (error) {
    errorMessage.value = error instanceof Error ? error.message : 'No fue posible cargar los carriers.'
  } finally {
    isLoading.value = false
  }
}

function resetForm() {
  selectedCarrierId.value = ''
  form.code = ''
  form.name = ''
  form.serviceLevel = ''
  form.trackingUrlTemplate = ''
  form.supportEmail = ''
  form.supportPhone = ''
  form.insuranceSupported = true
  form.isActive = true
  form.notes = ''
}

function editCarrier(carrier: CarrierRecord) {
  selectedCarrierId.value = carrier.id
  form.code = carrier.code
  form.name = carrier.name
  form.serviceLevel = carrier.serviceLevel
  form.trackingUrlTemplate = carrier.trackingUrlTemplate
  form.supportEmail = carrier.supportEmail
  form.supportPhone = carrier.supportPhone
  form.insuranceSupported = carrier.insuranceSupported
  form.isActive = carrier.isActive
  form.notes = carrier.notes
}

async function saveCarrier() {
  isSaving.value = true
  errorMessage.value = ''
  successMessage.value = ''

  try {
    if (selectedCarrierId.value) {
      await updateCarrier(authStore.token, selectedCarrierId.value, { ...form })
      successMessage.value = 'Carrier actualizado correctamente.'
    } else {
      await createCarrier(authStore.token, { ...form })
      successMessage.value = 'Carrier creado correctamente.'
    }

    resetForm()
    await loadCarriers()
  } catch (error) {
    errorMessage.value = error instanceof Error ? error.message : 'No fue posible guardar el carrier.'
  } finally {
    isSaving.value = false
  }
}

onMounted(loadCarriers)
</script>

<template>
  <section class="detail-page settings-page">
    <div class="detail-toolbar">
      <Button label="Volver" icon="pi pi-arrow-left" text @click="router.push({ name: 'orders' })" />
      <div class="header-actions">
        <Button label="Nuevo carrier" severity="secondary" outlined icon="pi pi-plus" @click="resetForm" />
        <Button label="Guardar carrier" icon="pi pi-save" :loading="isSaving" @click="saveCarrier" />
      </div>
    </div>

    <header class="detail-header">
      <div class="detail-hero-copy">
        <p class="eyebrow">Administración logística</p>
        <h1>Carriers y parametrización</h1>
        <p class="lede">Gestioná los transportistas disponibles, su nivel de servicio, tracking, soporte, disponibilidad de seguro y estado operativo.</p>
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
          <template #title>{{ selectedCarrierId ? 'Editar carrier' : 'Nuevo carrier' }}</template>
          <template #content>
            <div class="shipment-form-grid">
              <label class="shipment-form-field">
                <span>Código</span>
                <InputText v-model="form.code" placeholder="ANDREANI" />
              </label>
              <label class="shipment-form-field">
                <span>Nombre</span>
                <InputText v-model="form.name" placeholder="Andreani" />
              </label>
              <label class="shipment-form-field">
                <span>Nivel de servicio</span>
                <InputText v-model="form.serviceLevel" placeholder="Standard" />
              </label>
              <label class="shipment-form-field">
                <span>Teléfono soporte</span>
                <InputText v-model="form.supportPhone" placeholder="+54 800 000 0000" />
              </label>
              <label class="shipment-form-field shipment-span-2">
                <span>URL tracking</span>
                <InputText v-model="form.trackingUrlTemplate" placeholder="https://carrier.com/track/{trackingNumber}" />
              </label>
              <label class="shipment-form-field shipment-span-2">
                <span>Email soporte</span>
                <InputText v-model="form.supportEmail" placeholder="soporte@carrier.com" />
              </label>
              <label class="shipment-form-field shipment-span-2">
                <span>Notas operativas</span>
                <textarea v-model="form.notes" class="p-inputtext carriers-notes-area" rows="4" placeholder="Condiciones comerciales, SLA o observaciones internas"></textarea>
              </label>
              <label class="settings-check-row shipment-check-row">
                <span>Admite seguro</span>
                <input v-model="form.insuranceSupported" type="checkbox" />
              </label>
              <label class="settings-check-row shipment-check-row">
                <span>Activo</span>
                <input v-model="form.isActive" type="checkbox" />
              </label>
            </div>
          </template>
        </Card>

        <Card class="panel-card">
          <template #title>Resumen</template>
          <template #content>
            <div class="quote-card">
              <div>
                <span>Total carriers</span>
                <strong>{{ carriers.length }}</strong>
              </div>
              <div>
                <span>Activos</span>
                <strong>{{ carriers.filter((carrier) => carrier.isActive).length }}</strong>
              </div>
              <div>
                <span>Con seguro</span>
                <strong>{{ carriers.filter((carrier) => carrier.insuranceSupported).length }}</strong>
              </div>
              <div>
                <span>Edición actual</span>
                <strong>{{ selectedCarrierId ? 'Carrier existente' : 'Alta nueva' }}</strong>
              </div>
            </div>
          </template>
        </Card>

        <Card class="panel-card detail-span-2">
          <template #title>Catálogo de carriers</template>
          <template #content>
            <div class="rules-table-wrap">
              <table class="rules-table carriers-table">
                <thead>
                  <tr>
                    <th>Carrier</th>
                    <th>Código</th>
                    <th>Servicio</th>
                    <th>Seguro</th>
                    <th>Estado</th>
                    <th></th>
                  </tr>
                </thead>
                <tbody>
                  <tr v-for="carrier in carriers" :key="carrier.id">
                    <td>
                      <strong>{{ carrier.name }}</strong>
                      <div class="table-inline-note">{{ carrier.supportEmail || 'Sin email de soporte' }}</div>
                    </td>
                    <td>{{ carrier.code }}</td>
                    <td>{{ carrier.serviceLevel }}</td>
                    <td>{{ carrier.insuranceSupported ? 'Sí' : 'No' }}</td>
                    <td>{{ carrier.isActive ? 'Activo' : 'Inactivo' }}</td>
                    <td>
                      <Button label="Editar" severity="secondary" outlined icon="pi pi-pencil" @click="editCarrier(carrier)" />
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