import { reactive } from 'vue';
import { obtener_nutricion_service } from '@/services/nutricion_service';

export const nutricion_store = reactive({
  cargando: false,
  error: '',
  plan: [],

  async cargar_plan() {
    this.cargando = true;
    this.error = '';

    try {
      this.plan = await obtener_nutricion_service();
    } catch (error) {
      this.plan = [];
      this.error = error instanceof Error ? error.message : 'No se pudo cargar el plan nutricional.';
    } finally {
      this.cargando = false;
    }
  }
});
