<script setup lang="ts">
import { computed, onMounted, reactive, ref } from 'vue'
import { useRouter } from 'vue-router'
import { createCustomer, fetchCustomers, fetchCustomerTypes, updateCustomer } from '../services/ordersApi'
import { useAuthStore } from '../stores/auth'
import type { CustomerRecord, CustomerTypeRecord, CustomerUpsertPayload } from '../types/oms'

const authStore = useAuthStore()
const router = useRouter()

const customers = ref<CustomerRecord[]>([])
const customerTypes = ref<CustomerTypeRecord[]>([])
const selectedCustomerId = ref('')
const search = ref('')
const isLoading = ref(false)
const isSaving = ref(false)
const errorMessage = ref('')
const successMessage = ref('')

const form = reactive<CustomerUpsertPayload>({
  code: '',
  name: '',
  customerTypeId: '',
  assignedPriceListName: '',
  insuranceRatePercentage: 0,
  isActive: true,
})

const customerTypeOptions = computed(() => customerTypes.value.map((customerType) => ({
  label: `${customerType.name} · ${customerType.code}`,
  value: customerType.id,
})))

async function loadData() {
  isLoading.value = true
  errorMessage.value = ''

  try {
    const [loadedCustomerTypes, loadedCustomers] = await Promise.all([
      fetchCustomerTypes(authStore.token, true),
      fetchCustomers(authStore.token, true, search.value),
    ])

    customerTypes.value = loadedCustomerTypes
    customers.value = loadedCustomers

    if (!form.customerTypeId && loadedCustomerTypes.length > 0) {
      form.customerTypeId = loadedCustomerTypes[0].id
    }
  } catch (error) {
    errorMessage.value = error instanceof Error ? error.message : 'No fue posible cargar los clientes.'
  } finally {
    isLoading.value = false
  }
}

function resetForm() {
  selectedCustomerId.value = ''
  form.code = ''
  form.name = ''
  form.customerTypeId = customerTypes.value[0]?.id ?? ''
  form.assignedPriceListName = ''
  form.insuranceRatePercentage = 0
  form.isActive = true
}

function editCustomer(customer: CustomerRecord) {
  selectedCustomerId.value = customer.id
  form.code = customer.code
  form.name = customer.name
  form.customerTypeId = customer.customerTypeId
  form.assignedPriceListName = customer.assignedPriceListName
  form.insuranceRatePercentage = customer.insuranceRatePercentage
  form.isActive = customer.isActive
}

async function saveCustomer() {
  isSaving.value = true
  errorMessage.value = ''
  successMessage.value = ''

  try {
    if (selectedCustomerId.value) {
      await updateCustomer(authStore.token, selectedCustomerId.value, { ...form })
      successMessage.value = 'Cliente actualizado correctamente.'
    } else {
      await createCustomer(authStore.token, { ...form })
      successMessage.value = 'Cliente creado correctamente.'
    }

    resetForm()
    await loadData()
  } catch (error) {
    errorMessage.value = error instanceof Error ? error.message : 'No fue posible guardar el cliente.'
  } finally {
    isSaving.value = false
  }
}

onMounted(loadData)
</script>

<template>
  <section class="detail-page settings-page">
    <div class="detail-toolbar">
      <Button label="Volver" icon="pi pi-arrow-left" text @click="router.push({ name: 'orders' })" />
      <div class="header-actions">
        <Button label="Nuevo cliente" severity="secondary" outlined icon="pi pi-plus" @click="resetForm" />
        <Button label="Guardar cliente" icon="pi pi-save" :loading="isSaving" @click="saveCustomer" />
      </div>
    </div>

    <header class="detail-header">
      <div class="detail-hero-copy">
        <p class="eyebrow">Administración comercial</p>
        <h1>Clientes</h1>
        <p class="lede">Gestioná acuerdos comerciales por cliente, incluyendo lista de precios e impacto de seguro, sin depender del tipo para cotizar.</p>
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
          <template #title>{{ selectedCustomerId ? 'Editar cliente' : 'Nuevo cliente' }}</template>
          <template #content>
            <div class="shipment-form-grid">
              <label class="shipment-form-field">
                <span>Código</span>
                <InputText v-model="form.code" placeholder="ACMESA" />
              </label>
              <label class="shipment-form-field">
                <span>Nombre</span>
                <InputText v-model="form.name" placeholder="ACME SA" />
              </label>
              <label class="shipment-form-field shipment-span-2">
                <span>Tipo de cliente</span>
                <Dropdown v-model="form.customerTypeId" :options="customerTypeOptions" option-label="label" option-value="value" placeholder="Seleccionar tipo" />
              </label>
              <label class="shipment-form-field">
                <span>Lista de precios</span>
                <InputText v-model="form.assignedPriceListName" placeholder="Lista General" />
              </label>
              <label class="shipment-form-field">
                <span>% de seguro</span>
                <input v-model.number="form.insuranceRatePercentage" class="p-inputtext" type="number" min="0" step="0.01" />
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
                <span>Total clientes</span>
                <strong>{{ customers.length }}</strong>
              </div>
              <div>
                <span>Activos</span>
                <strong>{{ customers.filter((customer) => customer.isActive).length }}</strong>
              </div>
              <div>
                <span>Tipos disponibles</span>
                <strong>{{ customerTypes.length }}</strong>
              </div>
              <div>
                <span>Edición actual</span>
                <strong>{{ selectedCustomerId ? 'Cliente existente' : 'Alta nueva' }}</strong>
              </div>
            </div>
          </template>
        </Card>

        <Card class="panel-card detail-span-2">
          <template #title>Maestro de clientes</template>
          <template #content>
            <div class="shipment-order-search-row">
              <label class="shipment-form-field shipment-grow-field">
                <span>Buscar</span>
                <InputText v-model="search" placeholder="Cliente, código o tipo" @keyup.enter="loadData" />
              </label>
              <Button label="Actualizar" icon="pi pi-refresh" severity="secondary" outlined @click="loadData" />
            </div>

            <div class="rules-table-wrap">
              <table class="rules-table carriers-table">
                <thead>
                  <tr>
                    <th>Nombre</th>
                    <th>Código</th>
                    <th>Tipo</th>
                    <th>Lista</th>
                    <th>% Seguro</th>
                    <th>Estado</th>
                    <th></th>
                  </tr>
                </thead>
                <tbody>
                  <tr v-for="customer in customers" :key="customer.id">
                    <td><strong>{{ customer.name }}</strong></td>
                    <td>{{ customer.code }}</td>
                    <td>{{ customer.customerTypeName }}</td>
                    <td>{{ customer.assignedPriceListName }}</td>
                    <td>{{ customer.insuranceRatePercentage.toFixed(2) }}%</td>
                    <td>{{ customer.isActive ? 'Activo' : 'Inactivo' }}</td>
                    <td>
                      <Button label="Editar" severity="secondary" outlined icon="pi pi-pencil" @click="editCustomer(customer)" />
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