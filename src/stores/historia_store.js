import { reactive } from 'vue';
import {
  crear_registro_service,
  eliminar_registro_service,
  modificar_registro_service,
  obtener_historia_service
} from '@/services/historia_service';

export const historia_store = reactive({
  cargando: false,
  error: '',
  registros: [],
  paginaActual: 1,
  tamanoPagina: 5,
  total: 0,
  totalPaginas: 0,
  filtroBusqueda: '',
  filtroTipo: 'Todos',
  pacienteUsuarioId: null,

  async cargar_registros(opciones = {}) {
    const pagina = opciones.pagina ?? this.paginaActual ?? 1;
    const tamano = opciones.tamano ?? this.tamanoPagina ?? 5;
    const busqueda = opciones.busqueda ?? this.filtroBusqueda ?? '';
    const tipo = opciones.tipo ?? this.filtroTipo ?? 'Todos';
    const pacienteUsuarioId = opciones.pacienteUsuarioId !== undefined
      ? opciones.pacienteUsuarioId
      : this.pacienteUsuarioId;

    this.cargando = true;
    this.error = '';

    try {
      const respuesta = await obtener_historia_service({
        pagina,
        tamano,
        busqueda,
        tipo,
        pacienteUsuarioId
      });
      this.registros = respuesta.registros || [];
      this.paginaActual = Number(respuesta.pagina) || pagina;
      this.tamanoPagina = Number(respuesta.tamano) || tamano;
      this.total = Number(respuesta.total) || 0;
      this.totalPaginas = Number(respuesta.totalPaginas) || 0;
      this.filtroBusqueda = busqueda;
      this.filtroTipo = tipo;
      this.pacienteUsuarioId = pacienteUsuarioId;
    } catch (error) {
      this.registros = [];
      this.error = error instanceof Error ? error.message : 'No se pudo cargar la historia clínica.';
    } finally {
      this.cargando = false;
    }
  },

  async crear_registro(registro) {
    await crear_registro_service(registro);
    await this.cargar_registros({
      pagina: 1,
      busqueda: this.filtroBusqueda,
      tipo: this.filtroTipo,
      pacienteUsuarioId: this.pacienteUsuarioId
    });
  },

  async modificar_registro(id, registro) {
    await modificar_registro_service(id, registro);
    await this.cargar_registros({
      pagina: this.paginaActual,
      busqueda: this.filtroBusqueda,
      tipo: this.filtroTipo,
      pacienteUsuarioId: this.pacienteUsuarioId
    });
  },

  async eliminar_registro(id) {
    await eliminar_registro_service(id);
    await this.cargar_registros({
      pagina: this.paginaActual,
      busqueda: this.filtroBusqueda,
      tipo: this.filtroTipo,
      pacienteUsuarioId: this.pacienteUsuarioId
    });
  }
});
