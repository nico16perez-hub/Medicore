<template>
  <comp-page :titulo="'Historia Clínica'" :mostrar_actualizar="true" @actualizar="cargar">
    <section class="historia-encabezado">
      <div>
        <p class="historia-etiqueta">{{ es_doctor ? 'ATENCIÓN DEL PACIENTE' : 'TU SALUD' }}</p>
        <h2>{{ es_doctor ? 'Historia por paciente' : 'Un registro claro de tu salud' }}</h2>
        <p v-if="es_doctor">Revisá las consultas y mediciones de las personas asignadas a tu atención.</p>
        <p v-else>Encontrá tus mediciones y notas clínicas ordenadas de la más reciente a la más antigua.</p>
      </div>
      <span class="historia-icono" aria-hidden="true">{{ es_doctor ? '＋' : '♡' }}</span>
    </section>

    <section v-if="es_doctor" class="paciente-seleccion">
      <label for="paciente-historia">Paciente</label>
      <select
        id="paciente-historia"
        v-model.number="paciente_seleccionado_id"
        :disabled="cargando_pacientes || pacientes_asignados.length === 0"
        @change="cambiar_paciente"
      >
        <option :value="null" disabled>
          {{ cargando_pacientes ? 'Cargando pacientes…' : 'Seleccioná un paciente' }}
        </option>
        <option v-for="paciente in pacientes_asignados" :key="paciente.usuario_id" :value="paciente.usuario_id">
          {{ paciente.email }}
        </option>
      </select>
      <p v-if="paciente_seleccionado?.proximo_turno">
        Próximo turno: {{ formatear_fecha(paciente_seleccionado.proximo_turno) }}
      </p>
    </section>

    <div v-if="pacientes_error" class="mensaje estado-error">
      <p>{{ pacientes_error }}</p>
      <ion-button fill="outline" @click="cargar_pacientes">Reintentar</ion-button>
    </div>

    <div class="encabezado-abm">
      <ion-button v-if="puede_editar" :disabled="es_doctor && !paciente_seleccionado_id" @click="abrir_formulario">
        {{ editando ? 'Modificar registro' : 'Agregar registro' }}
      </ion-button>
    </div>

    <div class="filtros">
      <input
        v-model="filtro_busqueda"
        class="filtro-input"
        type="search"
        placeholder="Buscar por tipo, valor o notas"
        @input="aplicar_filtros"
      >
      <select v-model="filtro_tipo" class="filtro-select" @change="aplicar_filtros">
        <option value="Todos">Todos los tipos</option>
        <option v-for="tipo in tipos_registro" :key="tipo" :value="tipo">{{ tipo }}</option>
      </select>
    </div>

    <ion-modal :is-open="mostrar_formulario" @didDismiss="cerrar_formulario">
      <ion-header>
        <ion-toolbar>
          <ion-title>{{ editando ? 'Modificar registro' : 'Nuevo registro' }}</ion-title>
          <ion-buttons slot="end"><ion-button @click="cerrar_formulario">Cerrar</ion-button></ion-buttons>
        </ion-toolbar>
      </ion-header>
      <ion-content class="modal-contenido">
        <form class="formulario" @submit.prevent="guardar">
          <p class="ayuda">Agregá una medición o nota clínica.</p>
          <input
            v-if="es_doctor"
            :value="paciente_seleccionado?.email || ''"
            type="text"
            aria-label="Paciente asociado"
            readonly
          >
          <input v-model="formulario.fecha" type="date" required>
          <input v-model="formulario.tipo" placeholder="Tipo" required maxlength="80">
          <input v-model="formulario.valor" placeholder="Valor" required maxlength="80">
          <input v-model="formulario.notas" placeholder="Notas" required maxlength="240">
          <p v-if="form_error" class="error">{{ form_error }}</p>
          <div class="acciones">
            <ion-button expand="block" type="submit">{{ editando ? 'Guardar cambios' : 'Agregar registro' }}</ion-button>
            <ion-button expand="block" fill="clear" type="button" @click="cerrar_formulario">Cancelar</ion-button>
          </div>
        </form>
      </ion-content>
    </ion-modal>

    <comp-esqueleto v-if="cargando" />
    <div v-else-if="error" class="estado estado-error">
      <ion-text color="danger">
        <h2>No se pudo cargar la historia clínica</h2>
      </ion-text>
      <p>{{ error }}</p>
      <ion-button @click="cargar">Reintentar</ion-button>
    </div>
    <div v-else-if="es_doctor && !paciente_seleccionado_id" class="estado">
      <h2>{{ pacientes_asignados.length ? 'Seleccioná un paciente' : 'Todavía no tenés pacientes asignados' }}</h2>
      <p>
        {{ pacientes_asignados.length
          ? 'Al elegirlo vas a ver su historia clínica completa y cronológica.'
          : 'Los pacientes aparecerán aquí cuando secretaría confirme un turno a tu nombre.' }}
      </p>
    </div>
    <div v-else-if="registros.length === 0" class="estado">
      <div class="estado-icono">✓</div>
      <h2>{{ es_doctor ? 'La historia está lista para empezar' : 'Todavía no hay registros clínicos' }}</h2>
      <p>
        {{ es_doctor
          ? 'Este paciente aún no tiene registros en su historia clínica.'
          : 'Cuando el equipo de salud registre mediciones o notas, las vas a encontrar acá.' }}
      </p>
      <ion-button v-if="puede_editar && es_doctor" @click="abrir_formulario">Agregar primera nota</ion-button>
    </div>
    <section v-else class="linea-temporal">
      <article v-for="reg in registros" :key="reg.id" class="registro-clinico">
        <div class="registro-marca" aria-hidden="true"></div>
        <div class="registro-contenido">
          <div class="registro-meta">
            <span class="registro-tipo">{{ reg.tipo }}</span>
            <time>{{ formatear_fecha(reg.fecha) }}</time>
          </div>
          <strong class="registro-valor">{{ reg.valor }}</strong>
          <p>{{ reg.notas }}</p>
          <div v-if="puede_editar" class="registro-acciones">
            <ion-button size="small" fill="clear" @click="editar(reg)">Editar</ion-button>
            <ion-button size="small" fill="clear" color="danger" @click="eliminar(reg.id)">Borrar</ion-button>
          </div>
        </div>
      </article>
    </section>

    <div v-if="registros.length > 0" class="paginacion">
      <ion-button fill="clear" :disabled="pagina_actual <= 1 || cargando" @click="ir_a_pagina(-1)">Anterior</ion-button>
      <span class="detalle-pagina">Página {{ pagina_actual }} {{ total_paginas ? `de ${total_paginas}` : '' }}</span>
      <ion-button fill="clear" :disabled="!puede_siguiente || cargando" @click="ir_a_pagina(1)">Siguiente</ion-button>
    </div>
  </comp-page>
</template>

<script>
import { IonButton, IonButtons, IonContent, IonHeader, IonModal, IonText, IonTitle, IonToolbar } from '@ionic/vue';
import comp_esqueleto from '@/componets/base/comp_esqueleto.vue';
import comp_page from '@/componets/estructura/comp_page.vue';
import { obtener_pacientes_historia_service } from '@/services/historia_service';
import { historia_store } from '@/stores/historia_store';
import { sesion_store } from '@/stores/sesion_store';

export default {
  name: 'historia_page',
  components: {
    CompEsqueleto: comp_esqueleto,
    CompPage: comp_page,
    IonButton,
    IonButtons,
    IonContent,
    IonHeader,
    IonModal,
    IonText,
    IonTitle,
    IonToolbar
  },
  data() {
    return {
      store: historia_store,
      sesion_store,
      mostrar_formulario: false,
      editando: null,
      form_error: '',
      filtro_busqueda: '',
      filtro_tipo: 'Todos',
      pacientes_asignados: [],
      paciente_seleccionado_id: null,
      cargando_pacientes: false,
      pacientes_error: '',
      tipos_registro: ['Glucemia', 'Presion Arterial', 'Peso', 'Colesterol', 'Actividad'],
      formulario: {
        paciente_usuario_id: null,
        doctor_usuario_id: null,
        fecha: '',
        tipo: '',
        valor: '',
        notas: ''
      }
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
    rol() {
      return this.sesion_store.usuario?.rol?.codigo;
    },
    es_doctor() {
      return this.rol === 'doctor';
    },
    puede_editar() {
      return this.es_doctor;
    },
    paciente_seleccionado() {
      return this.pacientes_asignados.find(
        paciente => paciente.usuario_id === this.paciente_seleccionado_id
      ) || null;
    }
  },
  async mounted() {
    if (this.es_doctor) {
      const pacienteId = Number(this.$route.query.pacienteUsuarioId);
      this.paciente_seleccionado_id = Number.isInteger(pacienteId) && pacienteId > 0
        ? pacienteId
        : null;
      await this.cargar_pacientes();
    } else {
      await this.cargar();
    }
  },
  methods: {
    cargar_pacientes: async function() {
      this.cargando_pacientes = true;
      this.pacientes_error = '';
      try {
        this.pacientes_asignados = await obtener_pacientes_historia_service();
        if (!this.pacientes_asignados.some(item => item.usuario_id === this.paciente_seleccionado_id)) {
          this.paciente_seleccionado_id = this.pacientes_asignados[0]?.usuario_id || null;
        }
        if (this.paciente_seleccionado_id) {
          await this.cargar();
        } else {
          this.store.registros = [];
          this.store.error = '';
          this.store.total = 0;
        }
      } catch (error) {
        this.pacientes_error = error instanceof Error
          ? error.message
          : 'No se pudieron cargar los pacientes asignados.';
      } finally {
        this.cargando_pacientes = false;
      }
    },
    cargar: async function() {
      if (this.es_doctor && !this.paciente_seleccionado_id) return;
      await this.store.cargar_registros({
        pagina: 1,
        busqueda: this.filtro_busqueda,
        tipo: this.filtro_tipo,
        pacienteUsuarioId: this.paciente_seleccionado_id
      });
    },
    cambiar_paciente: async function() {
      this.filtro_busqueda = '';
      this.filtro_tipo = 'Todos';
      await this.cargar();
    },
    abrir_formulario: function() {
      this.form_error = '';
      this.formulario.paciente_usuario_id = this.paciente_seleccionado_id;
      this.mostrar_formulario = true;
    },
    cerrar_formulario: function() {
      this.editando = null;
      this.mostrar_formulario = false;
      this.formulario = {
        paciente_usuario_id: null,
        doctor_usuario_id: null,
        fecha: '',
        tipo: '',
        valor: '',
        notas: ''
      };
      this.form_error = '';
    },
    editar: function(registro) {
      this.editando = registro.id;
      this.formulario = { ...registro };
      this.form_error = '';
      this.mostrar_formulario = true;
    },
    cancelar_edicion: function() {
      this.editando = null;
      this.mostrar_formulario = false;
      this.formulario = {
        paciente_usuario_id: null,
        doctor_usuario_id: null,
        fecha: '',
        tipo: '',
        valor: '',
        notas: ''
      };
      this.form_error = '';
    },
    aplicar_filtros: async function() {
      await this.store.cargar_registros({
        pagina: 1,
        busqueda: this.filtro_busqueda,
        tipo: this.filtro_tipo,
        pacienteUsuarioId: this.paciente_seleccionado_id
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

      await this.store.cargar_registros({
        pagina: pagina_siguiente,
        busqueda: this.filtro_busqueda,
        tipo: this.filtro_tipo,
        pacienteUsuarioId: this.paciente_seleccionado_id
      });
    },
    guardar: async function() {
      this.form_error = '';
      const payload = {
        ...this.formulario,
        paciente_usuario_id: this.es_doctor
          ? this.paciente_seleccionado_id
          : this.formulario.paciente_usuario_id,
        tipo: String(this.formulario.tipo || '').trim(),
        valor: String(this.formulario.valor || '').trim(),
        notas: String(this.formulario.notas || '').trim()
      };

      try {
        if (this.editando) {
          await this.store.modificar_registro(this.editando, payload);
        } else {
          await this.store.crear_registro(payload);
        }
        this.cancelar_edicion();
      } catch (error) {
        this.form_error = error instanceof Error ? error.message : 'No se pudo guardar el registro.';
      }
    },
    eliminar: async function(id) {
      if (!window.confirm('¿Eliminar este registro de historia clínica?')) return;
      try {
        await this.store.eliminar_registro(id);
      } catch (error) {
        this.form_error = error instanceof Error ? error.message : 'No se pudo eliminar el registro.';
      }
    },
    formatear_fecha: function(fecha) {
      if (!fecha) return '';
      const [anio, mes, dia] = String(fecha).split('-');
      return `${dia}/${mes}/${anio}`;
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

.historia-encabezado {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 16px;
  margin: 20px 16px;
  padding: 24px;
  border: 1px solid rgba(var(--ion-color-primary-rgb), 0.14);
  border-radius: 20px;
  background: linear-gradient(125deg, rgba(var(--ion-color-primary-rgb), 0.12), rgba(var(--ion-color-primary-rgb), 0.035));
}

.historia-encabezado h2 {
  margin: 0 0 8px;
  font-size: clamp(1.3rem, 4vw, 1.8rem);
  font-weight: 750;
  letter-spacing: -0.035em;
}

.historia-encabezado p {
  margin: 0;
  max-width: 44rem;
  color: var(--ion-color-medium);
  line-height: 1.5;
}

.historia-encabezado .historia-etiqueta {
  margin-bottom: 8px;
  color: var(--ion-color-primary);
  font-size: 0.72rem;
  font-weight: 750;
  letter-spacing: 0.12em;
}

.historia-icono,
.estado-icono {
  display: grid;
  flex: 0 0 50px;
  width: 50px;
  height: 50px;
  place-items: center;
  border-radius: 16px;
  background: rgba(var(--ion-color-primary-rgb), 0.12);
  color: var(--ion-color-primary);
  font-size: 1.7rem;
  font-weight: 700;
}

.paciente-seleccion {
  display: grid;
  gap: 8px;
  max-width: 640px;
  margin: 0 16px 20px;
  padding: 18px;
  border: 1px solid var(--ion-color-step-150);
  border-radius: 16px;
  background: var(--ion-card-background, var(--ion-background-color));
  box-shadow: 0 8px 24px rgba(20, 50, 60, 0.05);
}

.paciente-seleccion label {
  font-size: 0.82rem;
  font-weight: 700;
}

.paciente-seleccion select {
  min-height: 48px;
  border: 1px solid var(--ion-color-step-250);
  border-radius: 10px;
  padding: 10px 12px;
  background: var(--ion-background-color);
  color: var(--ion-text-color);
  font: inherit;
}

.paciente-seleccion p {
  margin: 0;
  color: var(--ion-color-primary);
  font-size: 0.9rem;
  font-weight: 650;
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

.linea-temporal {
  position: relative;
  max-width: 900px;
  margin: 12px 16px 28px;
}

.linea-temporal::before {
  position: absolute;
  top: 12px;
  bottom: 18px;
  left: 7px;
  width: 2px;
  background: var(--ion-color-step-150);
  content: '';
}

.registro-clinico {
  position: relative;
  display: grid;
  grid-template-columns: 16px minmax(0, 1fr);
  gap: 16px;
  padding: 8px 0 18px;
}

.registro-marca {
  z-index: 1;
  width: 16px;
  height: 16px;
  margin-top: 6px;
  border: 4px solid var(--ion-background-color);
  border-radius: 50%;
  background: var(--ion-color-primary);
  box-shadow: 0 0 0 1px rgba(var(--ion-color-primary-rgb), 0.35);
}

.registro-contenido {
  min-width: 0;
  padding: 18px 20px;
  border: 1px solid var(--ion-color-step-150);
  border-radius: 16px;
  background: var(--ion-card-background, var(--ion-background-color));
  box-shadow: 0 8px 24px rgba(20, 50, 60, 0.045);
}

.registro-meta {
  display: flex;
  flex-wrap: wrap;
  align-items: center;
  justify-content: space-between;
  gap: 8px;
  margin-bottom: 12px;
}

.registro-tipo {
  padding: 5px 10px;
  border-radius: 999px;
  background: rgba(var(--ion-color-primary-rgb), 0.1);
  color: var(--ion-color-primary);
  font-size: 0.76rem;
  font-weight: 700;
}

.registro-meta time {
  color: var(--ion-color-medium);
  font-size: 0.82rem;
}

.registro-valor {
  display: block;
  font-size: 1.16rem;
}

.registro-contenido > p {
  margin: 8px 0 0;
  color: var(--ion-color-medium);
  line-height: 1.55;
}

.registro-acciones {
  display: flex;
  justify-content: flex-end;
  margin-top: 8px;
}

.mensaje {
  margin: 12px 16px;
  padding: 12px 16px;
  border-radius: 12px;
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

  .historia-encabezado {
    margin: 14px 12px;
    padding: 18px;
  }

  .historia-icono {
    flex-basis: 40px;
    width: 40px;
    height: 40px;
  }

  .paciente-seleccion,
  .linea-temporal {
    margin-right: 12px;
    margin-left: 12px;
  }

  .registro-contenido {
    padding: 15px;
  }
}
</style>