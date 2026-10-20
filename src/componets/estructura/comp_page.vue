<template>
  <ion-page>
    <ion-header>
      <ion-toolbar>
        <ion-buttons slot="start">
          <ion-menu-button :auto-hide="false" />
        </ion-buttons>
        <ion-title>{{ titulo }}</ion-title>
        <ion-buttons slot="end">
          <span class="notificaciones-header-wrap">
            <ion-button
              fill="clear"
              shape="round"
              class="notificaciones-header-button"
              :aria-label="etiqueta_notificaciones"
              @click="ir_a_notificaciones"
            >
              <ion-icon :icon="notifications" aria-hidden="true" />
            </ion-button>
            <ion-badge
              class="notificaciones-header-badge"
              v-if="notificaciones_store.totalNoLeidas > 0"
              color="danger"
              aria-hidden="true"
            >
              {{ notificaciones_store.totalNoLeidas > 99 ? '99+' : notificaciones_store.totalNoLeidas }}
            </ion-badge>
          </span>
        </ion-buttons>
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
  IonBadge,
  IonButton,
  IonButtons,
  IonContent,
  IonHeader,
  IonIcon,
  IonMenuButton,
  IonPage,
  IonRefresher,
  IonRefresherContent,
  IonTitle,
  IonToolbar
} from '@ionic/vue';
import { notifications } from 'ionicons/icons';
import { notificaciones_store } from '@/stores/notificaciones_store';
import router from '@/router';

export default {
  name: 'comp_page',
  components: {
    IonBadge,
    IonButton,
    IonButtons,
    IonContent,
    IonHeader,
    IonIcon,
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
  data() {
    return { notificaciones_store, notifications };
  },
  computed: {
    etiqueta_notificaciones() {
      const total = this.notificaciones_store.totalNoLeidas;
      return total > 0
        ? `Notificaciones, ${total} sin leer`
        : 'Notificaciones';
    }
  },
  mounted() {
    this.notificaciones_store.cargar_total_no_leidas();
  },
  methods: {
    refrescar: function(evento){
      this.$emit('actualizar');
      window.setTimeout(() => evento.detail.complete(), 350);
    },
    ir_a_notificaciones() {
      router.push({ name: 'notificaciones' });
    }
  }
};
</script>

<style scoped>
ion-header {
  --background: #1a1a1a;
  --color: #ffffff;
}

ion-toolbar {
  --background: #1a1a1a;
  --color: #ffffff;
  --border-color: rgba(255, 255, 255, 0.12);
}

ion-buttons[slot="end"] {
  margin-inline-end: 4px;
}

.notificaciones-header-wrap {
  position: relative;
  display: inline-flex;
  width: 44px;
  height: 44px;
  align-items: center;
  justify-content: center;
}

.notificaciones-header-button {
  --color: #ffffff;
  --padding-start: 0;
  --padding-end: 0;
  --ripple-color: rgba(255, 255, 255, 0.2);
  position: relative;
  width: 40px;
  height: 40px;
  margin: 0 2px;
}

.notificaciones-header-button ion-icon {
  font-size: 1.5rem;
  width: 1.5rem;
  height: 1.5rem;
  color: #ffffff;
}

.notificaciones-header-badge {
  --background: #ff4d4d;
  --color: #ffffff;
  position: absolute;
  top: 0;
  right: -1px;
  font-size: 0.65rem;
  font-weight: 700;
  min-width: 18px;
  height: 18px;
  border-radius: 9px;
  z-index: 2;
  padding: 0 4px;
  border: 2px solid #1a1a1a;
}

.notificaciones-header-button::part(native) {
  width: 40px;
  height: 40px;
  border-radius: 50%;
}

.notificaciones-header-button:hover::part(native) {
  background: rgba(255, 255, 255, 0.08);
}
</style>
