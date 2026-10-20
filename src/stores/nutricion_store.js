import { reactive } from 'vue';
import {
  crear_item_service,
  eliminar_item_service,
  modificar_item_service,
  obtener_nutricion_service
} from '@/services/nutricion_service';

export const nutricion_store = reactive({
  cargando: false,
  error: '',
  plan: [],
  paginaActual: 1,
  tamanoPagina: 5,
  total: 0,
  totalPaginas: 0,
  filtroBusqueda: '',
  filtroDia: 'Todos',

  async cargar_plan(opciones = {}) {
    const pagina = opciones.pagina ?? this.paginaActual ?? 1;
    const tamano = opciones.tamano ?? this.tamanoPagina ?? 5;
    const busqueda = opciones.busqueda ?? this.filtroBusqueda ?? '';
    const dia = opciones.dia ?? this.filtroDia ?? 'Todos';

    this.cargando = true;
    this.error = '';

    try {
      const respuesta = await obtener_nutricion_service({ pagina, tamano, busqueda, dia });
      this.plan = respuesta.plan || [];
      this.paginaActual = Number(respuesta.pagina) || pagina;
      this.tamanoPagina = Number(respuesta.tamano) || tamano;
      this.total = Number(respuesta.total) || 0;
      this.totalPaginas = Number(respuesta.totalPaginas) || 0;
      this.filtroBusqueda = busqueda;
      this.filtroDia = dia;
    } catch (error) {
      this.plan = [];
      this.error = error instanceof Error ? error.message : 'No se pudo cargar el plan nutricional.';
    } finally {
      this.cargando = false;
    }
  },

  async crear_item(item) {
    await crear_item_service(item);
    await this.cargar_plan({ pagina: 1, busqueda: this.filtroBusqueda, dia: this.filtroDia });
  },

  async modificar_item(id, item) {
    await modificar_item_service(id, item);
    await this.cargar_plan({ pagina: this.paginaActual, busqueda: this.filtroBusqueda, dia: this.filtroDia });
  },

  async eliminar_item(id) {
    await eliminar_item_service(id);
    await this.cargar_plan({ pagina: this.paginaActual, busqueda: this.filtroBusqueda, dia: this.filtroDia });
  }
});
