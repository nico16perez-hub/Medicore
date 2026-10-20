<template>
  <div class="contenedor-notificaciones">
    <transition-group name="notificacion" tag="div" class="lista-notificaciones">
      <div
        v-for="(notif, index) in notificaciones_activas"
        :key="notif.id + '-' + index"
        :class="[
          'notificacion-card',
          notif.tipo || 'info',
          { 'no-leida': !notif.leida }
        ]"
        @click="ir_a_notificaciones"
        @mouseenter="pausar_temporizador(index)"
        @mouseleave="reanudar_temporizador(index)"
      >
        <div class="notificacion-icono">
          <ion-icon :icon="icono_para(notif.tipo)" />
        </div>
        <div class="notificacion-contenido">
          <div class="notificacion-titulo">{{ notif.titulo || 'Nueva notificación' }}</div>
          <div class="notificacion-mensaje">{{ notif.mensaje || notif.body || 'Tienes un aviso nuevo en MediCore.' }}</div>
          <div class="notificacion-fecha">{{ notif.fecha || formatear_fecha(notif.creada_en_utc || notif.fecha) }}</div>
        </div>
        <button
          class="notificacion-cerrar"
          @click.stop="cerrar(index)"
          aria-label="Cerrar notificación"
        >
          <ion-icon icon="close-outline" />
        </button>
        <div
          v-if="!pausado[index]"
          class="notificacion-progreso"
          :style="{ width: progreso[index] + '%' }"
        ></div>
      </div>
    </transition-group>
    <div v-if="contador_oculto > 0" class="contador-oculto">+{{ contador_oculto }} más</div>
  </div>
</template>

<script>
import { reactive, ref, onMounted, onUnmounted, nextTick } from 'vue';
import { IonIcon } from '@ionic/vue';
import { notificaciones_store } from '@/stores/notificaciones_store';
import router from '@/router';

export default {
  name: 'compNotificacionesFlotantes',
  components: { IonIcon },
  props: {
    maximo_visibles: { type: Number, default: 4 },
    duracion_segundos: { type: Number, default: 6 }
  },
  setup(props) {
    const notificaciones_activas = reactive([]);
    const pausado = reactive({});
    const progreso = reactive({});
    let contador_temporizadores = {};
    const contador_oculto = ref(0);

    const icono_para = (tipo) => {
      const map = {
        exito: 'checkmark-circle-outline',
        advertencia: 'alert-outline',
        error: 'close-circle-outline',
        turno: 'calendar-outline',
        mensaje: 'chatbubble-outline',
        sistema: 'information-circle-outline'
      };
      return map[tipo] || 'notifications-outline';
    };

    const formatear_fecha = (valor) => {
      if (!valor) return '';
      const fecha = new Date(valor);
      if (Number.isNaN(fecha.getTime())) return valor;
      return fecha.toLocaleString('es-AR', {
        day: '2-digit', month: '2-digit', hour: '2-digit', minute: '2-digit'
      });
    };

    const agregar_notificacion = (notif) => {
      const id_unico = notif.id || Date.now() + Math.random();
      if (notificaciones_activas.find(n => n.id === id_unico)) return;
      const entrada = {
        id: id_unico,
        titulo: notif.titulo || 'Nueva notificación',
        mensaje: notif.mensaje || notif.body || '',
        tipo: notif.tipo || 'info',
        leida: notif.leida !== undefined ? notif.leida : false,
        fecha: notif.fecha || '',
        creada_en_utc: notif.creada_en_utc || ''
      };
      notificaciones_activas.unshift(entrada);
      pausado[id_unico] = false;
      progreso[id_unico] = 100;
      actualizar_contador_oculto();
      iniciar_temporizador(id_unico);
      setTimeout(() => remover_notificacion(id_unico), props.duracion_segundos * 1000 + 400);
    };

    const iniciar_temporizador = (id) => {
      if (contador_temporizadores[id]) clearInterval(contador_temporizadores[id]);
      const inicio = Date.now();
      const total_ms = props.duracion_segundos * 1000;
      contador_temporizadores[id] = setInterval(() => {
        if (pausado[id]) return;
        const transcurrido = Date.now() - inicio;
        const pct = Math.max(0, 100 - (transcurrido / total_ms) * 100);
        progreso[id] = pct;
        if (pct <= 0) {
          clearInterval(contador_temporizadores[id]);
          remover_notificacion(id);
        }
      }, 100);
    };

    const pausar_temporizador = (index) => { pausado[notificaciones_activas[index].id] = true; };
    const reanudar_temporizador = (index) => { pausado[notificaciones_activas[index].id] = false; };

    const cerrar = (index) => remover_notificacion(notificaciones_activas[index].id);

    const remover_notificacion = (id) => {
      const idx = notificaciones_activas.findIndex(n => n.id === id);
      if (idx === -1) return;
      if (contador_temporizadores[id]) {
        clearInterval(contador_temporizadores[id]);
        delete contador_temporizadores[id];
      }
      notificaciones_activas.splice(idx, 1);
      delete pausado[id];
      delete progreso[id];
      actualizar_contador_oculto();
    };

    const actualizar_contador_oculto = () => {
      const total_store = notificaciones_store.notificaciones?.length || 0;
      contador_oculto.value = Math.max(0, total_store - notificaciones_activas.length);
    };

    const ir_a_notificaciones = () => {
      try { router.push({ name: 'notificaciones' }); } catch (e) {}
    };

    const manejar_push_recibido = () => {
      nextTick(() => {
        notificaciones_store.cargar_total_no_leidas().then(() => {
          const ultima = notificaciones_store.notificaciones?.[0];
          if (ultima) agregar_notificacion({ ...ultima, tipo: 'mensaje', fecha: new Date().toISOString() });
        });
      });
    };

    const cargar_iniciales = async () => {
      try {
        await notificaciones_store.cargar_total_no_leidas();
        const recientes = notificaciones_store.notificaciones?.slice(0, props.maximo_visibles) || [];
        recientes.forEach(n => agregar_notificacion({ ...n, tipo: 'mensaje' }));
      } catch (e) {}
    };

    onMounted(() => {
      cargar_iniciales();
      window.addEventListener('medicore:push-recibido', manejar_push_recibido);
    });

    onUnmounted(() => {
      window.removeEventListener('medicore:push-recibido', manejar_push_recibido);
      Object.keys(contador_temporizadores).forEach(id => clearInterval(contador_temporizadores[id]));
    });

    return {
      notificaciones_activas, pausado, progreso, contador_oculto,
      icono_para, formatear_fecha, cerrar, ir_a_notificaciones
    };
  }
};
</script>

<style scoped>
.contenedor-notificaciones {
  position: fixed;
  top: 20px;
  right: 20px;
  z-index: 9999;
  display: flex;
  flex-direction: column;
  gap: 10px;
  pointer-events: none;
  width: min(420px, 92vw);
}

.lista-notificaciones {
  display: flex;
  flex-direction: column;
  gap: 10px;
  width: 100%;
}

.notificacion-card {
  pointer-events: auto;
  background: #ffffff;
  border-radius: 12px;
  box-shadow: 0 8px 24px rgba(0, 0, 0, 0.16), 0 2px 6px rgba(0, 0, 0, 0.1);
  border: 1px solid rgba(0, 0, 0, 0.06);
  border-left: 5px solid var(--ion-color-primary, #3880ff);
  padding: 14px 16px;
  display: flex;
  align-items: flex-start;
  gap: 12px;
  position: relative;
  overflow: hidden;
  color: #1a1a1a;
  font-family: inherit;
  /* Entrada */
  opacity: 0;
  transform: translateX(120px) scale(0.95);
  transition: all 0.35s cubic-bezier(0.16, 1, 0.3, 1);
}

.notificacion-card.notificacion-enter-from,
.notificacion-card.notificacion-enter-to {
  opacity: 1;
  transform: translateX(0) scale(1);
}

/* Salida */
.notificacion-card.notificacion-leave-from {
  opacity: 1;
  transform: translateX(0) scale(1);
}

.notificacion-card.notificacion-leave-to {
  opacity: 0;
  transform: translateX(100px) scale(0.95);
  height: 0;
  margin: 0;
  padding: 0;
}

.notificacion-card.no-leida {
  border-left-color: var(--ion-color-danger, #f04141);
  background: #fff7f7;
}

.notificacion-card.exito { border-left-color: var(--ion-color-success, #2dd07a); }
.notificacion-card.advertencia { border-left-color: var(--ion-color-warning, #ffd43d); }
.notificacion-card.error { border-left-color: var(--ion-color-danger, #f04141); }
.notificacion-card.turno { border-left-color: var(--ion-color-tertiary, #7c4dff); }

.notificacion-icono {
  flex-shrink: 0;
  width: 36px;
  height: 36px;
  border-radius: 10px;
  display: grid;
  place-items: center;
  background: rgba(56, 128, 255, 0.12);
  color: var(--ion-color-primary, #3880ff);
  font-size: 1.15rem;
}

.notificacion-card.no-leida .notificacion-icono {
  background: rgba(240, 65, 65, 0.12);
  color: var(--ion-color-danger, #f04141);
}

.notificacion-card.exito .notificacion-icono {
  background: rgba(45, 208, 122, 0.12);
  color: var(--ion-color-success, #2dd07a);
}

.notificacion-card.advertencia .notificacion-icono {
  background: rgba(255, 212, 61, 0.12);
  color: var(--ion-color-warning, #ffd43d);
}

.notificacion-contenido { flex: 1; min-width: 0; }

.notificacion-titulo {
  font-weight: 700;
  font-size: 0.92rem;
  margin: 0 0 4px 0;
  color: #1a1a1a;
  line-height: 1.25;
}

.notificacion-mensaje {
  font-size: 0.82rem;
  margin: 0 0 6px 0;
  color: #4a4a4a;
  line-height: 1.35;
  overflow: hidden;
  text-overflow: ellipsis;
  display: -webkit-box;
  -webkit-line-clamp: 2;
  -webkit-box-orient: vertical;
}

.notificacion-fecha {
  font-size: 0.72rem;
  color: #8a8a8a;
  margin: 0;
}

.notificacion-cerrar {
  flex-shrink: 0;
  background: transparent;
  border: none;
  color: #b0b0b0;
  font-size: 1.1rem;
  cursor: pointer;
  padding: 4px;
  border-radius: 6px;
  line-height: 1;
  transition: all 0.2s;
  display: grid;
  place-items: center;
}

.notificacion-cerrar:hover {
  color: #e03131;
  background: rgba(224, 49, 49, 0.08);
}

.notificacion-progreso {
  position: absolute;
  bottom: 0;
  left: 0;
  height: 3px;
  background: rgba(0, 0, 0, 0.08);
  width: 100%;
}

.notificacion-progreso::before {
  content: '';
  position: absolute;
  top: 0;
  left: 0;
  height: 100%;
  background: var(--ion-color-primary, #3880ff);
  width: var(--progreso-ancho, 100%);
  transition: width 0.1s linear;
}

.contador-oculto {
  position: absolute;
  top: -10px;
  right: -10px;
  background: var(--ion-color-danger, #f04141);
  color: #ffffff;
  font-size: 0.72rem;
  font-weight: 700;
  padding: 2px 8px;
  border-radius: 12px;
  box-shadow: 0 2px 6px rgba(0, 0, 0, 0.2);
  z-index: 1;
}

@media (max-width: 600px) {
  .contenedor-notificaciones { top: 10px; right: 10px; left: 10px; width: auto; }
  .notificacion-card { width: 100%; }
}
</style>