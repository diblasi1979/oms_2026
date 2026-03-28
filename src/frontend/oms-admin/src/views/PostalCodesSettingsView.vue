<script setup lang="ts">
import { onMounted, reactive, ref } from 'vue'
import { useRouter } from 'vue-router'
import { createPostalCode, fetchPostalCodes, updatePostalCode } from '../services/shippingApi'
import { useAuthStore } from '../stores/auth'
import type { PostalCodeRecord, PostalCodeUpsertPayload } from '../types/oms'

const authStore = useAuthStore()
const router = useRouter()

const postalCodes = ref<PostalCodeRecord[]>([])
const selectedPostalCodeId = ref('')
const isLoading = ref(false)
const isSaving = ref(false)
const errorMessage = ref('')
const successMessage = ref('')

const form = reactive<PostalCodeUpsertPayload>({
  country: 'Argentina',
  province: '',
  locality: '',
  postalCode: '',
  isActive: true,
  zone: '',
})

async function loadPostalCodes() {
  isLoading.value = true
  errorMessage.value = ''

  try {
    postalCodes.value = await fetchPostalCodes(authStore.token, true)
  } catch (error) {
    errorMessage.value = error instanceof Error ? error.message : 'No fue posible cargar el catálogo postal.'
  } finally {
    isLoading.value = false
  }
}

function resetForm() {
  selectedPostalCodeId.value = ''
  form.country = 'Argentina'
  form.province = ''
  form.locality = ''
  form.postalCode = ''
  form.isActive = true
  form.zone = ''
}

function editPostalCode(postalCode: PostalCodeRecord) {
  selectedPostalCodeId.value = postalCode.id
  form.country = postalCode.country
  form.province = postalCode.province
  form.locality = postalCode.locality
  form.postalCode = postalCode.postalCode
  form.isActive = postalCode.isActive
  form.zone = postalCode.zone
}

async function savePostalCode() {
  isSaving.value = true
  errorMessage.value = ''
  successMessage.value = ''

  try {
    if (selectedPostalCodeId.value) {
      await updatePostalCode(authStore.token, selectedPostalCodeId.value, { ...form })
      successMessage.value = 'Código postal actualizado correctamente.'
    } else {
      await createPostalCode(authStore.token, { ...form })
      successMessage.value = 'Código postal creado correctamente.'
    }

    resetForm()
    await loadPostalCodes()
  } catch (error) {
    errorMessage.value = error instanceof Error ? error.message : 'No fue posible guardar el código postal.'
  } finally {
    isSaving.value = false
  }
}

onMounted(loadPostalCodes)
</script>

<template>
  <section class="detail-page settings-page">
    <div class="detail-toolbar">
      <Button label="Volver" icon="pi pi-arrow-left" text @click="router.push({ name: 'orders' })" />
      <div class="header-actions">
        <Button label="Nuevo código" severity="secondary" outlined icon="pi pi-plus" @click="resetForm" />
        <Button label="Guardar código" icon="pi pi-save" :loading="isSaving" @click="savePostalCode" />
      </div>
    </div>

    <header class="detail-header">
      <div class="detail-hero-copy">
        <p class="eyebrow">Administración logística</p>
        <h1>Catálogo de códigos postales</h1>
        <p class="lede">Administrá altas y actualizaciones del catálogo postal sin depender de seed ni scripts manuales, incluyendo zona y estado operativo.</p>
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
          <template #title>{{ selectedPostalCodeId ? 'Editar código postal' : 'Nuevo código postal' }}</template>
          <template #content>
            <div class="shipment-form-grid">
              <label class="shipment-form-field">
                <span>País</span>
                <InputText v-model="form.country" placeholder="Argentina" />
              </label>
              <label class="shipment-form-field">
                <span>Provincia</span>
                <InputText v-model="form.province" placeholder="Buenos Aires" />
              </label>
              <label class="shipment-form-field">
                <span>Localidad</span>
                <InputText v-model="form.locality" placeholder="La Plata" />
              </label>
              <label class="shipment-form-field">
                <span>Código postal</span>
                <InputText v-model="form.postalCode" placeholder="1900" />
              </label>
              <label class="shipment-form-field">
                <span>Zona</span>
                <InputText v-model="form.zone" placeholder="AMBA" />
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
                <span>Total códigos</span>
                <strong>{{ postalCodes.length }}</strong>
              </div>
              <div>
                <span>Activos</span>
                <strong>{{ postalCodes.filter((postalCode) => postalCode.isActive).length }}</strong>
              </div>
              <div>
                <span>Zonas únicas</span>
                <strong>{{ new Set(postalCodes.map((postalCode) => postalCode.zone)).size }}</strong>
              </div>
            </div>
          </template>
        </Card>

        <Card class="panel-card detail-span-2">
          <template #title>Catálogo postal</template>
          <template #content>
            <div class="rules-table-wrap">
              <table class="rules-table carriers-table">
                <thead>
                  <tr>
                    <th>País</th>
                    <th>Provincia</th>
                    <th>Localidad</th>
                    <th>CP</th>
                    <th>Zona</th>
                    <th>Estado</th>
                    <th></th>
                  </tr>
                </thead>
                <tbody>
                  <tr v-for="postalCode in postalCodes" :key="postalCode.id">
                    <td>{{ postalCode.country }}</td>
                    <td>{{ postalCode.province }}</td>
                    <td>{{ postalCode.locality }}</td>
                    <td><strong>{{ postalCode.postalCode }}</strong></td>
                    <td>{{ postalCode.zone }}</td>
                    <td>{{ postalCode.isActive ? 'Activo' : 'Inactivo' }}</td>
                    <td>
                      <Button label="Editar" severity="secondary" outlined icon="pi pi-pencil" @click="editPostalCode(postalCode)" />
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