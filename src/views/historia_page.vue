<template>
  <comp-page :titulo="'Historia Clínica'" :mostrar_actualizar="true" @actualizar="cargar">
    <comp-esqueleto v-if="cargando" />
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
import { IonItem, IonLabel, IonList } from '@ionic/vue';
import comp_esqueleto from '@/componets/base/comp_esqueleto.vue';
import comp_page from '@/componets/estructura/comp_page.vue';
import { obtener_historia } from '@/datos/historia';

export default {
  name: 'historia_page',
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
      registros: []
    };
  },
  mounted() {
    this.cargar();
  },
  methods: {
    cargar: async function() {
      var vm = this;
      vm.cargando = true;
      const respuesta = await obtener_historia();
      vm.registros = respuesta.registros;
      vm.cargando = false;
    }
  }
};
</script>