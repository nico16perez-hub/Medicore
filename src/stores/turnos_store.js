import { reactive } from 'vue';
import {
  crear_turno_service,
  eliminar_turno_service,
  modificar_turno_service,
  obtener_turnos_service
} from '@/services/turnos_service';

export const turnos_store = reactive({
  cargando: false,
  error: '',
  turnos: [],
  paginaActual: 1,
  tamanoPagina: 5,
  total: 0,
  totalPaginas: 0,
  filtroBusqueda: '',
  filtroEstado: 'Todos',

  async cargar_turnos(opciones = {}) {
    const pagina = opciones.pagina ?? this.paginaActual ?? 1;
    const tamano = opciones.tamano ?? this.tamanoPagina ?? 5;
    const busqueda = opciones.busqueda ?? this.filtroBusqueda ?? '';
    const estado = opciones.estado ?? this.filtroEstado ?? 'Todos';

    this.cargando = true;
    this.error = '';

    try {
      const respuesta = await obtener_turnos_service({ pagina, tamano, busqueda, estado });
      this.turnos = respuesta.turnos || [];
      this.paginaActual = Number(respuesta.pagina) || pagina;
      this.tamanoPagina = Number(respuesta.tamano) || tamano;
      this.total = Number(respuesta.total) || 0;
      this.totalPaginas = Number(respuesta.totalPaginas) || 0;
      this.filtroBusqueda = busqueda;
      this.filtroEstado = estado;
    } catch (error) {
      this.turnos = [];
      this.error = error instanceof Error ? error.message : 'No se pudieron cargar los turnos.';
    } finally {
      this.cargando = false;
    }
  },

  async crear_turno(turno) {
    const creado = await crear_turno_service(turno);
    await this.cargar_turnos({ pagina: 1, busqueda: this.filtroBusqueda, estado: this.filtroEstado });
    return creado;
  },

  async modificar_turno(id, turno) {
    await modificar_turno_service(id, turno);
    await this.cargar_turnos({ pagina: this.paginaActual, busqueda: this.filtroBusqueda, estado: this.filtroEstado });
  },

  async eliminar_turno(id) {
    await eliminar_turno_service(id);
    await this.cargar_turnos({ pagina: this.paginaActual, busqueda: this.filtroBusqueda, estado: this.filtroEstado });
  }
});
