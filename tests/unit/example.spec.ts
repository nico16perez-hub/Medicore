import { mount } from '@vue/test-utils';
import { describe, expect, test } from 'vitest';
import InicioPage from '@/views/inicio_page.vue';

describe('inicio_page.vue', () => {
  test('renderiza el contenido principal de la pantalla de inicio', () => {
    const wrapper = mount(InicioPage);
    expect(wrapper.text()).toContain('Sistema Integral Sanatorial');
    expect(wrapper.text()).toContain('MediCore');
  });
});
