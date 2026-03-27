import { createRouter, createWebHistory } from 'vue-router'
import type { RouteLocationNormalized } from 'vue-router'
import LoginView from '../views/LoginView.vue'
import OrdersDashboardView from '../views/OrdersDashboardView.vue'
import OrderDetailView from '../views/OrderDetailView.vue'
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
  ],
})

router.beforeEach((to: RouteLocationNormalized) => {
  const authStore = useAuthStore()

  if (!to.meta.public && !authStore.isAuthenticated) {
    return { name: 'login' }
  }

  if (to.name === 'login' && authStore.isAuthenticated) {
    return { name: 'orders' }
  }

  return true
})

export default router
