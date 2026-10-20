import { reactive } from 'vue';
import {
  marcar_notificacion_leida_service,
  marcar_todas_leidas_service,
  obtener_no_leidas_service,
  obtener_notificaciones_service
} from '@/services/notificaciones_service';

export const notificaciones_store = reactive({
  cargando: false,
  error: '',
  notificaciones: [],
  paginaActual: 1,
  totalPaginas: 0,
  totalNoLeidas: 0,

  async cargar(pagina = 1) {
    this.cargando = true;
    this.error = '';
    try {
      const [bandeja, resumen] = await Promise.all([
        obtener_notificaciones_service(pagina),
        obtener_no_leidas_service()
      ]);
      this.notificaciones = bandeja.notificaciones || [];
      this.paginaActual = Number(bandeja.pagina) || pagina;
      this.totalPaginas = Number(bandeja.total_paginas) || 0;
      this.totalNoLeidas = Number(resumen.total) || 0;
    } catch (error) {
      this.error = error instanceof Error
        ? error.message
        : 'No se pudieron cargar las notificaciones.';
    } finally {
      this.cargando = false;
    }
  },

  async cargar_total_no_leidas() {
    try {
      const resumen = await obtener_no_leidas_service();
      this.totalNoLeidas = Number(resumen.total) || 0;
      return true;
    } catch (error) {
      this.error = error instanceof Error
        ? error.message
        : 'No se pudo consultar el total de notificaciones sin leer.';
      return false;
    }
  },

  async marcar_leida(id) {
    this.error = '';
    try {
      await marcar_notificacion_leida_service(id);
      const notificacion = this.notificaciones.find(item => item.id === id);
      if (notificacion && !notificacion.leida) {
        notificacion.leida = true;
        this.totalNoLeidas = Math.max(0, this.totalNoLeidas - 1);
      }
    } catch (error) {
      this.error = error instanceof Error
        ? error.message
        : 'No se pudo marcar la notificación como leída.';
    }
  },

  async marcar_todas_leidas() {
    this.error = '';
    try {
      await marcar_todas_leidas_service();
      this.notificaciones.forEach(notificacion => {
        notificacion.leida = true;
      });
      this.totalNoLeidas = 0;
    } catch (error) {
      this.error = error instanceof Error
        ? error.message
        : 'No se pudieron marcar las notificaciones como leídas.';
    }
  }
});
