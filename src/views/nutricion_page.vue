<template>
  <comp-page :titulo="'Plan Nutricional'" :mostrar_actualizar="true" @actualizar="cargar">
    <comp-esqueleto v-if="cargando" />
    <ion-list v-else>
      <ion-item v-for="item in plan" :key="item.id">
        <ion-label>
          <h3>{{ item.comida }} ({{ item.dia }})</h3>
          <p>{{ item.detalle }}</p>
        </ion-label>
      </ion-item>
    </ion-list>
  </comp-page>
</template>

<script>
import { IonItem, IonLabel, IonList } from '@ionic/vue';
import comp_esqueleto from '@/componets/base/comp_esqueleto.vue';
import comp_page from '@/componets/estructura/comp_page.vue';
import { obtener_nutricion } from '@/datos/nutricion';

export default {
  name: 'nutricion_page',
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
      plan: []
    };
  },
  mounted() {
    this.cargar();
  },
  methods: {
    cargar: async function() {
      var vm = this;
      vm.cargando = true;
      const respuesta = await obtener_nutricion();
      vm.plan = respuesta.plan;
      vm.cargando = false;
    }
  }
};
</script>