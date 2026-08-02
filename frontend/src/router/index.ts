import { createRouter, createWebHistory } from 'vue-router'
import { useAuthStore } from '../stores/auth'

const router = createRouter({
  history: createWebHistory(),
  routes: [
    { path: '/login', component: () => import('../views/LoginView.vue') },
    { path: '/solicitudes', component: () => import('../views/SolicitudesListView.vue') },
    { path: '/solicitudes/nueva', component: () => import('../views/SolicitudFormView.vue') },
    {
      path: '/solicitudes/:id/editar',
      component: () => import('../views/SolicitudFormView.vue'),
      props: (route) => ({ id: route.params.id as string, modo: 'editar' as const }),
    },
    {
      path: '/solicitudes/:id',
      component: () => import('../views/SolicitudDetalleView.vue'),
      props: (route) => ({ id: route.params.id as string }),
    },
    { path: '/', redirect: '/solicitudes' },
  ],
})

router.beforeEach((to) => {
  const auth = useAuthStore()
  if (to.path !== '/login' && !auth.isAuthenticated) {
    return '/login'
  }
  if (to.path === '/login' && auth.isAuthenticated) {
    return '/solicitudes'
  }
})

export default router
