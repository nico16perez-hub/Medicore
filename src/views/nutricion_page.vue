<template>
  <comp-page :titulo="'Plan Nutricional'" :mostrar_actualizar="true" @actualizar="cargar">
    <div class="encabezado-abm">
      <ion-button v-if="es_doctor" @click="abrir_formulario">
        {{ editando ? 'Modificar item' : 'Agregar item' }}
      </ion-button>
    </div>

    <div class="filtros">
      <input
        v-model="filtro_busqueda"
        class="filtro-input"
        type="search"
        placeholder="Buscar por día, comida o detalle"
        @input="aplicar_filtros"
      >
      <select v-model="filtro_dia" class="filtro-select" @change="aplicar_filtros">
        <option value="Todos">Todos los días</option>
        <option v-for="dia in dias_plan" :key="dia" :value="dia">{{ dia }}</option>
      </select>
    </div>

    <ion-modal :is-open="mostrar_formulario" @didDismiss="cerrar_formulario">
      <ion-header>
        <ion-toolbar>
          <ion-title>{{ editando ? 'Modificar item' : 'Nuevo item' }}</ion-title>
          <ion-buttons slot="end"><ion-button @click="cerrar_formulario">Cerrar</ion-button></ion-buttons>
        </ion-toolbar>
      </ion-header>
      <ion-content class="modal-contenido">
        <form class="formulario" @submit.prevent="guardar">
          <p class="ayuda">Completá los datos de la comida.</p>
          <input v-model="formulario.dia" placeholder="Día" required maxlength="30">
          <input v-model="formulario.comida" placeholder="Comida" required maxlength="60">
          <input v-model="formulario.detalle" placeholder="Detalle" required maxlength="240">
          <p v-if="form_error" class="error">{{ form_error }}</p>
          <div class="acciones">
            <ion-button expand="block" type="submit">{{ editando ? 'Guardar cambios' : 'Agregar item' }}</ion-button>
            <ion-button expand="block" fill="clear" type="button" @click="cerrar_formulario">Cancelar</ion-button>
          </div>
        </form>
      </ion-content>
    </ion-modal>

    <comp-esqueleto v-if="cargando" />
    <div v-else-if="error" class="estado estado-error">
      <ion-text color="danger">
        <h2>No se pudo cargar el plan nutricional</h2>
      </ion-text>
      <p>{{ error }}</p>
      <ion-button @click="cargar">Reintentar</ion-button>
    </div>
    <div v-else-if="plan.length === 0" class="estado">
      <h2>No hay items con ese filtro</h2>
      <p>Probá con otros criterios de búsqueda o agregá un nuevo item.</p>
      <ion-button fill="outline" @click="cargar">Actualizar</ion-button>
    </div>
    <ion-list v-else>
      <ion-item v-for="item in plan" :key="item.id">
        <ion-label>
          <h3>{{ item.comida }} ({{ item.dia }})</h3>
          <p>{{ item.detalle }}</p>
        </ion-label>
        <ion-button v-if="es_doctor" fill="clear" @click="editar(item)">Editar</ion-button>
        <ion-button v-if="es_doctor" fill="clear" color="danger" @click="eliminar(item.id)">Borrar</ion-button>
      </ion-item>
    </ion-list>

    <div v-if="plan.length > 0" class="paginacion">
      <ion-button fill="clear" :disabled="pagina_actual <= 1 || cargando" @click="ir_a_pagina(-1)">Anterior</ion-button>
      <span class="detalle-pagina">Página {{ pagina_actual }} {{ total_paginas ? `de ${total_paginas}` : '' }}</span>
      <ion-button fill="clear" :disabled="!puede_siguiente || cargando" @click="ir_a_pagina(1)">Siguiente</ion-button>
    </div>
  </comp-page>
</template>

<script>
import { IonButton, IonButtons, IonContent, IonHeader, IonItem, IonLabel, IonList, IonModal, IonText, IonTitle, IonToolbar } from '@ionic/vue';
import comp_esqueleto from '@/componets/base/comp_esqueleto.vue';
import comp_page from '@/componets/estructura/comp_page.vue';
import { nutricion_store } from '@/stores/nutricion_store';
import { sesion_store } from '@/stores/sesion_store';

export default {
  name: 'nutricion_page',
  components: {
    CompEsqueleto: comp_esqueleto,
    CompPage: comp_page,
    IonButton,
    IonButtons,
    IonContent,
    IonHeader,
    IonItem,
    IonLabel,
    IonList,
    IonModal,
    IonText,
    IonTitle,
    IonToolbar
  },
  data() {
    return {
      store: nutricion_store,
      sesion_store,
      mostrar_formulario: false,
      editando: null,
      form_error: '',
      filtro_busqueda: '',
      filtro_dia: 'Todos',
      dias_plan: ['Lunes', 'Martes', 'Miércoles', 'Jueves', 'Viernes', 'Sábado', 'Domingo'],
      formulario: { dia: '', comida: '', detalle: '' }
    };
  },
  computed: {
    cargando() {
      return this.store.cargando;
    },
    error() {
      return this.store.error;
    },
    plan() {
      return this.store.plan;
    },
    pagina_actual() {
      return this.store.paginaActual;
    },
    total_paginas() {
      return this.store.totalPaginas;
    },
    puede_siguiente() {
      return this.pagina_actual < this.total_paginas;
    },
    es_doctor() {
      return this.sesion_store.usuario?.rol?.codigo === 'doctor';
    }
  },
  mounted() {
    this.cargar();
  },
  methods: {
    cargar: async function() {
      await this.store.cargar_plan({
        pagina: 1,
        busqueda: this.filtro_busqueda,
        dia: this.filtro_dia
      });
    },
    abrir_formulario: function() {
      this.form_error = '';
      this.mostrar_formulario = true;
    },
    cerrar_formulario: function() {
      this.editando = null;
      this.mostrar_formulario = false;
      this.formulario = { dia: '', comida: '', detalle: '' };
      this.form_error = '';
    },
    editar: function(item) {
      this.editando = item.id;
      this.formulario = { ...item };
      this.form_error = '';
      this.mostrar_formulario = true;
    },
    cancelar_edicion: function() {
      this.editando = null;
      this.mostrar_formulario = false;
      this.formulario = { dia: '', comida: '', detalle: '' };
      this.form_error = '';
    },
    aplicar_filtros: async function() {
      await this.store.cargar_plan({
        pagina: 1,
        busqueda: this.filtro_busqueda,
        dia: this.filtro_dia
      });
    },
    ir_a_pagina: async function(delta) {
      const pagina_siguiente = this.pagina_actual + delta;
      if (pagina_siguiente < 1) {
        return;
      }

      if (this.total_paginas > 0 && pagina_siguiente > this.total_paginas) {
        return;
      }

      await this.store.cargar_plan({
        pagina: pagina_siguiente,
        busqueda: this.filtro_busqueda,
        dia: this.filtro_dia
      });
    },
    guardar: async function() {
      this.form_error = '';
      const payload = {
        ...this.formulario,
        dia: String(this.formulario.dia || '').trim(),
        comida: String(this.formulario.comida || '').trim(),
        detalle: String(this.formulario.detalle || '').trim()
      };

      try {
        if (this.editando) {
          await this.store.modificar_item(this.editando, payload);
        } else {
          await this.store.crear_item(payload);
        }
        this.cancelar_edicion();
      } catch (error) {
        this.form_error = error instanceof Error ? error.message : 'No se pudo guardar el item.';
      }
    },
    eliminar: async function(id) {
      if (!window.confirm('¿Eliminar este item del plan nutricional?')) return;
      try {
        await this.store.eliminar_item(id);
      } catch (error) {
        this.form_error = error instanceof Error ? error.message : 'No se pudo eliminar el item.';
      }
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

.formulario {
  display: grid;
  gap: 10px;
  padding: 20px 16px 28px;
}

.encabezado-abm {
  display: flex;
  justify-content: flex-end;
  padding: 16px 16px 8px;
}

.ayuda {
  margin: 0 0 4px;
  color: var(--ion-color-medium);
}

.formulario input,
.formulario select,
.filtro-input,
.filtro-select {
  border: 1px solid var(--ion-color-medium);
  border-radius: 6px;
  min-height: 48px;
  padding: 12px;
  background: var(--ion-color-light);
}

.filtros {
  display: grid;
  grid-template-columns: minmax(0, 2fr) minmax(180px, 240px);
  gap: 12px;
  padding: 0 16px 12px;
}

.acciones {
  display: flex;
  gap: 8px;
}

.error {
  color: var(--ion-color-danger);
}

.modal-contenido {
  --background: var(--ion-color-light);
}

.paginacion {
  display: flex;
  justify-content: space-between;
  align-items: center;
  gap: 8px;
  padding: 12px 16px 20px;
}

.detalle-pagina {
  font-size: 0.92rem;
  color: var(--ion-color-medium);
}

@media (max-width: 520px) {
  .filtros {
    grid-template-columns: 1fr;
  }
}
</style>