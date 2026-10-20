<template>
  <comp-page :titulo="'Inicio'">
    <main class="inicio">
      <section class="bienvenida">
        <div class="marca">M</div>
        <p class="sobrelinea">{{ etiqueta_rol }}</p>
        <h1>{{ saludo }}</h1>
        <p class="bienvenida-texto">{{ descripcion }}</p>
      </section>

      <section class="accesos">
        <h2>{{ titulo_accesos }}</h2>
        <div class="accesos-grid">
          <router-link
            v-for="acceso in accesos"
            :key="acceso.ruta"
            class="acceso"
            :to="acceso.ruta"
          >
            <span class="acceso-icono" aria-hidden="true">{{ acceso.icono }}</span>
            <span class="acceso-titulo">{{ acceso.titulo }}</span>
            <span class="acceso-descripcion">{{ acceso.descripcion }}</span>
            <span class="acceso-accion">{{ acceso.accion }} <span aria-hidden="true">→</span></span>
          </router-link>
        </div>
      </section>

      <aside class="recordatorio">
        <span aria-hidden="true">✦</span>
        <p>{{ recordatorio }}</p>
      </aside>
    </main>
  </comp-page>
</template>

<script>
import comp_page from '@/componets/estructura/comp_page.vue';
import { sesion_store } from '@/stores/sesion_store';

const accesos_por_rol = {
  paciente: [
    {
      ruta: '/app/turnos',
      icono: '＋',
      titulo: 'Mis turnos',
      descripcion: 'Solicitá una consulta y seguí la confirmación de secretaría.',
      accion: 'Ver mis turnos'
    },
    {
      ruta: '/app/historia',
      icono: '♡',
      titulo: 'Mi historia clínica',
      descripcion: 'Consultá tus registros de salud en un solo lugar.',
      accion: 'Ver mi historia'
    }
  ],
  doctor: [
    {
      ruta: '/app/historia',
      icono: '♡',
      titulo: 'Pacientes e historias',
      descripcion: 'Abrí la historia clínica completa de cada paciente asignado.',
      accion: 'Ver pacientes'
    },
    {
      ruta: '/app/turnos',
      icono: '▦',
      titulo: 'Mi agenda',
      descripcion: 'Consultá tus próximas consultas y pacientes del día.',
      accion: 'Ver mi agenda'
    }
  ],
  secretaria: [
    {
      ruta: '/app/turnos',
      icono: '▦',
      titulo: 'Solicitudes y agenda',
      descripcion: 'Revisá pedidos pendientes y asigná horarios disponibles.',
      accion: 'Organizar turnos'
    },
    {
      ruta: '/app/turnos',
      icono: '＋',
      titulo: 'Horarios de atención',
      descripcion: 'Mantené al día la disponibilidad semanal de cada doctor.',
      accion: 'Configurar agenda'
    }
  ]
};

export default {
  name: 'inicio_page',
  components: { CompPage: comp_page },
  computed: {
    usuario() {
      return sesion_store.usuario;
    },
    rol() {
      return this.usuario?.rol?.codigo || 'paciente';
    },
    etiqueta_rol() {
      return {
        paciente: 'TU ESPACIO DE SALUD',
        doctor: 'PANEL PROFESIONAL',
        secretaria: 'GESTIÓN SANATORIAL'
      }[this.rol] || 'MEDICORE';
    },
    saludo() {
      if (this.rol === 'paciente') return 'Hola, estamos para acompañarte.';
      if (this.rol === 'doctor') return 'Tu atención, organizada.';
      if (this.rol === 'secretaria') return 'La clínica empieza por una buena agenda.';
      return 'Bienvenido a MediCore.';
    },
    descripcion() {
      if (this.rol === 'paciente') {
        return 'Gestioná tus consultas y encontrá tu información clínica cuando la necesites.';
      }
      if (this.rol === 'doctor') {
        return 'Accedé a tu agenda y a la historia clínica de cada paciente que atendés.';
      }
      if (this.rol === 'secretaria') {
        return 'Coordiná solicitudes, disponibilidad médica y confirmaciones de turnos.';
      }
      return 'Tu información y novedades están reunidas en un mismo espacio.';
    },
    titulo_accesos() {
      return this.rol === 'paciente' ? '¿Qué necesitás hacer?' : 'Accesos de trabajo';
    },
    accesos() {
      return accesos_por_rol[this.rol] || [
        {
          ruta: '/app/perfil',
          icono: '◉',
          titulo: 'Mi cuenta',
          descripcion: 'Consultá los datos de tu sesión.',
          accion: 'Abrir mi cuenta'
        }
      ];
    },
    recordatorio() {
      if (this.rol === 'paciente') {
        return 'Tu solicitud de turno queda pendiente hasta que secretaría confirme un horario.';
      }
      if (this.rol === 'doctor') {
        return 'La historia clínica se consulta por paciente y solo para personas asignadas a tu atención.';
      }
      return 'Los turnos asignados notifican al paciente y al doctor; las nuevas solicitudes llegan a secretaría.';
    }
  }
};
</script>

<style scoped>
.inicio {
  width: min(100% - 32px, 1080px);
  margin: 28px auto 40px;
}

.bienvenida {
  position: relative;
  overflow: hidden;
  padding: clamp(24px, 6vw, 48px);
  border-radius: 28px;
  background: linear-gradient(120deg, #103c42, #137c74 72%, #2a9985);
  color: #ffffff;
}

.bienvenida::after {
  position: absolute;
  top: -110px;
  right: -60px;
  width: 300px;
  height: 300px;
  border: 1px solid rgba(255, 255, 255, 0.14);
  border-radius: 50%;
  box-shadow: 0 0 0 34px rgba(255, 255, 255, 0.04), 0 0 0 70px rgba(255, 255, 255, 0.035);
  content: '';
  pointer-events: none;
}

.marca {
  display: grid;
  width: 42px;
  height: 42px;
  place-items: center;
  border: 1px solid rgba(255, 255, 255, 0.32);
  border-radius: 14px;
  background: rgba(255, 255, 255, 0.13);
  font-weight: 800;
}

.sobrelinea {
  margin: 30px 0 10px;
  color: rgba(255, 255, 255, 0.76);
  font-size: 0.72rem;
  font-weight: 750;
  letter-spacing: 0.14em;
}

.bienvenida h1 {
  max-width: 680px;
  margin: 0;
  font-size: clamp(2rem, 5vw, 3.6rem);
  font-weight: 750;
  letter-spacing: -0.055em;
  line-height: 1.08;
}

.bienvenida-texto {
  max-width: 620px;
  margin: 16px 0 0;
  color: rgba(255, 255, 255, 0.84);
  font-size: clamp(0.98rem, 2vw, 1.1rem);
  line-height: 1.6;
}

.accesos {
  margin-top: 34px;
}

.accesos h2 {
  margin: 0 0 16px;
  font-size: 1.22rem;
  font-weight: 720;
  letter-spacing: -0.025em;
}

.accesos-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(min(100%, 280px), 1fr));
  gap: 16px;
}

.acceso {
  display: grid;
  min-height: 220px;
  grid-template-rows: auto auto 1fr auto;
  gap: 12px;
  padding: 22px;
  border: 1px solid var(--ion-color-step-150);
  border-radius: 20px;
  background: var(--ion-card-background, var(--ion-background-color));
  color: var(--ion-text-color);
  text-decoration: none;
  box-shadow: 0 8px 28px rgba(17, 49, 54, 0.055);
  transition: border-color 160ms ease, transform 160ms ease, box-shadow 160ms ease;
}

.acceso:hover {
  transform: translateY(-2px);
  border-color: rgba(var(--ion-color-primary-rgb), 0.5);
  box-shadow: 0 14px 32px rgba(17, 49, 54, 0.1);
}

.acceso-icono {
  display: grid;
  width: 42px;
  height: 42px;
  place-items: center;
  border-radius: 14px;
  background: rgba(var(--ion-color-primary-rgb), 0.11);
  color: var(--ion-color-primary);
  font-size: 1.5rem;
  font-weight: 700;
}

.acceso-titulo {
  font-size: 1.08rem;
  font-weight: 730;
}

.acceso-descripcion {
  color: var(--ion-color-medium);
  line-height: 1.5;
}

.acceso-accion {
  display: flex;
  justify-content: space-between;
  color: var(--ion-color-primary);
  font-size: 0.9rem;
  font-weight: 720;
}

.recordatorio {
  display: flex;
  align-items: flex-start;
  gap: 12px;
  margin-top: 24px;
  padding: 16px 18px;
  border: 1px solid rgba(var(--ion-color-primary-rgb), 0.15);
  border-radius: 16px;
  background: rgba(var(--ion-color-primary-rgb), 0.055);
  color: var(--ion-color-medium);
}

.recordatorio > span {
  color: var(--ion-color-primary);
  font-size: 1.1rem;
}

.recordatorio p {
  margin: 0;
  line-height: 1.5;
}

@media (max-width: 520px) {
  .inicio {
    width: min(100% - 24px, 1080px);
    margin-top: 16px;
  }

  .bienvenida {
    border-radius: 22px;
  }
}
</style>
