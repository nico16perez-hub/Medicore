<template>
  <comp-page :titulo="'Historia Clínica'" :mostrar_actualizar="true" @actualizar="cargar">
    <comp-esqueleto v-if="cargando" />
    <div v-else-if="error" class="estado estado-error">
      <ion-text color="danger">
        <h2>No se pudo cargar la historia clínica</h2>
      </ion-text>
      <p>{{ error }}</p>
      <ion-button @click="cargar">Reintentar</ion-button>
    </div>
    <div v-else-if="registros.length === 0" class="estado">
      <h2>No hay registros de historia clínica</h2>
      <p>Cuando la API tenga datos, van a aparecer en esta lista.</p>
      <ion-button fill="outline" @click="cargar">Actualizar</ion-button>
    </div>
    <ion-list v-else>
      <ion-item v-for="reg in registros" :key="reg.id">
        <ion-label>
          <h3>{{ reg.tipo }}</h3>
          <p><strong>Valor:</strong> {{ reg.valor }}</p>
          <p>{{ reg.fecha }} - {{ reg.notas }}</p>
        </ion-label>
      </ion-item>
    </ion-list>
  </comp-page>
</template>

<script>
import { IonButton, IonItem, IonLabel, IonList, IonText } from '@ionic/vue';
import comp_esqueleto from '@/componets/base/comp_esqueleto.vue';
import comp_page from '@/componets/estructura/comp_page.vue';
import { historia_store } from '@/stores/historia_store';

export default {
  name: 'historia_page',
  components: {
    CompEsqueleto: comp_esqueleto,
    CompPage: comp_page,
    IonButton,
    IonItem,
    IonLabel,
    IonList,
    IonText
  },
  data() {
    return {
      store: historia_store
    };
  },
  computed: {
    cargando() {
      return this.store.cargando;
    },
    error() {
      return this.store.error;
    },
    registros() {
      return this.store.registros;
    }
  },
  mounted() {
    this.cargar();
  },
  methods: {
    cargar: async function() {
      await this.store.cargar_registros();
    }
  }
};
</script>

<style scoped>
.estado {
  display: flex;
  min-height: 60vh;
  flex-direction: column;
  justify-content: center;
  padding: 24px;
  text-align: center;
}

.estado h2 {
  margin: 0 0 8px;
  font-size: 1.15rem;
  font-weight: 700;
}

.estado p {
  margin: 0 auto 18px;
  max-width: 34rem;
  color: var(--ion-color-medium);
  line-height: 1.4;
}

.estado ion-button {
  align-self: center;
}

.estado-error p {
  color: var(--ion-color-danger-shade);
}
</style>