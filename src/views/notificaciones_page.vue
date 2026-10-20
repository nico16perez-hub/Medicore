<template>
  <comp-page
    :titulo="'Notificaciones'"
    :mostrar_actualizar="true"
    @actualizar="cargar"
  >
    <ion-item v-if="store.totalNoLeidas > 0" class="notificaciones-resumen">
      <ion-label>{{ store.totalNoLeidas }} sin leer</ion-label>
      <ion-button slot="end" fill="clear" size="small" @click="marcar_todas_leidas">
        Marcar todas como leídas
      </ion-button>
    </ion-item>
    <ion-item v-if="store.error" class="notificaciones-error">
      <ion-label color="danger">{{ store.error }}</ion-label>
    </ion-item>
    <comp-esqueleto v-if="store.cargando" />
    <div v-else-if="!store.error && store.notificaciones.length === 0" class="estado-vacio">
      <h2>No tenés notificaciones</h2>
      <p>Los avisos relacionados con tus turnos aparecerán acá.</p>
    </div>
    <ion-list v-else-if="store.notificaciones.length" class="notificaciones-lista" lines="none">
      <ion-item
        v-for="notificacion in store.notificaciones"
        :key="notificacion.id"
        class="notificacion-item"
        :class="{ 'notificacion-no-leida': !notificacion.leida }"
      >
        <ion-label>
          <h2>{{ notificacion.titulo }}</h2>
          <p>{{ notificacion.mensaje }}</p>
          <p class="notificacion-fecha">{{ formatear_fecha(notificacion.creada_en_utc) }}</p>
        </ion-label>
        <ion-button
          v-if="!notificacion.leida"
          slot="end"
          fill="clear"
          size="small"
          class="marcar-leida"
          aria-label="Marcar como leída"
          @click="marcar_leida(notificacion.id)"
        >
          Marcar leída
        </ion-button>
      </ion-item>
    </ion-list>
    <div v-if="store.totalPaginas > 1" class="paginacion">
      <ion-button
        fill="clear"
        :disabled="store.paginaActual <= 1 || store.cargando"
        @click="cargar(store.paginaActual - 1)"
      >
        Anterior
      </ion-button>
      <span>Página {{ store.paginaActual }} de {{ store.totalPaginas }}</span>
      <ion-button
        fill="clear"
        :disabled="store.paginaActual >= store.totalPaginas || store.cargando"
        @click="cargar(store.paginaActual + 1)"
      >
        Siguiente
      </ion-button>
    </div>
  </comp-page>
</template>

<script>
import { IonButton, IonItem, IonLabel, IonList } from '@ionic/vue';
import comp_esqueleto from '@/componets/base/comp_esqueleto.vue';
import comp_page from '@/componets/estructura/comp_page.vue';
import { notificaciones_store } from '@/stores/notificaciones_store';

export default {
  name: 'notificaciones_page',
  components: {
    CompEsqueleto: comp_esqueleto,
    CompPage: comp_page,
    IonButton,
    IonItem,
    IonLabel,
    IonList
  },
  data() {
    return { store: notificaciones_store };
  },
  mounted() {
    this.cargar();
  },
  methods: {
    cargar(pagina = 1) {
      return this.store.cargar(pagina);
    },
    marcar_leida(id) {
      return this.store.marcar_leida(id);
    },
    marcar_todas_leidas() {
      return this.store.marcar_todas_leidas();
    },
    formatear_fecha(valor) {
      const fecha = new Date(valor);
      return Number.isNaN(fecha.getTime())
        ? valor
        : fecha.toLocaleString();
    }
  }
};
</script>

<style scoped>
.estado-vacio {
  margin: 18px 16px;
  padding: 32px 20px;
  border: 1px solid var(--ion-color-step-150);
  border-radius: 16px;
  background: var(--ion-card-background);
  text-align: center;
}

.estado-vacio h2 {
  margin: 0 0 8px;
  color: var(--ion-text-color);
}

.estado-vacio p {
  margin: 0;
  color: var(--ion-color-medium);
}

.notificaciones-resumen {
  --background: transparent;
  --color: var(--ion-text-color);
  --min-height: 58px;
  --padding-start: 18px;
  --inner-padding-end: 12px;
  --border-color: var(--ion-color-step-150);
  border-bottom: 1px solid var(--ion-color-step-150);
  font-weight: 650;
}

.notificaciones-resumen ion-button {
  --color: var(--ion-color-primary);
  font-size: 0.78rem;
  font-weight: 700;
}

.notificaciones-error {
  --background: transparent;
  --padding-start: 18px;
}

.notificaciones-lista {
  padding: 8px 12px 20px;
  background: transparent;
}

.notificacion-item {
  --background: var(--ion-card-background);
  --color: var(--ion-text-color);
  --border-color: transparent;
  --min-height: 82px;
  --padding-start: 16px;
  --inner-padding-end: 12px;
  margin: 7px 0;
  border: 1px solid var(--ion-color-step-150);
  border-radius: 12px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.07);
}

.notificacion-no-leida {
  --background: rgba(var(--ion-color-primary-rgb), 0.14);
  border-color: rgba(var(--ion-color-primary-rgb), 0.38);
}

.notificacion-item ion-label h2 {
  margin: 2px 0 5px;
  color: var(--ion-text-color);
  font-size: 0.98rem;
  font-weight: 700;
}

.notificacion-item ion-label p {
  margin: 3px 0;
  color: var(--ion-text-color);
  font-size: 0.88rem;
  line-height: 1.4;
  opacity: 0.82;
}

.notificacion-item ion-label p.notificacion-fecha {
  color: var(--ion-color-medium);
  font-size: 0.75rem;
  opacity: 1;
}

.marcar-leida {
  --color: var(--ion-color-primary);
  font-size: 0.75rem;
  font-weight: 700;
}

.paginacion {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 12px;
  padding: 12px 16px 24px;
}
</style>
