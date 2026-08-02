<script setup lang="ts">
import { RouterView, useRouter } from 'vue-router'
import { useAuthStore, useUiStore } from './stores/auth'

const auth = useAuthStore()
const ui = useUiStore()
const router = useRouter()

function logout() {
  auth.logout()
  void router.push('/login')
}
</script>

<template>
  <div>
    <nav v-if="auth.isAuthenticated" data-testid="app-nav" class="nav">
      <strong>MesaSitec</strong>
      <span data-testid="nav-usuario-nombre">{{ auth.usuario?.nombre }}</span>
      <span data-testid="nav-usuario-rol">{{ auth.usuario?.rol }}</span>
      <button data-testid="btn-logout" @click="logout">Salir</button>
    </nav>
    <main class="main">
      <RouterView />
    </main>
    <p v-if="ui.toast" data-testid="toast-mensaje" class="toast">{{ ui.toast }}</p>
  </div>
</template>

<style>
* {
  box-sizing: border-box;
}
body {
  margin: 0;
  font-family: system-ui, sans-serif;
  color: #111827;
}
.nav {
  display: flex;
  gap: 1rem;
  align-items: center;
  padding: 1rem;
  background: #1f2937;
  color: white;
}
.nav button {
  margin-left: auto;
}
.main {
  padding: 1rem;
}
.toast {
  position: fixed;
  bottom: 1rem;
  right: 1rem;
  background: #111827;
  color: white;
  padding: 0.75rem 1rem;
  border-radius: 6px;
}
</style>
