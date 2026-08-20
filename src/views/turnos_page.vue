<template>
  <comp-page :titulo="'Mis Turnos'" :mostrar_actualizar="true" @actualizar="cargar">
    <comp-esqueleto v-if="cargando" />
    <ion-list v-else>
      <ion-item v-for="turno in turnos" :key="turno.id">
        <ion-label>
          <h3>{{ turno.especialidad }} - {{ turno.profesional }}</h3>
          <p>{{ turno.fecha }} a las {{ turno.hora }}</p>
          <p><strong>Estado:</strong> {{ turno.estado }}</p>
        </ion-label>
      </ion-item>
    </ion-list>
  </comp-page>
</template>

<script>
import { IonItem, IonLabel, IonList } from '@ionic/vue';
import comp_esqueleto from '@/componets/base/comp_esqueleto.vue';
import comp_page from '@/componets/estructura/comp_page.vue';
import { obtener_turnos } from '@/datos/turnos';

export default {
  name: 'turnos_page',
  components: {
    CompEsqueleto: comp_esqueleto,
    CompPage: comp_page,
    IonItem,
    IonLabel,
    IonList
  },
  data() {
    return {
      cargando: true,
      turnos: []
    };
  },
  mounted() {
    this.cargar();
  },
  methods: {
    cargar: async function() {
      var vm = this;
      vm.cargando = true;
      const respuesta = await obtener_turnos();
      vm.turnos = respuesta.turnos;
      vm.cargando = false;
    }
  }
};
</script>