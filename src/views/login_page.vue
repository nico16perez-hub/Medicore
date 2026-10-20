<template>
  <ion-page>
    <ion-content class="login-contenido">
      <section class="login-panel">
        <div class="marca-icono">M</div>
        <h1>MediCore</h1>
        <p>Ingresá para continuar</p>

        <form class="formulario-login" @submit.prevent="ingresar">
          <ion-input
            v-model="email"
            label="Email"
            label-placement="stacked"
            type="email"
            autocomplete="username"
            required
          />
          <ion-input
            v-model="password"
            label="Contraseña"
            label-placement="stacked"
            type="password"
            autocomplete="current-password"
            required
          />
          <p v-if="error" class="mensaje-error" role="alert">{{ error }}</p>
          <ion-button expand="block" type="submit" :disabled="cargando">
            {{ cargando ? 'Ingresando…' : 'Iniciar sesión' }}
          </ion-button>
        </form>

        <ion-button
          v-if="sesion_store.requiereBiometria"
          expand="block"
          fill="outline"
          :disabled="cargando"
          @click="desbloquear"
        >
          Desbloquear con biometría
        </ion-button>
        <p v-if="sesion_store.requiereBiometria" class="ayuda">
          También podés ingresar con tu email y contraseña.
        </p>
      </section>
    </ion-content>
  </ion-page>
</template>

<script>
import { IonButton, IonContent, IonInput, IonPage } from '@ionic/vue';
import { login, desbloquear_con_biometria, sesion_store } from '@/stores/sesion_store';

export default {
  name: 'login_page',
  components: { IonButton, IonContent, IonInput, IonPage },
  data() {
    return {
      sesion_store,
      email: '',
      password: '',
      cargando: false,
      error: ''
    };
  },
  mounted() {
    this.error = sesion_store.error;
  },
  methods: {
    ingresar: async function() {
      this.cargando = true;
      this.error = '';
      try {
        await login(this.email.trim(), this.password);
        await this.$router.replace('/app/inicio');
      } catch (error) {
        this.error = error instanceof Error ? error.message : 'No se pudo iniciar sesión.';
      } finally {
        this.cargando = false;
      }
    },
    desbloquear: async function() {
      this.cargando = true;
      this.error = '';
      try {
        if (await desbloquear_con_biometria()) {
          await this.$router.replace('/app/inicio');
        }
      } catch (error) {
        this.error = error instanceof Error ? error.message : 'No se pudo desbloquear la sesión.';
      } finally {
        this.cargando = false;
      }
    }
  }
};
</script>

<style scoped>
.login-contenido {
  --background: var(--ion-background-color);
}

.login-panel {
  display: grid;
  gap: 14px;
  width: min(100% - 32px, 420px);
  margin: min(14vh, 120px) auto 32px;
}

.login-panel h1,
.login-panel p {
  margin: 0;
  text-align: center;
}

.login-panel h1 {
  font-size: 1.8rem;
}

.login-panel > p,
.ayuda {
  color: var(--ion-color-medium);
}

.marca-icono {
  display: grid;
  place-items: center;
  width: 52px;
  height: 52px;
  margin: 0 auto;
  border-radius: 12px;
  color: #fff;
  background: #1f8a70;
  font-size: 1.35rem;
  font-weight: 700;
}

.formulario-login {
  display: grid;
  gap: 16px;
  padding: 18px 0;
}

.mensaje-error {
  color: var(--ion-color-danger);
  text-align: left !important;
}

.ayuda {
  font-size: 0.9rem;
}
</style>
