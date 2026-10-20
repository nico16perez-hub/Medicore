import { mount } from '@vue/test-utils';
import { beforeEach, describe, expect, test } from 'vitest';
import InicioPage from '@/views/inicio_page.vue';
import { sesion_store } from '@/stores/sesion_store';

describe('inicio_page.vue', () => {
  beforeEach(() => {
    sesion_store.usuario = { rol: { codigo: 'paciente' } };
  });

  test('muestra accesos pensados para el paciente', () => {
    const wrapper = mount(InicioPage, {
      global: {
        stubs: {
          RouterLink: { template: '<a><slot /></a>' }
        }
      }
    });

    expect(wrapper.text()).toContain('Hola, estamos para acompañarte.');
    expect(wrapper.text()).toContain('Mis turnos');
    expect(wrapper.text()).toContain('Mi historia clínica');
  });
});
