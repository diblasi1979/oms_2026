import { createRouter, createWebHistory } from 'vue-router'
import type { RouteLocationNormalized } from 'vue-router'
import LoginView from '../views/LoginView.vue'
import OrdersDashboardView from '../views/OrdersDashboardView.vue'
import OrderDetailView from '../views/OrderDetailView.vue'
import ShippingPricingSettingsView from '../views/ShippingPricingSettingsView.vue'
import ShipmentCreateView from '../views/ShipmentCreateView.vue'
import CarriersSettingsView from '../views/CarriersSettingsView.vue'
import CustomerTypesSettingsView from '../views/CustomerTypesSettingsView.vue'
import PostalCodesSettingsView from '../views/PostalCodesSettingsView.vue'
import { useAuthStore } from '../stores/auth'

const router = createRouter({
  history: createWebHistory(),
  routes: [
    {
      path: '/',
      redirect: '/orders',
    },
    {
      path: '/login',
      name: 'login',
      component: LoginView,
      meta: { public: true },
    },
    {
      path: '/orders',
      name: 'orders',
      component: OrdersDashboardView,
    },
    {
      path: '/orders/:orderId',
      name: 'order-detail',
      component: OrderDetailView,
      props: true,
    },
    {
      path: '/shipments/new',
      name: 'shipment-create',
      component: ShipmentCreateView,
    },
    {
      path: '/settings/shipping-pricing',
      name: 'shipping-pricing-settings',
      component: ShippingPricingSettingsView,
      meta: { requiresAdmin: true },
    },
    {
      path: '/settings/carriers',
      name: 'carriers-settings',
      component: CarriersSettingsView,
      meta: { requiresAdmin: true },
    },
    {
      path: '/settings/customer-types',
      name: 'customer-types-settings',
      component: CustomerTypesSettingsView,
      meta: { requiresAdmin: true },
    },
    {
      path: '/settings/postal-codes',
      name: 'postal-codes-settings',
      component: PostalCodesSettingsView,
      meta: { requiresAdmin: true },
    },
  ],
})

router.beforeEach((to: RouteLocationNormalized) => {
  const authStore = useAuthStore()

  if (!to.meta.public && !authStore.isAuthenticated) {
    return { name: 'login' }
  }

  if (to.meta.requiresAdmin && authStore.role !== 'Admin') {
    return { name: 'orders' }
  }

  if (to.name === 'login' && authStore.isAuthenticated) {
    return { name: 'orders' }
  }

  return true
})

export default router
