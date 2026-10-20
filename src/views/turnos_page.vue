<template>
  <comp-page :titulo="titulo_pagina" :mostrar_actualizar="true" @actualizar="cargar">
    <section class="turnos-intro">
      <p class="turnos-etiqueta">{{ etiqueta_turnos }}</p>
      <h2>{{ encabezado_turnos }}</h2>
      <p>{{ descripcion_turnos }}</p>
    </section>

    <div class="encabezado-abm">
      <ion-button v-if="puede_solicitar || es_secretaria" @click="abrir_formulario">
        {{ es_secretaria ? (editando ? 'Modificar turno' : 'Agregar turno') : 'Solicitar turno' }}
      </ion-button>
      <ion-button v-if="es_secretaria" fill="outline" @click="abrir_agenda">
        Horarios de atención
      </ion-button>
      <ion-button fill="outline" @click="escanear_qr">Escanear QR de turno</ion-button>
    </div>

    <div class="filtros">
      <input
        v-model="filtro_busqueda"
        class="filtro-input"
        type="search"
        placeholder="Buscar por profesional, especialidad o estado"
        @input="aplicar_filtros"
      >
      <select v-model="filtro_estado" class="filtro-select" @change="aplicar_filtros">
        <option value="Todos">Todos los estados</option>
        <option v-for="estado in estados_turno" :key="estado" :value="estado">
          {{ estado }}
        </option>
      </select>
    </div>
    <ion-item v-if="detalle_error && !mostrar_detalle">
      <ion-label color="danger">{{ detalle_error }}</ion-label>
    </ion-item>
    <ion-item v-if="form_message">
      <ion-label>{{ form_message }}</ion-label>
    </ion-item>

    <ion-modal :is-open="mostrar_formulario" @didDismiss="cerrar_formulario">
      <ion-header>
        <ion-toolbar>
          <ion-title>{{ editando ? 'Modificar turno' : 'Nuevo turno' }}</ion-title>
          <ion-buttons slot="end">
            <ion-button @click="cerrar_formulario">Cerrar</ion-button>
          </ion-buttons>
        </ion-toolbar>
      </ion-header>
      <ion-content class="modal-contenido">
        <form class="formulario" @submit.prevent="guardar">
          <p class="ayuda">
            {{ puede_solicitar
              ? 'Elegí un área y un profesional. La secretaría confirmará un horario disponible.'
              : 'Completá los datos del turno.' }}
          </p>
          <label v-if="es_secretaria" for="paciente-turno">Paciente</label>
          <select
            v-if="es_secretaria"
            id="paciente-turno"
            v-model.number="formulario.paciente_usuario_id"
            required
          >
            <option :value="null" disabled>Seleccioná un paciente</option>
            <option v-for="paciente in pacientes_agenda" :key="paciente.id" :value="paciente.id">
              {{ paciente.email }}
            </option>
          </select>
          <label v-if="es_secretaria && editando" for="doctor-turno">Doctor asignado</label>
          <select
            v-if="es_secretaria && editando"
            id="doctor-turno"
            v-model.number="formulario.doctor_usuario_id"
            required
            @change="actualizar_doctor_edicion"
          >
            <option :value="null" disabled>Seleccioná un doctor</option>
            <option v-for="doctor in doctores_agenda" :key="doctor.id" :value="doctor.id">
              {{ doctor.email }}
            </option>
          </select>
          <input
            v-if="es_secretaria && editando"
            v-model="formulario.profesional"
            placeholder="Profesional"
            required
            maxlength="120"
          >
          <label v-if="puede_solicitar || (es_secretaria && !editando)" for="especialidad-turno">
            Área o especialidad
          </label>
          <select
            v-if="puede_solicitar || (es_secretaria && !editando)"
            id="especialidad-turno"
            v-model="formulario.especialidad"
            required
            @change="cambiar_especialidad_turno"
          >
            <option value="" disabled>Seleccioná un área</option>
            <option v-for="especialidad in especialidades_turno" :key="especialidad" :value="especialidad">
              {{ especialidad }}
            </option>
          </select>
          <input
            v-if="es_secretaria && editando"
            v-model="formulario.especialidad"
            placeholder="Especialidad"
            required
            maxlength="120"
          >
          <label v-if="puede_solicitar" for="doctor-preferido-turno">Profesional</label>
          <label v-else-if="es_secretaria && !editando" for="doctor-preferido-turno">Doctor</label>
          <select
            v-if="puede_solicitar"
            id="doctor-preferido-turno"
            v-model.number="formulario.doctor_preferido_usuario_id"
            required
            :disabled="!formulario.especialidad || cargando_directorio"
          >
            <option :value="null" disabled>
              {{ cargando_directorio ? 'Cargando profesionales...' : 'Seleccioná un profesional' }}
            </option>
            <option v-for="doctor in doctores_de_especialidad" :key="doctor.id" :value="doctor.id">
              {{ doctor.email }}
            </option>
          </select>
          <select
            v-if="es_secretaria && !editando"
            id="doctor-preferido-turno"
            v-model.number="formulario.doctor_usuario_id"
            required
            :disabled="!formulario.especialidad"
            @change="cargar_horarios_nuevo_turno"
          >
            <option :value="null" disabled>Seleccioná un doctor</option>
            <option v-for="doctor in doctores_de_especialidad" :key="doctor.id" :value="doctor.id">
              {{ doctor.email }}
            </option>
          </select>
          <p
            v-if="puede_solicitar && formulario.especialidad &&
              !doctores_de_especialidad.length && !cargando_directorio"
            class="error"
          >
            No hay profesionales registrados para esta especialidad. Consultá con secretaría.
          </p>
          <p v-if="puede_solicitar && !especialidades_turno.length && !cargando_directorio" class="error">
            Secretaría todavía no configuró las áreas de atención de los profesionales.
          </p>
          <template v-if="es_secretaria && !editando">
            <label for="fecha-nuevo-turno">Día de atención</label>
            <select
              id="fecha-nuevo-turno"
              v-model="formulario.fecha"
              required
              :disabled="cargando_horarios_nuevo || !dias_nuevo_turno.length"
              @change="formulario.hora = ''"
            >
              <option value="" disabled>
                {{ cargando_horarios_nuevo ? 'Buscando días...' : 'Seleccioná un día' }}
              </option>
              <option v-for="dia in dias_nuevo_turno" :key="dia.fecha" :value="dia.fecha">
                {{ dia.etiqueta }} · {{ dia.cantidad }} horarios
              </option>
            </select>
            <label v-if="formulario.fecha">Horario (turnos de 30 minutos)</label>
            <div v-if="formulario.fecha" class="horarios-disponibles">
              <ion-button
                v-for="slot in horarios_nuevo_turno_dia"
                :key="slot.hora"
                size="small"
                :fill="formulario.hora === normalizar_hora(slot.hora) ? 'solid' : 'outline'"
                @click="formulario.hora = normalizar_hora(slot.hora)"
              >
                {{ normalizar_hora(slot.hora) }}
              </ion-button>
            </div>
            <p v-if="error_horarios_nuevo" class="error">{{ error_horarios_nuevo }}</p>
          </template>
          <template v-else>
            <label v-if="puede_solicitar" for="fecha-preferida">Fecha preferida (sujeta a confirmación)</label>
            <label v-else-if="es_secretaria" for="fecha-edicion">Fecha</label>
            <input
              :id="es_secretaria ? 'fecha-edicion' : 'fecha-preferida'"
              v-model="formulario.fecha"
              type="date"
              :min="puede_solicitar ? fecha_hoy : undefined"
              required
            >
            <label v-if="puede_solicitar" for="hora-preferida">Horario preferido (sujeto a confirmación)</label>
            <label v-else-if="es_secretaria" for="hora-edicion">Horario</label>
            <input
              :id="es_secretaria ? 'hora-edicion' : 'hora-preferida'"
              v-model="formulario.hora"
              type="time"
              required
            >
          </template>
          <select v-if="es_secretaria && editando" v-model="formulario.estado" required>
            <option v-for="estado in estados_turno" :key="estado" :value="estado">
              {{ estado }}
            </option>
          </select>
          <p v-if="form_error" class="error">{{ form_error }}</p>
          <div class="acciones">
            <ion-button
              expand="block"
              type="submit"
              :disabled="cargando_directorio || (puede_solicitar && !formulario.doctor_preferido_usuario_id)"
            >
              {{ es_secretaria ? (editando ? 'Guardar cambios' : 'Agregar turno') : 'Enviar solicitud' }}
            </ion-button>
            <ion-button expand="block" fill="clear" type="button" @click="cerrar_formulario">Cancelar</ion-button>
          </div>
        </form>
      </ion-content>
    </ion-modal>

    <comp-esqueleto v-if="cargando" />
    <div v-else-if="error" class="estado estado-error">
      <ion-text color="danger">
        <h2>No se pudieron cargar los turnos</h2>
      </ion-text>
      <p>{{ error }}</p>
      <ion-button @click="cargar">Reintentar</ion-button>
    </div>
    <div v-else-if="turnos.length === 0" class="estado">
      <h2>No hay turnos con ese filtro</h2>
      <p>Probá con otros criterios de búsqueda o agregá un nuevo turno.</p>
      <ion-button fill="outline" @click="cargar">Actualizar</ion-button>
    </div>
    <ion-list v-else class="lista-turnos">
      <ion-item v-for="turno in turnos" :key="turno.id" class="turno-item">
        <ion-label>
          <h3>{{ turno.especialidad }} - {{ turno.profesional }}</h3>
          <p>{{ formatear_fecha(turno.fecha) }} a las {{ formatear_hora(turno.hora) }}</p>
          <p v-if="(es_secretaria || rol === 'doctor') && turno.paciente_email">
            <strong>Paciente:</strong> {{ turno.paciente_email }}
          </p>
          <p v-if="(es_secretaria || rol === 'doctor') && turno.paciente_usuario_id && !turno.paciente_email">
            <strong>Paciente:</strong> Usuario #{{ turno.paciente_usuario_id }}
          </p>
          <p v-if="turno.doctor_email || turno.doctor_preferido_email">
            <strong>{{ turno.doctor_email ? 'Doctor:' : 'Profesional solicitado:' }}</strong>
            {{ turno.doctor_email || turno.doctor_preferido_email }}
          </p>
          <p><strong>Estado:</strong> {{ turno.estado }}</p>
        </ion-label>
        <ion-button
          v-if="es_secretaria && turno.estado?.toLowerCase() === 'pendiente' && turno.paciente_usuario_id"
          fill="outline"
          @click="abrir_asignacion(turno)"
        >
          Asignar horario
        </ion-button>
        <ion-button
          v-if="rol === 'doctor' && turno.paciente_usuario_id"
          fill="outline"
          @click="abrir_historia_paciente(turno.paciente_usuario_id)"
        >
          Historia clínica
        </ion-button>
        <ion-button fill="clear" @click="abrir_detalle(turno)">Detalle y QR</ion-button>
        <ion-button v-if="es_secretaria" fill="clear" @click="editar(turno)">Editar</ion-button>
        <ion-button v-if="es_secretaria" fill="clear" color="danger" @click="eliminar(turno.id)">Cancelar</ion-button>
      </ion-item>
    </ion-list>

    <div v-if="turnos.length > 0" class="paginacion">
      <ion-button fill="clear" :disabled="pagina_actual <= 1 || cargando" @click="ir_a_pagina(-1)">
        Anterior
      </ion-button>
      <span class="detalle-pagina">Página {{ pagina_actual }} {{ total_paginas ? `de ${total_paginas}` : '' }}</span>
      <ion-button fill="clear" :disabled="!puede_siguiente || cargando" @click="ir_a_pagina(1)">
        Siguiente
      </ion-button>
    </div>

    <ion-modal :is-open="mostrar_asignacion" @didDismiss="cerrar_asignacion">
      <ion-header>
        <ion-toolbar>
          <ion-title>Asignar turno</ion-title>
          <ion-buttons slot="end">
            <ion-button @click="cerrar_asignacion">Cerrar</ion-button>
          </ion-buttons>
        </ion-toolbar>
      </ion-header>
      <ion-content class="modal-contenido">
        <form class="formulario" @submit.prevent="confirmar_asignacion">
          <p v-if="turno_a_asignar" class="ayuda">
            Solicitud #{{ turno_a_asignar.id }} para
            {{ turno_a_asignar.paciente_email || `usuario #${turno_a_asignar.paciente_usuario_id}` }}.
            La fecha solicitada no queda confirmada hasta asignar un horario.
          </p>
          <label for="doctor-asignacion">Doctor</label>
          <select
            id="doctor-asignacion"
            v-model.number="asignacion.doctor_usuario_id"
            required
            @change="cambiar_doctor_asignacion"
          >
            <option :value="null" disabled>Seleccioná un doctor</option>
            <option v-for="doctor in doctores_asignacion" :key="doctor.id" :value="doctor.id">
              {{ doctor.email }}
            </option>
          </select>
          <label for="fecha-asignacion">Día de atención</label>
          <select
            id="fecha-asignacion"
            v-model="asignacion.fecha"
            required
            :disabled="cargando_horarios || !dias_con_horarios.length"
            @change="asignacion.hora = ''"
          >
            <option value="" disabled>
              {{ cargando_horarios ? 'Buscando días...' : 'Seleccioná un día' }}
            </option>
            <option v-for="dia in dias_con_horarios" :key="dia.fecha" :value="dia.fecha">
              {{ dia.etiqueta }} · {{ dia.cantidad }} horarios
            </option>
          </select>
          <label v-if="asignacion.fecha">Horario (turnos de 30 minutos)</label>
          <div v-if="asignacion.fecha" class="horarios-disponibles">
            <ion-button
              v-for="slot in horarios_del_dia"
              :key="slot.hora"
              size="small"
              :fill="asignacion.hora === normalizar_hora(slot.hora) ? 'solid' : 'outline'"
              @click="asignacion.hora = normalizar_hora(slot.hora)"
            >
              {{ normalizar_hora(slot.hora) }}
            </ion-button>
          </div>
          <p v-if="horarios_error" class="error">{{ horarios_error }}</p>
          <p v-if="asignacion_error" class="error">{{ asignacion_error }}</p>
          <div class="acciones">
            <ion-button
              expand="block"
              type="submit"
              :disabled="cargando_horarios || !asignacion.fecha || !asignacion.hora"
            >
              Confirmar y notificar
            </ion-button>
            <ion-button expand="block" fill="clear" type="button" @click="cerrar_asignacion">
              Cancelar
            </ion-button>
          </div>
        </form>
      </ion-content>
    </ion-modal>

    <ion-modal :is-open="mostrar_agenda" @didDismiss="cerrar_agenda">
      <ion-header>
        <ion-toolbar>
          <ion-title>Horarios de atención</ion-title>
          <ion-buttons slot="end">
            <ion-button @click="cerrar_agenda">Cerrar</ion-button>
          </ion-buttons>
        </ion-toolbar>
      </ion-header>
      <ion-content class="modal-contenido">
        <form class="formulario" @submit.prevent="guardar_agenda">
          <p class="ayuda">Configurá los días y rangos semanales de cada doctor. Un turno dura 30 minutos; podés cargar más de un rango por día.</p>
          <label for="doctor-agenda">Doctor</label>
          <select
            id="doctor-agenda"
            v-model.number="doctor_agenda_id"
            required
            @change="cargar_agenda_doctor"
          >
            <option :value="null" disabled>Seleccioná un doctor</option>
            <option v-for="doctor in doctores_agenda" :key="doctor.id" :value="doctor.id">
              {{ doctor.email }}
            </option>
          </select>
          <label v-if="doctor_agenda_id" for="especialidades-doctor-agenda">Áreas de atención</label>
          <p v-if="doctor_agenda_id" class="ayuda">
            Separá las especialidades con coma. El profesional aparecerá en cada área seleccionada.
          </p>
          <input
            v-if="doctor_agenda_id"
            id="especialidades-doctor-agenda"
            v-model="especialidades_doctor_texto"
            placeholder="Ej.: Pediatría, Clínica médica"
            maxlength="600"
          >
          <ion-button
            v-if="doctor_agenda_id"
            expand="block"
            type="button"
            :disabled="guardando_especialidades"
            @click="guardar_especialidades"
          >
            Guardar áreas de atención
          </ion-button>
          <div v-for="(franja, indice) in agenda_semanal" :key="indice" class="franja-agenda">
            <select v-model.number="franja.dia_semana" aria-label="Día de atención">
              <option v-for="dia in dias_semana" :key="dia.id" :value="dia.id">{{ dia.nombre }}</option>
            </select>
            <input v-model="franja.hora_inicio" type="time" aria-label="Hora de inicio" required>
            <input v-model="franja.hora_fin" type="time" aria-label="Hora de finalización" required>
            <ion-button fill="clear" color="danger" type="button" @click="quitar_franja(indice)">
              Quitar
            </ion-button>
          </div>
          <ion-button fill="outline" type="button" @click="agregar_franja">
            Agregar rango
          </ion-button>
          <p v-if="agenda_error" class="error">{{ agenda_error }}</p>
          <p v-if="agenda_mensaje">{{ agenda_mensaje }}</p>
          <ion-button expand="block" type="submit" :disabled="!doctor_agenda_id || guardando_agenda">
            Guardar horarios
          </ion-button>
        </form>
      </ion-content>
    </ion-modal>

    <ion-modal :is-open="mostrar_detalle" @didDismiss="cerrar_detalle">
      <ion-header>
        <ion-toolbar>
          <ion-title>Detalle del turno</ion-title>
          <ion-buttons slot="end">
            <ion-button @click="cerrar_detalle">Cerrar</ion-button>
          </ion-buttons>
        </ion-toolbar>
      </ion-header>
      <ion-content class="detalle-contenido">
        <ion-card v-if="turno_seleccionado">
          <ion-card-header>
            <ion-card-title>{{ turno_seleccionado.especialidad }}</ion-card-title>
            <ion-card-subtitle>Turno #{{ turno_seleccionado.id }}</ion-card-subtitle>
          </ion-card-header>
          <ion-card-content>
            <p>
              <strong>Profesional:</strong>
              {{ turno_seleccionado.doctor_email || turno_seleccionado.doctor_preferido_email || turno_seleccionado.profesional }}
            </p>
            <p><strong>Fecha:</strong> {{ formatear_fecha(turno_seleccionado.fecha) }}</p>
            <p><strong>Hora:</strong> {{ formatear_hora(turno_seleccionado.hora) }}</p>
            <p><strong>Estado:</strong> {{ turno_seleccionado.estado }}</p>
            <p class="qr-ayuda">Este QR contiene únicamente el identificador estable del turno.</p>
            <img v-if="qr_url" class="qr-imagen" :src="qr_url" alt="Código QR del turno">
            <p v-if="detalle_error" class="error">{{ detalle_error }}</p>
            <ion-button
              v-if="turno_seleccionado.estado?.toLowerCase() === 'confirmado' && !es_navegador"
              expand="block"
              @click="compartir_comprobante"
            >
              Compartir comprobante
            </ion-button>
            <ion-button
              v-if="turno_seleccionado.estado?.toLowerCase() === 'confirmado' && es_navegador"
              expand="block"
              @click="descargar_comprobante"
            >
              Descargar comprobante PDF
            </ion-button>
            <ion-button expand="block" fill="clear" @click="cerrar_detalle">Cerrar</ion-button>
          </ion-card-content>
        </ion-card>
      </ion-content>
    </ion-modal>
  </comp-page>
</template>

<script>
import { IonButton, IonButtons, IonCard, IonCardContent, IonCardHeader, IonCardSubtitle, IonCardTitle, IonContent, IonHeader, IonItem, IonLabel, IonList, IonModal, IonText, IonTitle, IonToolbar } from '@ionic/vue';
import { BarcodeFormat, BarcodeScanner } from '@capacitor-mlkit/barcode-scanning';
import { Directory, Filesystem } from '@capacitor/filesystem';
import { Capacitor } from '@capacitor/core';
import { Share } from '@capacitor/share';
import QRCode from 'qrcode';
import comp_esqueleto from '@/componets/base/comp_esqueleto.vue';
import comp_page from '@/componets/estructura/comp_page.vue';
import { turnos_store } from '@/stores/turnos_store';
import { sesion_store } from '@/stores/sesion_store';
import { obtener_comprobante_service } from '@/services/comprobantes_service';
import { obtener_turno_service } from '@/services/turnos_service';
import { crear_contenido_qr_turno, leer_id_qr_turno } from '@/services/turno_qr_service';
import { respuesta_haptica } from '@/services/preferencias_service';
import { notificar_solicitud_turno_local_service } from '@/services/notificaciones_locales_service';
import {
  guardar_disponibilidad_doctor_service,
  obtener_disponibilidad_doctor_service,
  obtener_doctores_agenda_service,
  obtener_doctores_directorio_service,
  obtener_especialidades_doctor_service,
  guardar_especialidades_doctor_service,
  obtener_horarios_disponibles_service,
  obtener_pacientes_agenda_service
} from '@/services/agenda_service';

export default {
  name: 'turnos_page',
  components: {
    CompEsqueleto: comp_esqueleto,
    CompPage: comp_page,
    IonButton,
    IonButtons,
    IonCard,
    IonCardContent,
    IonCardHeader,
    IonCardSubtitle,
    IonCardTitle,
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
      store: turnos_store,
      sesion_store,
      mostrar_formulario: false,
      editando: null,
      form_error: '',
      form_message: '',
      mostrar_asignacion: false,
      turno_a_asignar: null,
      asignacion: { doctor_usuario_id: null, fecha: '', hora: '' },
      asignacion_error: '',
      horarios_error: '',
      cargando_horarios: false,
      doctores_agenda: [],
      doctores_directorio: [],
      cargando_directorio: false,
      horarios_disponibles: [],
      mostrar_agenda: false,
      doctor_agenda_id: null,
      agenda_semanal: [],
      agenda_error: '',
      agenda_mensaje: '',
      guardando_agenda: false,
      especialidades_doctor_texto: '',
      guardando_especialidades: false,
      pacientes_agenda: [],
      horarios_nuevo_turno: [],
      cargando_horarios_nuevo: false,
      error_horarios_nuevo: '',
      dias_semana: [
        { id: 0, nombre: 'Domingo' },
        { id: 1, nombre: 'Lunes' },
        { id: 2, nombre: 'Martes' },
        { id: 3, nombre: 'Miércoles' },
        { id: 4, nombre: 'Jueves' },
        { id: 5, nombre: 'Viernes' },
        { id: 6, nombre: 'Sábado' }
      ],
      filtro_busqueda: '',
      filtro_estado: 'Todos',
      estados_turno: ['Pendiente', 'Confirmado', 'Reprogramado', 'Cancelado'],
      formulario: this.formulario_vacio(),
      mostrar_detalle: false,
      turno_seleccionado: null,
      qr_url: '',
      detalle_error: '',
      es_navegador: !Capacitor.isNativePlatform()
    };
  },
  computed: {
    cargando() {
      return this.store.cargando;
    },
    error() {
      return this.store.error;
    },
    turnos() {
      return this.store.turnos;
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
    es_secretaria() {
      return this.rol === 'secretaria';
    },
    puede_solicitar() {
      return this.rol === 'paciente';
    },
    titulo_pagina() {
      if (this.es_secretaria) return 'Gestión de turnos';
      if (this.rol === 'doctor') return 'Mi agenda';
      return 'Mis turnos';
    },
    etiqueta_turnos() {
      if (this.es_secretaria) return 'COORDINACIÓN DE ATENCIÓN';
      if (this.rol === 'doctor') return 'AGENDA PROFESIONAL';
      return 'ATENCIÓN PARA VOS';
    },
    encabezado_turnos() {
      if (this.es_secretaria) return 'Solicitudes y agenda médica';
      if (this.rol === 'doctor') return 'Tus próximas consultas';
      return 'Tus consultas, más simples';
    },
    descripcion_turnos() {
      if (this.es_secretaria) {
        return 'Revisá primero las solicitudes pendientes y asigná un horario de la agenda del doctor.';
      }
      if (this.rol === 'doctor') {
        return 'Consultá tu agenda y prepará cada consulta con la historia clínica del paciente.';
      }
      return 'Pedí un turno con la especialidad que necesitás y secretaría te confirmará el horario.';
    },
    dias_con_horarios() {
      const dias = new Map();
      for (const slot of this.horarios_disponibles) {
        if (!dias.has(slot.fecha)) {
          dias.set(slot.fecha, { fecha: slot.fecha, cantidad: 0 });
        }
        dias.get(slot.fecha).cantidad += 1;
      }
      return [...dias.values()].map(dia => ({
        ...dia,
        etiqueta: this.formatear_fecha_completa(dia.fecha)
      }));
    },
    horarios_del_dia() {
      return this.horarios_disponibles.filter(slot => slot.fecha === this.asignacion.fecha);
    },
    dias_nuevo_turno() {
      const dias = new Map();
      for (const slot of this.horarios_nuevo_turno) {
        if (!dias.has(slot.fecha)) {
          dias.set(slot.fecha, { fecha: slot.fecha, cantidad: 0 });
        }
        dias.get(slot.fecha).cantidad += 1;
      }
      return [...dias.values()].map(dia => ({
        ...dia,
        etiqueta: this.formatear_fecha_completa(dia.fecha)
      }));
    },
    horarios_nuevo_turno_dia() {
      return this.horarios_nuevo_turno.filter(slot => slot.fecha === this.formulario.fecha);
    },
    fecha_hoy() {
      return fecha_local_hoy();
    },
    especialidades_turno() {
      const doctores = this.es_secretaria ? this.doctores_agenda : this.doctores_directorio;
      return [...new Set(doctores.flatMap(doctor => doctor.especialidades || []))]
        .sort((a, b) => a.localeCompare(b, 'es'));
    },
    doctores_de_especialidad() {
      const doctores = this.es_secretaria ? this.doctores_agenda : this.doctores_directorio;
      return doctores.filter(doctor =>
        (doctor.especialidades || []).some(especialidad =>
          especialidad.localeCompare(this.formulario.especialidad || '', 'es', {
            sensitivity: 'accent'
          }) === 0
        )
      );
    },
    doctores_asignacion() {
      const especialidad = this.turno_a_asignar?.especialidad;
      if (!especialidad) return [];
      return this.doctores_agenda.filter(doctor =>
        (doctor.especialidades || []).some(area =>
          area.localeCompare(especialidad, 'es', { sensitivity: 'accent' }) === 0
        )
      );
    }
  },
  mounted() {
    this.cargar();
  },
  methods: {
    cargar: async function() {
      await this.store.cargar_turnos({
        pagina: 1,
        busqueda: this.filtro_busqueda,
        estado: this.filtro_estado
      });
    },
    formulario_vacio: function() {
      return {
        paciente_usuario_id: null,
        doctor_usuario_id: null,
        doctor_preferido_usuario_id: null,
        profesional: '',
        especialidad: '',
        fecha: '',
        hora: '',
        estado: 'Pendiente'
      };
    },
    actualizar_doctor_edicion: function() {
      const doctor = this.doctores_agenda.find(
        item => item.id === this.formulario.doctor_usuario_id
      );
      if (doctor) this.formulario.profesional = doctor.email;
    },
    abrir_formulario: async function() {
      this.form_error = '';
      this.form_message = '';
      this.error_horarios_nuevo = '';
      this.horarios_nuevo_turno = [];
      this.formulario = this.formulario_vacio();
      if (this.puede_solicitar) {
        this.formulario.profesional = 'A asignar por secretaría';
        this.formulario.fecha = fecha_local_hoy();
        await this.cargar_directorio_doctores();
      }
      if (this.es_secretaria) {
        this.formulario.estado = 'Confirmado';
        await Promise.all([this.cargar_doctores(), this.cargar_pacientes_agenda()]);
        if (!this.doctores_agenda.length || !this.pacientes_agenda.length) {
          this.form_error = 'Para agendar necesitás al menos un doctor activo y un paciente registrado.';
        }
      }
      this.mostrar_formulario = true;
    },
    cargar_directorio_doctores: async function() {
      this.cargando_directorio = true;
      try {
        this.doctores_directorio = await obtener_doctores_directorio_service();
      } catch (error) {
        this.form_error = error instanceof Error
          ? error.message
          : 'No se pudieron cargar los profesionales disponibles.';
      } finally {
        this.cargando_directorio = false;
      }
    },
    cambiar_especialidad_turno: function() {
      this.formulario.doctor_usuario_id = null;
      this.formulario.doctor_preferido_usuario_id = null;
      this.formulario.fecha = this.puede_solicitar ? fecha_local_hoy() : '';
      this.formulario.hora = '';
      this.horarios_nuevo_turno = [];
      this.error_horarios_nuevo = '';
    },
    cerrar_formulario: function() {
      this.editando = null;
      this.mostrar_formulario = false;
      this.formulario = this.formulario_vacio();
      this.form_error = '';
    },
    editar: async function(turno) {
      this.editando = turno.id;
      if (this.es_secretaria) {
        await Promise.all([this.cargar_doctores(), this.cargar_pacientes_agenda()]);
      }
      this.formulario = {
        ...turno,
        hora: this.normalizar_hora(turno.hora)
      };
      this.form_error = '';
      this.mostrar_formulario = true;
    },
    cancelar_edicion: function() {
      this.editando = null;
      this.mostrar_formulario = false;
      this.formulario = this.formulario_vacio();
      this.form_error = '';
    },
    abrir_detalle: async function(turno) {
      this.turno_seleccionado = turno;
      this.mostrar_detalle = true;
      this.detalle_error = '';
      this.qr_url = '';
      try {
        this.qr_url = await QRCode.toDataURL(crear_contenido_qr_turno(turno.id), {
          errorCorrectionLevel: 'Q',
          margin: 2,
          width: 220
        });
      } catch (error) {
        this.detalle_error = error instanceof Error ? error.message : 'No se pudo generar el código QR.';
      }
    },
    abrir_historia_paciente: function(paciente_usuario_id) {
      this.$router.push({
        path: '/app/historia',
        query: { pacienteUsuarioId: String(paciente_usuario_id) }
      });
    },
    cerrar_detalle: function() {
      this.mostrar_detalle = false;
      this.turno_seleccionado = null;
      this.qr_url = '';
      this.detalle_error = '';
    },
    escanear_qr: async function() {
      this.detalle_error = '';
      try {
        const compatibilidad = await BarcodeScanner.isSupported();
        if (!compatibilidad.supported) {
          throw new Error('El escaneo QR no está disponible en este dispositivo o navegador.');
        }
        let permisos = await BarcodeScanner.checkPermissions();
        if (permisos.camera !== 'granted') {
          permisos = await BarcodeScanner.requestPermissions();
        }
        if (permisos.camera !== 'granted') {
          throw new Error('Se denegó el permiso de cámara para escanear códigos QR.');
        }
        const resultado = await BarcodeScanner.scan({ formats: [BarcodeFormat.QrCode] });
        const contenido = resultado.barcodes[0]?.rawValue;
        if (!contenido) return;
        const turno_id = leer_id_qr_turno(contenido);
        if (turno_id === null) {
          throw new Error('El código escaneado no es un identificador de turno de MediCore válido.');
        }
        const turno = await obtener_turno_service(turno_id);
        await respuesta_haptica('confirmacion');
        await this.abrir_detalle(turno);
      } catch (error) {
        if (!es_cancelacion_escaneo(error)) {
          this.detalle_error = error instanceof Error ? error.message : 'No se pudo leer el código QR.';
          await respuesta_haptica('error');
        }
      }
    },
    descargar_comprobante: async function() {
      if (!this.turno_seleccionado || !this.turno_confirmado()) return;
      this.detalle_error = '';
      try {
        const pdf = await obtener_comprobante_service(this.turno_seleccionado.id);
        const url = URL.createObjectURL(pdf);
        const enlace = document.createElement('a');
        enlace.href = url;
        enlace.download = `comprobante-turno-${this.turno_seleccionado.id}.pdf`;
        enlace.click();
        URL.revokeObjectURL(url);
      } catch (error) {
        this.detalle_error = error instanceof Error ? error.message : 'No se pudo obtener el comprobante.';
        await respuesta_haptica('error');
      }
    },
    compartir_comprobante: async function() {
      if (!this.turno_seleccionado || !this.turno_confirmado()) return;
      this.detalle_error = '';
      const ruta = `comprobante-turno-${this.turno_seleccionado.id}-${Date.now()}.pdf`;
      try {
        const pdf = await obtener_comprobante_service(this.turno_seleccionado.id);
        const data = await blob_a_base64(pdf);
        const archivo = await Filesystem.writeFile({
          path: ruta,
          data,
          directory: Directory.Cache,
          recursive: true
        });
        try {
          await Share.share({
            title: 'Comprobante de turno MediCore',
            text: `Comprobante del turno #${this.turno_seleccionado.id}`,
            url: archivo.uri,
            dialogTitle: 'Compartir comprobante'
          });
          await respuesta_haptica('confirmacion');
        } finally {
          await Filesystem.deleteFile({ path: ruta, directory: Directory.Cache });
        }
      } catch (error) {
        this.detalle_error = error instanceof Error ? error.message : 'No se pudo compartir el comprobante.';
        await respuesta_haptica('error');
      }
    },
    turno_confirmado: function() {
      return this.turno_seleccionado?.estado?.toLowerCase() === 'confirmado';
    },
    aplicar_filtros: async function() {
      await this.store.cargar_turnos({
        pagina: 1,
        busqueda: this.filtro_busqueda,
        estado: this.filtro_estado
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

      await this.store.cargar_turnos({
        pagina: pagina_siguiente,
        busqueda: this.filtro_busqueda,
        estado: this.filtro_estado
      });
    },
    formatear_fecha: function(fecha) {
      if (typeof fecha !== 'string') {
        return fecha || '';
      }

      return fecha;
    },
    formatear_hora: function(hora) {
      if (typeof hora !== 'string') {
        return hora || '';
      }

      return hora.length > 5 ? hora.substring(0, 5) : hora;
    },
    normalizar_hora: function(hora) {
      return this.formatear_hora(hora || '');
    },
    guardar: async function() {
      this.form_error = '';
      this.form_message = '';
      const payload = {
        ...this.formulario,
        profesional: this.puede_solicitar
          ? 'A asignar por secretaría'
          : this.es_secretaria && !this.editando
            ? this.doctores_agenda.find(doctor =>
                doctor.id === this.formulario.doctor_usuario_id)?.email || ''
            : String(this.formulario.profesional || '').trim(),
        especialidad: String(this.formulario.especialidad || '').trim(),
        estado: this.es_secretaria && !this.editando
          ? 'Confirmado'
          : String(this.formulario.estado || '').trim(),
        hora: this.normalizar_hora(this.formulario.hora)
      };

      try {
        if (this.editando) {
          await this.store.modificar_turno(this.editando, payload);
        } else {
          const creado = await this.store.crear_turno(payload);
          if (this.puede_solicitar && creado?.id) {
            const resultado = await notificar_solicitud_turno_local_service(creado.id);
            this.form_message = resultado.ok
              ? 'La solicitud del turno fue registrada. ' + resultado.mensaje
              : `La solicitud fue registrada. ${resultado.mensaje}`;
          } else {
            this.form_message = 'El turno fue registrado correctamente.';
          }
        }
        await respuesta_haptica('confirmacion');
        this.cancelar_edicion();
      } catch (error) {
        this.form_error = error instanceof Error ? error.message : 'No se pudo guardar el turno.';
        await respuesta_haptica('error');
      }
    },
    cargar_doctores: async function() {
      try {
        this.doctores_agenda = await obtener_doctores_agenda_service();
      } catch (error) {
        const mensaje = error instanceof Error ? error.message : 'No se pudieron cargar los doctores.';
        this.agenda_error = mensaje;
        this.asignacion_error = mensaje;
        this.form_error = mensaje;
      }
    },
    cargar_pacientes_agenda: async function() {
      try {
        this.pacientes_agenda = await obtener_pacientes_agenda_service();
      } catch (error) {
        this.form_error = error instanceof Error
          ? error.message
          : 'No se pudieron cargar los pacientes registrados.';
      }
    },
    cargar_horarios_nuevo_turno: async function() {
      this.horarios_nuevo_turno = [];
      this.formulario.fecha = '';
      this.formulario.hora = '';
      this.error_horarios_nuevo = '';
      const doctorId = this.formulario.doctor_usuario_id;
      if (!doctorId) return;
      this.cargando_horarios_nuevo = true;
      try {
        this.horarios_nuevo_turno = await obtener_horarios_disponibles_service(
          doctorId,
          fecha_local_hoy()
        );
        if (!this.horarios_nuevo_turno.length) {
          this.error_horarios_nuevo = 'Este doctor no tiene horarios libres en las próximas cuatro semanas.';
        }
      } catch (error) {
        this.error_horarios_nuevo = error instanceof Error
          ? error.message
          : 'No se pudieron consultar los horarios disponibles.';
      } finally {
        this.cargando_horarios_nuevo = false;
      }
    },
    abrir_agenda: async function() {
      this.agenda_error = '';
      this.agenda_mensaje = '';
      this.mostrar_agenda = true;
      await this.cargar_doctores();
      if (!this.doctor_agenda_id && this.doctores_agenda.length) {
        this.doctor_agenda_id = this.doctores_agenda[0].id;
      }
      if (this.doctor_agenda_id) await this.cargar_agenda_doctor();
    },
    cerrar_agenda: function() {
      this.mostrar_agenda = false;
      this.agenda_error = '';
      this.agenda_mensaje = '';
    },
    cargar_agenda_doctor: async function() {
      if (!this.doctor_agenda_id) {
        this.agenda_semanal = [];
        this.especialidades_doctor_texto = '';
        return;
      }
      this.agenda_error = '';
      try {
        const [disponibilidad, especialidades] = await Promise.all([
          obtener_disponibilidad_doctor_service(this.doctor_agenda_id),
          obtener_especialidades_doctor_service(this.doctor_agenda_id)
        ]);
        this.agenda_semanal = disponibilidad.map(franja => ({
          dia_semana: franja.dia_semana,
          hora_inicio: this.normalizar_hora(franja.hora_inicio),
          hora_fin: this.normalizar_hora(franja.hora_fin)
        }));
        this.especialidades_doctor_texto = especialidades.join(', ');
      } catch (error) {
        this.agenda_error = error instanceof Error
          ? error.message
          : 'No se pudo cargar la agenda semanal.';
      }
    },
    guardar_especialidades: async function() {
      if (!this.doctor_agenda_id) return;
      this.agenda_error = '';
      this.agenda_mensaje = '';
      this.guardando_especialidades = true;
      try {
        const especialidades = this.especialidades_doctor_texto
          .split(',')
          .map(especialidad => especialidad.trim())
          .filter(Boolean);
        await guardar_especialidades_doctor_service(this.doctor_agenda_id, especialidades);
        await this.cargar_doctores();
        this.especialidades_doctor_texto = especialidades.join(', ');
        this.agenda_mensaje = 'Se guardaron las áreas de atención del doctor.';
        await respuesta_haptica('confirmacion');
      } catch (error) {
        this.agenda_error = error instanceof Error
          ? error.message
          : 'No se pudieron guardar las áreas de atención.';
        await respuesta_haptica('error');
      } finally {
        this.guardando_especialidades = false;
      }
    },
    agregar_franja: function() {
      this.agenda_semanal.push({
        dia_semana: 1,
        hora_inicio: '09:00',
        hora_fin: '13:00'
      });
    },
    quitar_franja: function(indice) {
      this.agenda_semanal.splice(indice, 1);
    },
    guardar_agenda: async function() {
      if (!this.doctor_agenda_id) return;
      this.agenda_error = '';
      this.agenda_mensaje = '';
      this.guardando_agenda = true;
      try {
        const disponibilidad = this.agenda_semanal.map(franja => ({
          dia_semana: Number(franja.dia_semana),
          hora_inicio: `${franja.hora_inicio}:00`,
          hora_fin: `${franja.hora_fin}:00`
        }));
        await guardar_disponibilidad_doctor_service(this.doctor_agenda_id, disponibilidad);
        this.agenda_mensaje = 'Se guardaron los horarios semanales del doctor.';
        await respuesta_haptica('confirmacion');
      } catch (error) {
        this.agenda_error = error instanceof Error
          ? error.message
          : 'No se pudo guardar la agenda semanal.';
        await respuesta_haptica('error');
      } finally {
        this.guardando_agenda = false;
      }
    },
    abrir_asignacion: async function(turno) {
      this.turno_a_asignar = turno;
      this.asignacion = { doctor_usuario_id: null, fecha: '', hora: '' };
      this.asignacion_error = '';
      this.horarios_error = '';
      this.horarios_disponibles = [];
      this.mostrar_asignacion = true;
      await this.cargar_doctores();
      if (this.doctores_asignacion.length) {
        const doctorPreferido = turno.doctor_preferido_usuario_id;
        this.asignacion.doctor_usuario_id = this.doctores_asignacion.some(
          doctor => doctor.id === doctorPreferido
        )
          ? doctorPreferido
          : this.doctores_asignacion[0].id;
        await this.cambiar_doctor_asignacion();
        const preferenciaFecha = turno.fecha;
        const preferenciaHora = this.normalizar_hora(turno.hora);
        const horarioPreferido = this.horarios_disponibles.find(slot =>
          slot.fecha === preferenciaFecha &&
          this.normalizar_hora(slot.hora) === preferenciaHora);
        if (horarioPreferido) {
          this.asignacion.fecha = horarioPreferido.fecha;
          this.asignacion.hora = preferenciaHora;
        } else if (this.dias_con_horarios.some(dia => dia.fecha === preferenciaFecha)) {
          this.asignacion.fecha = preferenciaFecha;
        }
      } else {
        this.asignacion_error = `No hay doctores configurados para ${turno.especialidad}. Configurá esa área desde Horarios de atención.`;
      }
    },
    cerrar_asignacion: function() {
      this.mostrar_asignacion = false;
      this.turno_a_asignar = null;
      this.asignacion = { doctor_usuario_id: null, fecha: '', hora: '' };
      this.horarios_disponibles = [];
      this.asignacion_error = '';
      this.horarios_error = '';
    },
    cambiar_doctor_asignacion: async function() {
      this.asignacion.fecha = '';
      this.asignacion.hora = '';
      this.horarios_disponibles = [];
      this.horarios_error = '';
      if (!this.asignacion.doctor_usuario_id) return;
      this.cargando_horarios = true;
      try {
        const hoy = new Date();
        const desde = `${hoy.getFullYear()}-${String(hoy.getMonth() + 1).padStart(2, '0')}-${String(hoy.getDate()).padStart(2, '0')}`;
        this.horarios_disponibles = await obtener_horarios_disponibles_service(
          this.asignacion.doctor_usuario_id,
          desde
        );
        if (!this.horarios_disponibles.length) {
          this.horarios_error = 'No hay horarios disponibles en los próximos 28 días. Revisá la agenda del doctor.';
        }
      } catch (error) {
        this.horarios_error = error instanceof Error
          ? error.message
          : 'No se pudieron consultar los horarios disponibles.';
      } finally {
        this.cargando_horarios = false;
      }
    },
    formatear_fecha_completa: function(fecha) {
      const [anio, mes, dia] = fecha.split('-').map(Number);
      const fecha_local = new Date(anio, mes - 1, dia);
      return fecha_local.toLocaleDateString('es-AR', {
        weekday: 'long',
        day: 'numeric',
        month: 'long'
      });
    },
    formatear_slot: function(slot) {
      return `${this.formatear_fecha_completa(slot.fecha)} a las ${this.normalizar_hora(slot.hora)}`;
    },
    confirmar_asignacion: async function() {
      if (!this.turno_a_asignar || !this.asignacion.fecha || !this.asignacion.hora) return;
      this.asignacion_error = '';
      const doctor = this.doctores_asignacion.find(item => item.id === this.asignacion.doctor_usuario_id);
      if (!doctor) {
        this.asignacion_error = 'Seleccioná un doctor válido.';
        return;
      }
      try {
        await this.store.modificar_turno(this.turno_a_asignar.id, {
          paciente_usuario_id: this.turno_a_asignar.paciente_usuario_id,
          doctor_usuario_id: doctor.id,
          profesional: doctor.email,
          especialidad: this.turno_a_asignar.especialidad,
          fecha: this.asignacion.fecha,
          hora: this.normalizar_hora(this.asignacion.hora),
          estado: 'Confirmado'
        });
        this.form_message = 'Turno asignado. Se notificó al paciente y al doctor.';
        this.cerrar_asignacion();
        await respuesta_haptica('confirmacion');
      } catch (error) {
        this.asignacion_error = error instanceof Error
          ? error.message
          : 'No se pudo asignar el turno.';
        await respuesta_haptica('error');
      }
    },
    eliminar: async function(id) {
      if (!window.confirm('¿Desea cancelar este turno?')) return;
      try {
        await this.store.eliminar_turno(id);
        await respuesta_haptica('confirmacion');
      } catch (error) {
        this.form_error = error instanceof Error ? error.message : 'No se pudo cancelar el turno.';
        await respuesta_haptica('error');
      }
    }
  }
};

function es_cancelacion_escaneo(error) {
  return error instanceof Error && /cancel|dismiss/i.test(error.message);
}

function fecha_local_hoy() {
  const hoy = new Date();
  return `${hoy.getFullYear()}-${String(hoy.getMonth() + 1).padStart(2, '0')}-${String(hoy.getDate()).padStart(2, '0')}`;
}

function blob_a_base64(blob) {
  return new Promise((resolve, reject) => {
    const lector = new FileReader();
    lector.onload = () => {
      const resultado = lector.result;
      if (typeof resultado !== 'string') {
        reject(new Error('No se pudo preparar el PDF para compartir.'));
        return;
      }
      resolve(resultado.split(',')[1]);
    };
    lector.onerror = () => reject(new Error('No se pudo leer el comprobante PDF.'));
    lector.readAsDataURL(blob);
  });
}
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
  flex-wrap: wrap;
  justify-content: flex-end;
  padding: 16px 16px 8px;
}

.turnos-intro {
  margin: 20px 16px 8px;
  padding: 22px 24px;
  border: 1px solid rgba(var(--ion-color-primary-rgb), 0.14);
  border-radius: 20px;
  background: linear-gradient(125deg, rgba(var(--ion-color-primary-rgb), 0.12), rgba(var(--ion-color-primary-rgb), 0.035));
}

.turnos-etiqueta {
  margin: 0 0 8px;
  color: var(--ion-color-primary);
  font-size: 0.72rem;
  font-weight: 750;
  letter-spacing: 0.12em;
}

.turnos-intro h2 {
  margin: 0 0 8px;
  font-size: clamp(1.3rem, 4vw, 1.8rem);
  font-weight: 750;
  letter-spacing: -0.035em;
}

.turnos-intro > p:last-child {
  max-width: 48rem;
  margin: 0;
  color: var(--ion-color-medium);
  line-height: 1.5;
}

.horarios-disponibles {
  display: flex;
  flex-wrap: wrap;
  gap: 8px;
  padding: 6px 0;
}

.lista-turnos {
  padding: 0 12px;
  background: transparent;
}

.turno-item {
  --border-radius: 14px;
  --background: var(--ion-card-background, var(--ion-background-color));
  margin: 8px 0;
  border: 1px solid var(--ion-color-step-150);
  border-radius: 14px;
  box-shadow: 0 6px 18px rgba(17, 49, 54, 0.04);
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

.franja-agenda {
  display: grid;
  grid-template-columns: minmax(110px, 1fr) repeat(2, minmax(95px, 1fr)) auto;
  align-items: center;
  gap: 8px;
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

.detalle-contenido {
  --background: var(--ion-color-light);
}

.qr-ayuda {
  color: var(--ion-color-medium);
  font-size: 0.9rem;
}

.qr-imagen {
  display: block;
  width: min(220px, 70vw);
  height: auto;
  margin: 16px auto;
  border-radius: 12px;
}

@media (max-width: 520px) {
  .filtros {
    grid-template-columns: 1fr;
  }

  .franja-agenda {
    grid-template-columns: 1fr 1fr;
  }

  .turnos-intro {
    margin: 14px 12px 8px;
    padding: 18px;
  }
}
</style>
