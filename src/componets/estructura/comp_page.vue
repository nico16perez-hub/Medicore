<template>
  <ion-page>
    <ion-header>
      <ion-toolbar>
        <ion-buttons slot="start">
          <ion-menu-button :auto-hide="false" />
        </ion-buttons>
        <ion-title>{{ titulo }}</ion-title>
      </ion-toolbar>
    </ion-header>
    <ion-content :fullscreen="true">
      <ion-refresher v-if="mostrar_actualizar" slot="fixed" @ionRefresh="refrescar">
        <ion-refresher-content pulling-text="Desliza para actualizar" refreshing-text="Actualizando..." />
      </ion-refresher>
      <slot />
    </ion-content>
  </ion-page>
</template>

<script>
import {
  IonButtons,
  IonContent,
  IonHeader,
  IonMenuButton,
  IonPage,
  IonRefresher,
  IonRefresherContent,
  IonTitle,
  IonToolbar
} from '@ionic/vue';

export default {
  name: 'comp_page',
  components: {
    IonButtons,
    IonContent,
    IonHeader,
    IonMenuButton,
    IonPage,
    IonRefresher,
    IonRefresherContent,
    IonTitle,
    IonToolbar
  },
  emits: ['actualizar'],
  props: {
    mostrar_actualizar: {
      type: Boolean,
      default: false
    },
    titulo: {
      type: String,
      required: true
    }
  },
  methods: {
    refrescar: function(evento){
      var vm = this;
      vm.$emit('actualizar');
      window.setTimeout(() => evento.detail.complete(), 350);
    }
  }
};
</script>
