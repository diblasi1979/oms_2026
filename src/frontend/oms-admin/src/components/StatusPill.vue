<script setup lang="ts">
import { computed } from 'vue'
import type { OrderStatus } from '../types/oms'

const props = defineProps<{
  status: OrderStatus
}>()

const normalizedLabel = computed(() => {
  switch (props.status) {
    case 'Pending':
      return 'Pendiente'
    case 'Preparing':
      return 'Preparando'
    case 'Shipped':
      return 'En tránsito'
    case 'Delivered':
      return 'Entregado'
    case 'Cancelled':
      return 'Cancelado'
    default:
      return props.status
  }
})

const statusClass = computed(() => `status-pill status-pill-${props.status.toLowerCase()}`)
</script>

<template>
  <span :class="statusClass">{{ normalizedLabel }}</span>
</template>
