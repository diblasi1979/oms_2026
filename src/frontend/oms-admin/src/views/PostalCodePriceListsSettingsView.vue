<script setup lang="ts">
import { computed, onMounted, reactive, ref } from 'vue'
import { useRouter } from 'vue-router'
import { createPostalCodePriceList, fetchPostalCodePriceLists, fetchPostalCodes, updatePostalCodePriceList } from '../services/shippingApi'
import { useAuthStore } from '../stores/auth'
import type { PostalCodePriceListRecord, PostalCodePriceListUpsertPayload, PostalCodeRecord } from '../types/oms'

const authStore = useAuthStore()
const router = useRouter()

const priceLists = ref<PostalCodePriceListRecord[]>([])
const postalCodes = ref<PostalCodeRecord[]>([])
const selectedPriceListId = ref('')
const isLoading = ref(false)
const isSaving = ref(false)
const errorMessage = ref('')
const successMessage = ref('')

const form = reactive<PostalCodePriceListUpsertPayload>({
  listName: '',
  postalCode: '',
  value: 0,
})

const resolvedZone = computed(() => postalCodes.value.find((postalCode) => postalCode.postalCode === form.postalCode)?.zone ?? 'Se resuelve desde catálogo')

async function loadData() {
  isLoading.value = true
  errorMessage.value = ''

  try {
    const [loadedPriceLists, loadedPostalCodes] = await Promise.all([
      fetchPostalCodePriceLists(authStore.token),
      fetchPostalCodes(authStore.token, true),
    ])

    priceLists.value = loadedPriceLists
    postalCodes.value = loadedPostalCodes
  } catch (error) {
    errorMessage.value = error instanceof Error ? error.message : 'No fue posible cargar las listas de precios.'
  } finally {
    isLoading.value = false
  }
}

function resetForm() {
  selectedPriceListId.value = ''
  form.listName = ''
  form.postalCode = ''
  form.value = 0
}

function editPriceList(priceList: PostalCodePriceListRecord) {
  selectedPriceListId.value = priceList.id ?? ''
  form.listName = priceList.listName
  form.postalCode = priceList.postalCode
  form.value = priceList.value
}

async function savePriceList() {
  isSaving.value = true
  errorMessage.value = ''
  successMessage.value = ''

  try {
    if (selectedPriceListId.value) {
      await updatePostalCodePriceList(authStore.token, selectedPriceListId.value, { ...form })
      successMessage.value = 'Tarifa actualizada correctamente.'
    } else {
      await createPostalCodePriceList(authStore.token, { ...form })
      successMessage.value = 'Tarifa creada correctamente.'
    }

    resetForm()
    await loadData()
  } catch (error) {
    errorMessage.value = error instanceof Error ? error.message : 'No fue posible guardar la tarifa.'
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
        <Button label="Nueva tarifa" severity="secondary" outlined icon="pi pi-plus" @click="resetForm" />
        <Button label="Guardar tarifa" icon="pi pi-save" :loading="isSaving" @click="savePriceList" />
      </div>
    </div>

    <header class="detail-header">
      <div class="detail-hero-copy">
        <p class="eyebrow">Administración logística</p>
        <h1>Listas de precios por código postal</h1>
        <p class="lede">Gestioná la matriz de tarifas por lista y código postal exacto en una pantalla separada del simulador.</p>
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
          <template #title>{{ selectedPriceListId ? 'Editar tarifa' : 'Nueva tarifa' }}</template>
          <template #content>
            <div class="shipment-form-grid">
              <label class="shipment-form-field">
                <span>Nombre de lista</span>
                <InputText v-model="form.listName" placeholder="Lista General" />
              </label>
              <label class="shipment-form-field">
                <span>Código postal</span>
                <InputText v-model="form.postalCode" placeholder="1000" />
              </label>
              <label class="shipment-form-field">
                <span>Zona</span>
                <InputText :model-value="resolvedZone" disabled />
              </label>
              <label class="shipment-form-field">
                <span>Tarifa</span>
                <input v-model.number="form.value" class="p-inputtext" type="number" min="0" step="0.01" />
              </label>
            </div>
          </template>
        </Card>

        <Card class="panel-card">
          <template #title>Resumen</template>
          <template #content>
            <div class="quote-card">
              <div>
                <span>Total tarifas</span>
                <strong>{{ priceLists.length }}</strong>
              </div>
              <div>
                <span>Listas únicas</span>
                <strong>{{ new Set(priceLists.map((priceList) => priceList.listName)).size }}</strong>
              </div>
              <div>
                <span>CP cubiertos</span>
                <strong>{{ new Set(priceLists.map((priceList) => priceList.postalCode)).size }}</strong>
              </div>
            </div>
          </template>
        </Card>

        <Card class="panel-card detail-span-2">
          <template #title>Matriz tarifaria</template>
          <template #content>
            <div class="rules-table-wrap">
              <table class="rules-table carriers-table">
                <thead>
                  <tr>
                    <th>Lista</th>
                    <th>CP</th>
                    <th>Zona</th>
                    <th>Tarifa</th>
                    <th></th>
                  </tr>
                </thead>
                <tbody>
                  <tr v-for="priceList in priceLists" :key="priceList.id">
                    <td>{{ priceList.listName }}</td>
                    <td><strong>{{ priceList.postalCode }}</strong></td>
                    <td>{{ priceList.zone }}</td>
                    <td>{{ new Intl.NumberFormat('es-AR', { style: 'currency', currency: 'ARS' }).format(priceList.value) }}</td>
                    <td>
                      <Button label="Editar" severity="secondary" outlined icon="pi pi-pencil" @click="editPriceList(priceList)" />
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