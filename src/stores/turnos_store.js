import { reactive } from 'vue';
import { obtener_turnos_service } from '@/services/turnos_service';

export const turnos_store = reactive({
  cargando: false,
  error: '',
  turnos: [],

  async cargar_turnos() {
    this.cargando = true;
    this.error = '';

    try {
      this.turnos = await obtener_turnos_service();
    } catch (error) {
      this.turnos = [];
      this.error = error instanceof Error ? error.message : 'No se pudieron cargar los turnos.';
    } finally {
      this.cargando = false;
    }
  }
});
