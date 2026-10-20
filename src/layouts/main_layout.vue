<template>
  <ion-page>
    <comp-menu content_id="contenido_principal" />
    <ion-tabs id="contenido_principal">
      <ion-router-outlet />
      <comp-tabs />
    </ion-tabs>
    <comp-notificaciones-flotantes />
  </ion-page>
</template>

<script>
import {
  IonPage,
  IonRouterOutlet,
  IonTabs
} from '@ionic/vue';
import comp_menu from '@/componets/estructura/comp_menu.vue';
import comp_tabs from '@/componets/estructura/comp_tabs.vue';
import comp_notificaciones_flotantes from '@/componets/notificaciones/comp_notificaciones_flotantes.vue';

export default {
  name: 'main_layout',
  components: {
    CompMenu: comp_menu,
    CompTabs: comp_tabs,
    CompNotificacionesFlotantes: comp_notificaciones_flotantes,
    IonPage,
    IonRouterOutlet,
    IonTabs
  }
};
</script>