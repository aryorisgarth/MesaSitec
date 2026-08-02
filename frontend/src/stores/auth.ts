import { defineStore } from 'pinia'
import { getMe, login as apiLogin } from '../api/solicitudes'
import type { UsuarioDto } from '../types/api'

export const useAuthStore = defineStore('auth', {
  state: () => ({
    usuario: JSON.parse(localStorage.getItem('usuario') ?? 'null') as UsuarioDto | null,
    token: localStorage.getItem('accessToken'),
  }),
  getters: {
    isAuthenticated: (state) => !!state.token,
  },
  actions: {
    async login(email: string, password: string) {
      const res = await apiLogin(email, password)
      this.token = res.accessToken
      this.usuario = res.usuario
      localStorage.setItem('accessToken', res.accessToken)
      localStorage.setItem('usuario', JSON.stringify(res.usuario))
    },
    async refreshMe() {
      this.usuario = await getMe()
      localStorage.setItem('usuario', JSON.stringify(this.usuario))
    },
    logout() {
      this.token = null
      this.usuario = null
      localStorage.removeItem('accessToken')
      localStorage.removeItem('usuario')
    },
  },
})

export const useUiStore = defineStore('ui', {
  state: () => ({
    toast: '' as string,
  }),
  actions: {
    showToast(message: string) {
      this.toast = message
      setTimeout(() => {
        this.toast = ''
      }, 3000)
    },
  },
})
