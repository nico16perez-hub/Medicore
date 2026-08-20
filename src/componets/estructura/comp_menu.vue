<template>
  <ion-menu :content-id="content_id" type="overlay" class="menu-lateral">
    <ion-content>
      <div class="menu-encabezado">
        <div class="marca-icono">M</div>
        <div>
          <h2>MediCore</h2>
          <p>Panel sanatorial</p>
        </div>
      </div>

      <ion-list v-for="grupo in grupos_menu" :key="grupo.id" lines="none">
        <ion-list-header v-if="grupo.titulo">{{ grupo.titulo }}</ion-list-header>
        <ion-menu-toggle v-for="item in grupo.items" :key="item.id" :auto-hide="false">
          <ion-item
            button
            class="menu-item"
            :detail="false"
            :router-link="item.ruta"
            router-direction="root"
          >
            <ion-icon slot="start" :icon="item.icono" />
            <ion-label>{{ item.titulo }}</ion-label>
          </ion-item>
        </ion-menu-toggle>
      </ion-list>
    </ion-content>
  </ion-menu>
</template>

<script>
import {
  IonContent,
  IonIcon,
  IonItem,
  IonLabel,
  IonList,
  IonListHeader,
  IonMenu,
  IonMenuToggle
} from '@ionic/vue';
import { obtener_grupos_menu } from '@/config/navegacion';

export default {
  name: 'comp_menu',
  components: {
    IonContent,
    IonIcon,
    IonItem,
    IonLabel,
    IonList,
    IonListHeader,
    IonMenu,
    IonMenuToggle
  },
  props: {
    content_id: {
      type: String,
      required: true
    }
  },
  computed: {
    grupos_menu(){
      return obtener_grupos_menu();
    }
  }
};
</script>

<style scoped>
.menu-lateral {
  --width: min(82vw, 310px);
  --background: #101418;
}

.menu-lateral ion-content {
  --background: #101418;
  --color: #f3f6f8;
}

.menu-encabezado {
  display: flex;
  align-items: center;
  gap: 12px;
  padding: 24px 18px 18px;
  border-bottom: 1px solid rgba(255, 255, 255, 0.08);
}

.marca-icono {
  display: grid;
  place-items: center;
  width: 42px;
  height: 42px;
  border-radius: 8px;
  color: #ffffff;
  background: #1f8a70;
  font-size: 1.15rem;
  font-weight: 700;
}

.menu-encabezado h2,
.menu-encabezado p {
  margin: 0;
}

.menu-encabezado h2 {
  color: #ffffff;
  font-size: 1.05rem;
  font-weight: 700;
}

.menu-encabezado p {
  margin-top: 3px;
  color: rgba(243, 246, 248, 0.64);
  font-size: 0.78rem;
}

ion-list {
  padding: 10px 8px;
  background: transparent;
}

ion-list + ion-list {
  border-top: 1px solid rgba(255, 255, 255, 0.08);
}

ion-list-header {
  min-height: 30px;
  padding-inline: 10px;
  color: rgba(243, 246, 248, 0.62);
  font-size: 0.72rem;
  font-weight: 700;
  letter-spacing: 0;
}

.menu-item {
  --min-height: 44px;
  --padding-start: 10px;
  --inner-padding-end: 10px;
  --background: transparent;
  --background-hover: rgba(255, 255, 255, 0.08);
  --background-activated: rgba(31, 138, 112, 0.22);
  --color: rgba(243, 246, 248, 0.86);
  --border-radius: 8px;
  margin: 2px 0;
  font-size: 0.88rem;
}

.menu-item ion-icon {
  margin-right: 12px;
  color: rgba(243, 246, 248, 0.72);
  font-size: 1.05rem;
}

.menu-item.router-link-active,
.menu-item.router-link-exact-active {
  --background: rgba(31, 138, 112, 0.18);
  --color: #ffffff;
}

.menu-item.router-link-active ion-icon,
.menu-item.router-link-exact-active ion-icon {
  color: #39c7a6;
}
</style>
