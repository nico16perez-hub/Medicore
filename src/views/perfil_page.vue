<template>
  <comp-page :titulo="'Mi cuenta'">
    <section class="perfil-foto">
      <ion-avatar>
        <img v-if="foto_url" :src="foto_url" alt="Foto del perfil">
        <ion-icon v-else :icon="person_circle_outline" aria-hidden="true" />
      </ion-avatar>
      <div v-if="foto_pendiente" class="foto-previa">
        <img :src="foto_pendiente" alt="Vista previa de la foto seleccionada">
        <p>Vista previa</p>
        <ion-button size="small" @click="guardar_foto">Guardar foto</ion-button>
        <ion-button size="small" fill="clear" @click="cancelar_foto">Cancelar</ion-button>
      </div>
      <div v-else>
        <ion-button fill="outline" @click="abrir_opciones_foto">
          {{ foto_url ? 'Reemplazar foto' : 'Agregar foto' }}
        </ion-button>
        <input
          ref="selector_foto"
          class="selector-oculto"
          type="file"
          accept="image/jpeg,image/png,image/webp"
          @change="seleccionar_archivo"
        >
      </div>
    </section>
    <ion-list>
      <ion-item>
        <ion-label>
          <h2>{{ usuario?.email || 'Usuario' }}</h2>
          <p>Rol: {{ usuario?.rol?.nombre || 'Sin rol asignado' }}</p>
          <p>Estado de sesión: {{ autenticado ? 'Autenticada' : 'No autenticada' }}</p>
        </ion-label>
      </ion-item>
      <ion-item v-if="!usuario?.rol">
        <ion-label class="aviso-sin-rol">
          Tu usuario está autenticado, pero todavía no tiene un rol asignado.
        </ion-label>
      </ion-item>
      <ion-item v-if="usuario?.rol?.codigo === 'paciente'">
        <ion-label>
          <h2>Ubicación de referencia</h2>
          <p>Es una ubicación opcional guardada por vos; no se considera tu domicilio.</p>
          <p v-if="ubicacion">
            Registrada {{ ubicacion.actualizada_en_utc ? new Date(ubicacion.actualizada_en_utc).toLocaleString() : '' }}
          </p>
          <p v-if="ubicacion_mensaje" :class="{ 'mensaje-error': ubicacion_error }">
            {{ ubicacion_mensaje }}
          </p>
          <div class="acciones-perfil">
            <ion-button size="small" fill="outline" :disabled="ubicacion_cargando" @click="registrar_ubicacion">
              Registrar mi ubicación
            </ion-button>
            <ion-button
              v-if="ubicacion"
              size="small"
              fill="outline"
              @click="abrir_ubicacion"
            >
              Ver ubicación
            </ion-button>
            <ion-button
              v-if="sanatorio_direccion"
              size="small"
              fill="outline"
              @click="como_llegar"
            >
              Cómo llegar
            </ion-button>
            <ion-button
              v-if="ubicacion"
              size="small"
              fill="clear"
              color="medium"
              @click="eliminar_ubicacion"
            >
              Eliminar
            </ion-button>
          </div>
        </ion-label>
      </ion-item>
      <ion-item v-if="push_store.token">
        <ion-label>
          <h2>Token FCM de este dispositivo</h2>
          <p class="push-token">{{ push_store.token }}</p>
          <p>Usalo solo para probar desde Firebase Console y no lo compartas.</p>
        </ion-label>
      </ion-item>
      <ion-item>
        <ion-label>
          <h2>Notificaciones push</h2>
          <p>{{ push_store.mensaje || 'Registro opcional para recibir avisos en Android.' }}</p>
        </ion-label>
        <ion-button slot="end" fill="outline" @click="activar_push">
          Activar
        </ion-button>
      </ion-item>
      <ion-item>
        <ion-label>Modo oscuro</ion-label>
        <ion-toggle slot="end" :checked="tema_oscuro" @ion-change="cambiar_tema" />
      </ion-item>
      <ion-item>
        <ion-label>
          <h2>Vibración</h2>
          <p>Respuesta háptica al confirmar, escanear o encontrar un error.</p>
        </ion-label>
        <ion-toggle slot="end" :checked="vibracion" @ion-change="cambiar_vibracion" />
      </ion-item>
      <ion-item v-if="sesion_store.nativeSecureStorage && sesion_store.biometricEnabled">
        <ion-label>
          <h2>Desbloqueo biométrico habilitado</h2>
          <p>La biometría desbloquea esta sesión guardada; no reemplaza tu contraseña.</p>
        </ion-label>
      </ion-item>
      <ion-item v-if="sesion_store.nativeSecureStorage && !sesion_store.biometricEnabled">
        <ion-button expand="block" fill="outline" @click="activar_biometria">
          Habilitar desbloqueo biométrico
        </ion-button>
      </ion-item>
      <ion-item v-else-if="sesion_store.nativeSecureStorage">
        <ion-button expand="block" fill="outline" @click="desactivar_biometria">
          Deshabilitar biometría
        </ion-button>
      </ion-item>
      <ion-item v-if="error">
        <ion-label color="danger">{{ error }}</ion-label>
      </ion-item>
      <ion-item>
        <ion-button expand="block" color="danger" @click="cerrar_sesion">
          Cerrar sesión
        </ion-button>
      </ion-item>
    </ion-list>
  </comp-page>
</template>

<script>
import { actionSheetController } from '@ionic/vue';
import { IonAvatar, IonButton, IonIcon, IonItem, IonLabel, IonList, IonToggle } from '@ionic/vue';
import { Camera, CameraResultType, CameraSource } from '@capacitor/camera';
import { Capacitor } from '@capacitor/core';
import { personCircleOutline } from 'ionicons/icons';
import comp_page from '@/componets/estructura/comp_page.vue';
import { alternar_tema, es_tema_oscuro } from '@/config/tema';
import { guardar_foto_perfil_service, obtener_foto_perfil_service } from '@/services/perfil_service';
import {
  guardar_preferencia_vibracion,
  respuesta_haptica,
  vibracion_habilitada
} from '@/services/preferencias_service';
import {
  abrir_mapa_service,
  eliminar_ubicacion_perfil_service,
  guardar_ubicacion_perfil_service,
  obtener_ubicacion_actual_service,
  obtener_ubicacion_perfil_service
} from '@/services/ubicacion_service';
import { registrar_dispositivo_push_service } from '@/services/notificaciones_push_service';
import { notificaciones_push_store } from '@/stores/notificaciones_push_store';
import {
  deshabilitar_biometria,
  habilitar_biometria,
  logout,
  sesion_store
} from '@/stores/sesion_store';

export default {
  name: 'perfil_page',
  components: {
    CompPage: comp_page,
    IonAvatar,
    IonButton,
    IonIcon,
    IonItem,
    IonLabel,
    IonList,
    IonToggle
  },
  data() {
    return {
      tema_oscuro: es_tema_oscuro(),
      sesion_store,
      error: '',
      foto_url: '',
      foto_carga_id: 0,
      foto_pendiente: '',
      foto_blob_pendiente: null,
      vibracion: vibracion_habilitada(),
      person_circle_outline: personCircleOutline,
      ubicacion: null,
      ubicacion_mensaje: '',
      ubicacion_error: false,
      ubicacion_cargando: false,
      push_store: notificaciones_push_store
    };
  },
  computed: {
    sanatorio_direccion() {
      return import.meta.env.VITE_SANATORIO_DIRECCION || '';
    },
    usuario() {
      return this.sesion_store.usuario;
    },
    autenticado() {
      return this.sesion_store.autenticado;
    }
  },
  watch: {
    'usuario.id': function(usuario_id) {
      this.cargar_foto(usuario_id);
    }
  },
  mounted() {
    this.cargar_foto();
    if (this.usuario?.rol?.codigo === 'paciente') this.cargar_ubicacion();
  },
  beforeUnmount() {
    if (this.foto_url.startsWith('blob:')) URL.revokeObjectURL(this.foto_url);
  },
  methods: {
    cargar_ubicacion: async function() {
      try {
        this.ubicacion = await obtener_ubicacion_perfil_service();
      } catch (error) {
        this.ubicacion_error = true;
        this.ubicacion_mensaje = error instanceof Error
          ? error.message
          : 'No se pudo consultar la ubicación guardada.';
      }
    },
    registrar_ubicacion: async function() {
      this.ubicacion_cargando = true;
      this.ubicacion_mensaje = '';
      this.ubicacion_error = false;
      const resultado = await obtener_ubicacion_actual_service();
      if (!resultado.ok) {
        this.ubicacion_mensaje = resultado.mensaje;
        this.ubicacion_error = true;
        this.ubicacion_cargando = false;
        return;
      }
      try {
        await guardar_ubicacion_perfil_service(resultado.ubicacion);
        this.ubicacion = {
          ...resultado.ubicacion,
          actualizada_en_utc: new Date().toISOString()
        };
        this.ubicacion_mensaje = 'Ubicación de referencia guardada.';
      } catch (error) {
        this.ubicacion_error = true;
        this.ubicacion_mensaje = error instanceof Error
          ? error.message
          : 'No se pudo guardar la ubicación.';
      } finally {
        this.ubicacion_cargando = false;
      }
    },
    abrir_ubicacion: async function() {
      const resultado = await abrir_mapa_service(this.ubicacion);
      this.ubicacion_error = !resultado.ok;
      this.ubicacion_mensaje = resultado.mensaje;
    },
    como_llegar: async function() {
      const resultado = await abrir_mapa_service(
        { direccion: this.sanatorio_direccion },
        'llegar'
      );
      this.ubicacion_error = !resultado.ok;
      this.ubicacion_mensaje = resultado.mensaje;
    },
    eliminar_ubicacion: async function() {
      this.ubicacion_error = false;
      this.ubicacion_mensaje = '';
      try {
        await eliminar_ubicacion_perfil_service();
        this.ubicacion = null;
        this.ubicacion_mensaje = 'Se eliminó la ubicación de referencia.';
      } catch (error) {
        this.ubicacion_error = true;
        this.ubicacion_mensaje = error instanceof Error
          ? error.message
          : 'No se pudo eliminar la ubicación.';
      }
    },
    activar_push: async function() {
      const resultado = await registrar_dispositivo_push_service();
      this.push_store.mensaje = resultado.mensaje;
    },
    cargar_foto: async function(usuario_id = this.usuario?.id) {
      const carga_id = ++this.foto_carga_id;
      if (this.foto_url.startsWith('blob:')) URL.revokeObjectURL(this.foto_url);
      this.foto_url = '';
      if (!usuario_id) return;
      try {
        const blob = await obtener_foto_perfil_service();
        if (carga_id !== this.foto_carga_id || this.usuario?.id !== usuario_id) return;
        if (blob) this.foto_url = URL.createObjectURL(blob);
      } catch (error) {
        if (carga_id === this.foto_carga_id && this.usuario?.id === usuario_id) {
          this.error = error instanceof Error ? error.message : 'No se pudo cargar la foto del perfil.';
        }
      }
    },
    abrir_opciones_foto: async function() {
      this.error = '';
      if (!Capacitor.isNativePlatform()) {
        this.$refs.selector_foto?.click();
        return;
      }

      const opciones = await actionSheetController.create({
        header: 'Foto del perfil',
        buttons: [
          { text: 'Cámara', handler: () => this.capturar_foto(CameraSource.Camera) },
          { text: 'Galería', handler: () => this.capturar_foto(CameraSource.Photos) },
          { text: 'Cancelar', role: 'cancel' }
        ]
      });
      await opciones.present();
    },
    capturar_foto: async function(origen) {
      this.error = '';
      try {
        const permiso = origen === CameraSource.Camera ? 'camera' : 'photos';
        const estado = await Camera.checkPermissions();
        if (estado[permiso] !== 'granted' && estado[permiso] !== 'limited') {
          const solicitado = await Camera.requestPermissions({ permissions: [permiso] });
          if (solicitado[permiso] !== 'granted' && solicitado[permiso] !== 'limited') {
            throw new Error('Se denegó el permiso para acceder a la cámara o las fotos.');
          }
        }
        const foto = await Camera.getPhoto({
          quality: 90,
          allowEditing: true,
          resultType: CameraResultType.DataUrl,
          source: origen
        });
        if (!foto.dataUrl) throw new Error('No se recibió una imagen válida.');
        const blob = data_url_a_blob(foto.dataUrl);
        this.preparar_foto(foto.dataUrl, blob);
      } catch (error) {
        if (!es_cancelacion(error)) {
          this.error = error instanceof Error ? error.message : 'No se pudo obtener la foto.';
        }
      }
    },
    seleccionar_archivo: function(evento) {
      const archivo = evento.target.files?.[0];
      evento.target.value = '';
      if (!archivo) return;
      if (!['image/jpeg', 'image/png', 'image/webp'].includes(archivo.type)) {
        this.error = 'El formato debe ser JPEG, PNG o WebP.';
        return;
      }
      if (archivo.size > 5 * 1024 * 1024) {
        this.error = 'La imagen no puede superar los 5 MB.';
        return;
      }
      const lector = new FileReader();
      lector.onload = () => this.preparar_foto(lector.result, archivo);
      lector.onerror = () => {
        this.error = 'No se pudo leer el archivo seleccionado.';
      };
      lector.readAsDataURL(archivo);
    },
    preparar_foto: function(vista_previa, blob) {
      const tipo = blob.type || data_url_mime(vista_previa);
      if (!['image/jpeg', 'image/png', 'image/webp'].includes(tipo) ||
          blob.size > 5 * 1024 * 1024) {
        this.error = 'Elegí una imagen JPEG, PNG o WebP de hasta 5 MB.';
        return;
      }
      this.foto_pendiente = vista_previa;
      this.foto_blob_pendiente = blob;
      this.error = '';
    },
    guardar_foto: async function() {
      if (!this.foto_blob_pendiente) return;
      this.error = '';
      try {
        const usuario_id = this.usuario?.id;
        await guardar_foto_perfil_service(this.foto_blob_pendiente);
        if (this.usuario?.id !== usuario_id) return;
        if (this.foto_url.startsWith('blob:')) URL.revokeObjectURL(this.foto_url);
        this.foto_url = URL.createObjectURL(this.foto_blob_pendiente);
        this.cancelar_foto();
        await respuesta_haptica('confirmacion');
      } catch (error) {
        this.error = error instanceof Error ? error.message : 'No se pudo guardar la foto del perfil.';
        await respuesta_haptica('error');
      }
    },
    cancelar_foto: function() {
      this.foto_pendiente = '';
      this.foto_blob_pendiente = null;
    },
    cambiar_vibracion: function(evento) {
      this.vibracion = Boolean(evento.detail.checked);
      guardar_preferencia_vibracion(this.vibracion);
      if (this.vibracion) respuesta_haptica('confirmacion');
    },
    cambiar_tema: function(evento) {
      if (evento.detail.checked === this.tema_oscuro) return;
      alternar_tema();
      this.tema_oscuro = es_tema_oscuro();
    },
    activar_biometria: async function() {
      this.error = '';
      try {
        await habilitar_biometria();
      } catch (error) {
        this.error = error instanceof Error ? error.message : 'No se pudo habilitar la biometría.';
      }
    },
    desactivar_biometria: async function() {
      this.error = '';
      try {
        await deshabilitar_biometria();
      } catch (error) {
        this.error = error instanceof Error ? error.message : 'No se pudo deshabilitar la biometría.';
      }
    },
    cerrar_sesion: async function() {
      this.error = '';
      try {
        this.push_store.token = '';
        await logout();
      } catch (error) {
        this.error = error instanceof Error ? error.message : 'No se pudo revocar la sesión en el servidor.';
        this.sesion_store.error = this.error;
      } finally {
        await this.$router.replace('/login');
      }
    }
  }
};

function data_url_mime(data_url) {
  return typeof data_url === 'string' ? data_url.match(/^data:([^;]+);base64,/)?.[1] || '' : '';
}

function data_url_a_blob(data_url) {
  const [cabecera, contenido] = data_url.split(',');
  const tipo = cabecera.match(/^data:([^;]+);base64$/)?.[1] || '';
  const binario = atob(contenido);
  const bytes = new Uint8Array(binario.length);
  for (let indice = 0; indice < binario.length; indice += 1) {
    bytes[indice] = binario.charCodeAt(indice);
  }
  return new Blob([bytes], { type: tipo });
}

function es_cancelacion(error) {
  return error instanceof Error && /cancel|dismiss/i.test(error.message);
}
</script>

<style scoped>
.perfil-foto {
  display: flex;
  align-items: center;
  gap: 18px;
  padding: 20px 16px;
}

.perfil-foto ion-avatar {
  width: 88px;
  height: 88px;
  background: var(--ion-color-primary-tint);
}

.perfil-foto ion-avatar ion-icon {
  width: 100%;
  height: 100%;
  color: var(--ion-color-primary-contrast);
}

.foto-previa img {
  display: block;
  width: 96px;
  height: 96px;
  border-radius: 16px;
  object-fit: cover;
}

.foto-previa p {
  margin: 6px 0;
  color: var(--ion-color-medium);
}

.selector-oculto {
  display: none;
}

.acciones-perfil {
  display: flex;
  flex-wrap: wrap;
  gap: 6px;
  margin-top: 8px;
}

.mensaje-error {
  color: var(--ion-color-danger);
}

.push-token {
  user-select: text;
  overflow-wrap: anywhere;
}
</style>
