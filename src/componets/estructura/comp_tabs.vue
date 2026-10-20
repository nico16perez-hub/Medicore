<template>
  <ion-tab-bar slot="bottom">
    <ion-tab-button
      v-for="item in items_tabs"
      :key="item.id"
      :tab="item.id"
      :href="item.ruta"
    >
      <ion-icon :icon="item.icono" />
      <ion-label>{{ item.titulo }}</ion-label>
    </ion-tab-button>
  </ion-tab-bar>
</template>

<script>
import {
  IonIcon,
  IonLabel,
  IonTabBar,
  IonTabButton
} from '@ionic/vue';
import { obtener_tabs } from '@/config/navegacion';
import { sesion_store } from '@/stores/sesion_store';

export default {
  name: 'comp_tabs',
  components: {
    IonIcon,
    IonLabel,
    IonTabBar,
    IonTabButton
  },
  computed: {
    items_tabs(){
      return obtener_tabs(sesion_store.usuario?.rol?.codigo || null)
        .filter(item => item.id !== 'notificaciones');
    }
  }
};
</script>