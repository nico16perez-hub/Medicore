import { reactive } from 'vue';
import { obtener_historia_service } from '@/services/historia_service';

export const historia_store = reactive({
  cargando: false,
  error: '',
  registros: [],

  async cargar_registros() {
    this.cargando = true;
    this.error = '';

    try {
      this.registros = await obtener_historia_service();
    } catch (error) {
      this.registros = [];
      this.error = error instanceof Error ? error.message : 'No se pudo cargar la historia clínica.';
    } finally {
      this.cargando = false;
    }
  }
});
