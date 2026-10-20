import { createApp } from 'vue';
import { IonicVue } from '@ionic/vue';
import App from './App.vue';
import router from './router'; // Esto funcionará aunque tu archivo sea router/index.ts
import { aplicar_tema_guardado } from './config/tema'; // O './config/tema.ts' si le pusiste esa extensión
import { restaurar_registro_push_service } from './services/notificaciones_push_service';
import { sesion_store } from './stores/sesion_store';
import { notificaciones_push_store } from './stores/notificaciones_push_store';

/* Core CSS required for Ionic components to work properly */
import '@ionic/vue/css/core.css';

/* Basic CSS for apps built with Ionic */
import '@ionic/vue/css/normalize.css';
import '@ionic/vue/css/structure.css';
import '@ionic/vue/css/typography.css';
import '@ionic/vue/css/padding.css';
import '@ionic/vue/css/float-elements.css';
import '@ionic/vue/css/text-alignment.css';
import '@ionic/vue/css/text-transformation.css';
import '@ionic/vue/css/flex-utils.css';
import '@ionic/vue/css/display.css';


import '@ionic/vue/css/palettes/dark.class.css';


import './theme/variables.css';

const app = createApp(App)
  .use(IonicVue)
  .use(router);

router.isReady().then(() => {
  
  aplicar_tema_guardado();
  app.mount('#app');
  if (sesion_store.autenticado) {
    restaurar_registro_push_service().then(resultado => {
      if (resultado.mensaje) notificaciones_push_store.mensaje = resultado.mensaje;
    });
  }
}); 