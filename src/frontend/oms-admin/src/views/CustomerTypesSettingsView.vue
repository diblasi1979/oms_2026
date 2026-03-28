<script setup lang="ts">
import { onMounted, reactive, ref } from 'vue'
import { useRouter } from 'vue-router'
import { createCustomerType, fetchCustomerTypes, updateCustomerType } from '../services/ordersApi'
import { useAuthStore } from '../stores/auth'
import type { CustomerTypeRecord, CustomerTypeUpsertPayload } from '../types/oms'

const authStore = useAuthStore()
const router = useRouter()

const customerTypes = ref<CustomerTypeRecord[]>([])
const selectedCustomerTypeId = ref('')
const isLoading = ref(false)
const isSaving = ref(false)
const errorMessage = ref('')
const successMessage = ref('')

const form = reactive<CustomerTypeUpsertPayload>({
  code: '',
  name: '',
  description: '',
  isActive: true,
})

async function loadCustomerTypes() {
  isLoading.value = true
  errorMessage.value = ''

  try {
    customerTypes.value = await fetchCustomerTypes(authStore.token, true)
  } catch (error) {
    errorMessage.value = error instanceof Error ? error.message : 'No fue posible cargar los tipos de cliente.'
  } finally {
    isLoading.value = false
  }
}

function resetForm() {
  selectedCustomerTypeId.value = ''
  form.code = ''
  form.name = ''
  form.description = ''
  form.isActive = true
}

function editCustomerType(customerType: CustomerTypeRecord) {
  selectedCustomerTypeId.value = customerType.id
  form.code = customerType.code
  form.name = customerType.name
  form.description = customerType.description
  form.isActive = customerType.isActive
}

async function saveCustomerType() {
  isSaving.value = true
  errorMessage.value = ''
  successMessage.value = ''

  try {
    if (selectedCustomerTypeId.value) {
      await updateCustomerType(authStore.token, selectedCustomerTypeId.value, { ...form })
      successMessage.value = 'Tipo de cliente actualizado correctamente.'
    } else {
      await createCustomerType(authStore.token, { ...form })
      successMessage.value = 'Tipo de cliente creado correctamente.'
    }

    resetForm()
    await loadCustomerTypes()
  } catch (error) {
    errorMessage.value = error instanceof Error ? error.message : 'No fue posible guardar el tipo de cliente.'
  } finally {
    isSaving.value = false
  }
}

onMounted(loadCustomerTypes)
</script>

<template>
  <section class="detail-page settings-page">
    <div class="detail-toolbar">
      <Button label="Volver" icon="pi pi-arrow-left" text @click="router.push({ name: 'orders' })" />
      <div class="header-actions">
        <Button label="Nuevo tipo" severity="secondary" outlined icon="pi pi-plus" @click="resetForm" />
        <Button label="Guardar tipo" icon="pi pi-save" :loading="isSaving" @click="saveCustomerType" />
      </div>
    </div>

    <header class="detail-header">
      <div class="detail-hero-copy">
        <p class="eyebrow">Administración comercial</p>
        <h1>Tipos de cliente</h1>
        <p class="lede">Gestioná la segmentación comercial y operativa. La lista y el seguro ahora se administran por cliente.</p>
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
          <template #title>{{ selectedCustomerTypeId ? 'Editar tipo de cliente' : 'Nuevo tipo de cliente' }}</template>
          <template #content>
            <div class="shipment-form-grid">
              <label class="shipment-form-field">
                <span>Código</span>
                <InputText v-model="form.code" placeholder="STANDARD" />
              </label>
              <label class="shipment-form-field">
                <span>Nombre</span>
                <InputText v-model="form.name" placeholder="Standard" />
              </label>
              <label class="shipment-form-field shipment-span-2">
                <span>Descripción</span>
                <textarea v-model="form.description" class="p-inputtext carriers-notes-area" rows="4" placeholder="Criterios comerciales o segmentación"></textarea>
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
                <span>Total tipos</span>
                <strong>{{ customerTypes.length }}</strong>
              </div>
              <div>
                <span>Activos</span>
                <strong>{{ customerTypes.filter((customerType) => customerType.isActive).length }}</strong>
              </div>
              <div>
                <span>Inactivos</span>
                <strong>{{ customerTypes.filter((customerType) => !customerType.isActive).length }}</strong>
              </div>
              <div>
                <span>Edición actual</span>
                <strong>{{ selectedCustomerTypeId ? 'Tipo existente' : 'Alta nueva' }}</strong>
              </div>
            </div>
          </template>
        </Card>

        <Card class="panel-card detail-span-2">
          <template #title>Maestro comercial</template>
          <template #content>
            <div class="rules-table-wrap">
              <table class="rules-table carriers-table">
                <thead>
                  <tr>
                    <th>Nombre</th>
                    <th>Código</th>
                    <th>Descripción</th>
                    <th>Estado</th>
                    <th></th>
                  </tr>
                </thead>
                <tbody>
                  <tr v-for="customerType in customerTypes" :key="customerType.id">
                    <td>
                      <strong>{{ customerType.name }}</strong>
                    </td>
                    <td>{{ customerType.code }}</td>
                    <td>{{ customerType.description || 'Sin descripción' }}</td>
                    <td>{{ customerType.isActive ? 'Activo' : 'Inactivo' }}</td>
                    <td>
                      <Button label="Editar" severity="secondary" outlined icon="pi pi-pencil" @click="editCustomerType(customerType)" />
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