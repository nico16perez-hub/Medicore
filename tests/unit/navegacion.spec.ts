import { describe, expect, test } from 'vitest';
import { obtener_tabs } from '@/config/navegacion';

describe('navegación por rol', () => {
  test('paciente conserva solo sus pantallas y las comunes', () => {
    expect(obtener_tabs('paciente').map(item => item.id)).toEqual([
      'inicio',
      'turnos',
      'notificaciones',
      'historia',
      'nutricion',
      'perfil'
    ]);
  });

  test('doctor ve las pantallas médicas y secretaria no ve módulos clínicos', () => {
    expect(obtener_tabs('doctor').map(item => item.id)).toEqual([
      'inicio',
      'turnos',
      'notificaciones',
      'historia',
      'nutricion',
      'perfil'
    ]);
    expect(obtener_tabs('secretaria').map(item => item.id)).toEqual([
      'inicio',
      'turnos',
      'notificaciones',
      'perfil'
    ]);
  });

  test('un usuario autenticado sin rol conserva Inicio, notificaciones y Mi cuenta', () => {
    expect(obtener_tabs(null).map(item => item.id)).toEqual(['inicio', 'notificaciones', 'perfil']);
  });
});
