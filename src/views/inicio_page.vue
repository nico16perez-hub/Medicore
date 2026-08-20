<template>
  <comp-page :titulo="'MediCore'">
    <ion-card>
      <ion-card-header>
        <ion-card-title>Sistema Integral Sanatorial</ion-card-title>
        <ion-card-subtitle>Gestión centralizada de pacientes</ion-card-subtitle>
      </ion-card-header>
      <ion-card-content>
        MediCore unifica turnos, historia clínica y seguimiento nutricional en una sola plataforma.
      </ion-card-content>
    </ion-card>
    <ion-card>
      <ion-card-header>
        <ion-card-title>Versión 2</ion-card-title>
      </ion-card-header>
      <ion-card-content>
        La navegación (rutas, menú y tabs) se deriva completamente de <strong>src/config/navegacion.js</strong>.
      </ion-card-content>
    </ion-card>
  </comp-page>
</template>

<script>
import {
  IonCard,
  IonCardContent,
  IonCardHeader,
  IonCardSubtitle,
  IonCardTitle
} from '@ionic/vue';
import comp_page from '@/componets/estructura/comp_page.vue';

export default {
  name: 'inicio_page',
  components: {
    CompPage: comp_page,
    IonCard,
    IonCardContent,
    IonCardHeader,
    IonCardSubtitle,
    IonCardTitle
  }
};
</script>