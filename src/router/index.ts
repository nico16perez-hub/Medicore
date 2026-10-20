import { createRouter, createWebHistory } from '@ionic/vue-router';
import { navegacion } from '@/config/navegacion';
import main_layout from '@/layouts/main_layout.vue';
import login_page from '@/views/login_page.vue';
import { restaurar_sesion, sesion_store } from '@/stores/sesion_store';

const rutas_app = navegacion.map(item => ({
  path: item.ruta.replace('/app/', ''),
  name: item.id,
  component: item.componente,
  meta: { requiresAuth: true, roles: item.roles }
}));

const routes = [
  {
    path: '/login',
    name: 'login',
    component: login_page,
    meta: { public: true }
  },
  {
    path: '/',
    redirect: '/app/inicio',
    meta: { requiresAuth: true }
  },
  {
    path: '/app',
    component: main_layout,
    children: [
      {
        path: '',
        redirect: '/app/inicio'
      },
      ...rutas_app
    ]
  },
  {
    path: '/:pathMatch(.*)*',
    redirect: '/app/inicio'
  }
];

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes
});

router.beforeEach(async to => {
  if (to.meta.public) {
    try {
      if (await restaurar_sesion()) {
        return { name: 'inicio' };
      }
    } catch (error) {
      sesion_store.error = error instanceof Error ? error.message : 'No se pudo restaurar la sesión.';
    }
    return true;
  }

  let autenticado = false;
  try {
    autenticado = await restaurar_sesion();
  } catch (error) {
    sesion_store.error = error instanceof Error ? error.message : 'No se pudo restaurar la sesión.';
  }
  if (!autenticado) {
    return { name: 'login', query: { redirect: to.fullPath } };
  }

  const roles = to.meta.roles;
  if (Array.isArray(roles) && roles.length > 0 &&
      !roles.includes(sesion_store.usuario?.rol?.codigo)) {
    return { name: 'inicio' };
  }
  return true;
});

if (typeof window !== 'undefined') {
  window.addEventListener('medicore:session-expired', () => {
    if (router.currentRoute.value.name !== 'login') {
      router.replace({ name: 'login' });
    }
  });
  window.addEventListener('medicore:role-changed', () => {
    const roles = router.currentRoute.value.meta.roles;
    if (Array.isArray(roles) && roles.length > 0 &&
        !roles.includes(sesion_store.usuario?.rol?.codigo)) {
      router.replace({ name: 'inicio' });
    }
  });
}

export default router;