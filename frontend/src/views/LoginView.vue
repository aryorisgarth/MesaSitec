<script setup lang="ts">
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { ApiError } from '../api/client'
import { useAuthStore } from '../stores/auth'

const email = ref('')
const password = ref('')
const error = ref('')
const loading = ref(false)
const router = useRouter()
const auth = useAuthStore()

async function submit() {
  error.value = ''
  loading.value = true
  try {
    await auth.login(email.value, password.value)
    await router.push('/solicitudes')
  } catch (e) {
    error.value = e instanceof ApiError ? e.message : 'Error al iniciar sesión'
  } finally {
    loading.value = false
  }
}
</script>

<template>
  <div class="login-page">
    <form @submit.prevent="submit">
      <h1>MesaSitec</h1>
      <label>
        Email
        <input v-model="email" data-testid="login-email" type="email" required />
      </label>
      <label>
        Contraseña
        <input v-model="password" data-testid="login-password" type="password" required />
      </label>
      <p v-if="error" data-testid="login-error" class="error">{{ error }}</p>
      <button data-testid="login-submit" type="submit" :disabled="loading">
        {{ loading ? 'Ingresando...' : 'Ingresar' }}
      </button>
    </form>
  </div>
</template>

<style scoped>
.login-page {
  min-height: 100vh;
  display: grid;
  place-items: center;
  background: #f3f4f6;
}
form {
  background: white;
  padding: 2rem;
  border-radius: 8px;
  width: 360px;
  display: grid;
  gap: 1rem;
}
label {
  display: grid;
  gap: 0.25rem;
}
input, button {
  padding: 0.5rem;
}
.error {
  color: #b91c1c;
}
</style>
