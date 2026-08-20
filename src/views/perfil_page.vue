<template>
  <comp-page :titulo="'Mi cuenta'">
    <ion-list>
      <ion-item>
        <ion-label>Modo oscuro</ion-label>
        <ion-toggle slot="end" :checked="tema_oscuro" @ion-change="cambiar_tema" />
      </ion-item>
    </ion-list>
  </comp-page>
</template>

<script>
import { IonItem, IonLabel, IonList, IonToggle } from '@ionic/vue';
import comp_page from '@/componets/estructura/comp_page.vue';
import { alternar_tema, es_tema_oscuro } from '@/config/tema';

export default {
  name: 'perfil_page',
  components: {
    CompPage: comp_page,
    IonItem,
    IonLabel,
    IonList,
    IonToggle
  },
  data() {
    return {
      tema_oscuro: es_tema_oscuro()
    };
  },
  methods: {
    cambiar_tema: function(evento) {
      if (evento.detail.checked === this.tema_oscuro) return;
      alternar_tema();
      this.tema_oscuro = es_tema_oscuro();
    }
  }
};
</script>
