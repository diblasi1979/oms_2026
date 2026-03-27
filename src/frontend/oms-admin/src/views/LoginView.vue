<script setup lang="ts">
import { reactive, ref } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '../stores/auth'

const authStore = useAuthStore()
const router = useRouter()
const errorMessage = ref('')

const form = reactive({
  username: 'admin',
  password: 'OmsAdmin123!',
})

async function submit() {
  errorMessage.value = ''

  try {
    await authStore.signIn(form)
    await router.push({ name: 'orders' })
  } catch (error) {
    errorMessage.value = error instanceof Error ? error.message : 'No fue posible iniciar sesion.'
  }
}
</script>

<template>
  <section class="login-page">
    <div class="login-backdrop"></div>
    <Card class="login-card">
      <template #title>
        OMS Control Tower
      </template>
      <template #subtitle>
        Monitoreo de pedidos, stock reservado y envíos operativos.
      </template>
      <template #content>
        <div class="login-copy-block">
          <p class="eyebrow eyebrow-inline">Dark operations</p>
          <h2>Una vista más nítida para logística y fulfillment.</h2>
        </div>
        <div class="form-grid">
          <label>
            <span>Usuario</span>
            <InputText v-model="form.username" placeholder="admin" />
          </label>
          <label>
            <span>Clave</span>
            <Password v-model="form.password" toggle-mask :feedback="false" input-class="w-full" />
          </label>
          <Message v-if="errorMessage" severity="error" :closable="false">
            {{ errorMessage }}
          </Message>
          <div class="demo-credentials">
            <strong>Demo:</strong>
            <span>admin / OmsAdmin123!</span>
            <span>operator / OmsOperator123!</span>
          </div>
          <Button label="Ingresar" icon="pi pi-arrow-right" :loading="authStore.isSubmitting" @click="submit" />
        </div>
      </template>
    </Card>
  </section>
</template>
