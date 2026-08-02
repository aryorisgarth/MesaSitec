<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import { useRouter } from 'vue-router'
import { ApiError } from '../api/client'
import { getSolicitud, transicionar } from '../api/solicitudes'
import { useAuthStore, useUiStore } from '../stores/auth'
import type { AccionTransicion, SolicitudDetalleDto } from '../types/api'

const props = defineProps<{ id: string }>()
const router = useRouter()
const auth = useAuthStore()
const ui = useUiStore()

const solicitud = ref<SolicitudDetalleDto | null>(null)
const loading = ref(true)
const error = ref('')
const modalAbierto = ref(false)
const accionActual = ref<AccionTransicion | null>(null)
const motivo = ref('')
const agenteId = ref('')
const modalError = ref('')

const transiciones: Record<string, Partial<Record<AccionTransicion, string>>> = {
  Nueva: { asignar: 'Asignada', cancelar: 'Cancelada' },
  Asignada: { iniciar: 'EnProceso', asignar: 'Asignada', cancelar: 'Cancelada' },
  EnProceso: { resolver: 'Resuelta', asignar: 'Asignada', cancelar: 'Cancelada' },
  Resuelta: { cerrar: 'Cerrada', reabrir: 'EnProceso' },
}

function puedeAccion(accion: AccionTransicion): boolean {
  if (!solicitud.value || !auth.usuario) return false
  const estado = solicitud.value.estado
  const rol = auth.usuario.rol
  const propia = solicitud.value.solicitante.id === auth.usuario.id

  if (!transiciones[estado]?.[accion]) return false

  if (accion === 'cancelar') return rol === 'Admin'
  if (['asignar', 'iniciar', 'resolver', 'reabrir'].includes(accion)) {
    return rol === 'Admin' || rol === 'Agente'
  }
  if (accion === 'cerrar') {
    return rol === 'Admin' || rol === 'Agente' || (rol === 'Solicitante' && propia)
  }
  return false
}

const puedeEditar = computed(() => {
  if (!solicitud.value || !auth.usuario) return false
  const rol = auth.usuario.rol
  const propia = solicitud.value.solicitante.id === auth.usuario.id
  if (rol === 'Admin' || rol === 'Agente') return true
  return rol === 'Solicitante' && propia && solicitud.value.estado === 'Nueva'
})

async function cargar() {
  loading.value = true
  error.value = ''
  try {
    solicitud.value = await getSolicitud(props.id)
  } catch (e) {
    error.value = e instanceof Error ? e.message : 'Error al cargar'
  } finally {
    loading.value = false
  }
}

function abrirModal(accion: AccionTransicion) {
  accionActual.value = accion
  motivo.value = ''
  agenteId.value = ''
  modalError.value = ''
  modalAbierto.value = true
}

async function confirmarAccion() {
  if (!accionActual.value) return
  modalError.value = ''
  try {
    await transicionar(props.id, {
      accion: accionActual.value,
      motivo: motivo.value || undefined,
      agenteId: accionActual.value === 'asignar' ? agenteId.value : undefined,
    })
    modalAbierto.value = false
    ui.showToast('Acción aplicada correctamente')
    await cargar()
  } catch (e) {
    modalError.value = e instanceof ApiError ? e.message : 'Error en la acción'
  }
}

onMounted(cargar)
</script>

<template>
  <div>
    <p v-if="loading">Cargando detalle...</p>
    <p v-else-if="error" class="error">{{ error }}</p>
    <div v-else-if="solicitud">
      <h2 data-testid="detalle-titulo">{{ solicitud.titulo }}</h2>
      <p data-testid="detalle-codigo">{{ solicitud.codigo }}</p>
      <p data-testid="detalle-descripcion">{{ solicitud.descripcion }}</p>
      <p data-testid="detalle-estado">Estado: {{ solicitud.estado }}</p>
      <p data-testid="detalle-prioridad">Prioridad: {{ solicitud.prioridad }}</p>
      <p data-testid="detalle-categoria">Categoría: {{ solicitud.categoria.nombre }}</p>
      <p data-testid="detalle-agente">Agente: {{ solicitud.agente?.nombre ?? 'Sin asignar' }}</p>
      <p data-testid="detalle-fecha-creacion">Creación: {{ solicitud.fechaCreacion }}</p>
      <p data-testid="detalle-fecha-limite">SLA: {{ solicitud.fechaLimiteSla }}</p>
      <p v-if="solicitud.vencida" data-testid="detalle-vencida">Vencida</p>
      <p v-if="solicitud.motivoResolucion || solicitud.motivoCancelacion" data-testid="detalle-motivo">
        {{ solicitud.motivoResolucion ?? solicitud.motivoCancelacion }}
      </p>

      <div class="actions">
        <button v-if="puedeEditar" data-testid="btn-editar" @click="router.push(`/solicitudes/${id}/editar`)">
          Editar
        </button>
        <button v-if="puedeAccion('asignar')" data-testid="btn-accion-asignar" @click="abrirModal('asignar')">
          Asignar
        </button>
        <button v-if="puedeAccion('iniciar')" data-testid="btn-accion-iniciar" @click="abrirModal('iniciar')">
          Iniciar
        </button>
        <button v-if="puedeAccion('resolver')" data-testid="btn-accion-resolver" @click="abrirModal('resolver')">
          Resolver
        </button>
        <button v-if="puedeAccion('cerrar')" data-testid="btn-accion-cerrar" @click="abrirModal('cerrar')">
          Cerrar
        </button>
        <button v-if="puedeAccion('reabrir')" data-testid="btn-accion-reabrir" @click="abrirModal('reabrir')">
          Reabrir
        </button>
        <button v-if="puedeAccion('cancelar')" data-testid="btn-accion-cancelar" @click="abrirModal('cancelar')">
          Cancelar
        </button>
      </div>
    </div>

    <div v-if="modalAbierto" data-testid="modal-accion" class="modal">
      <div class="modal-content">
        <h3>Acción: {{ accionActual }}</h3>
        <input
          v-if="accionActual === 'asignar'"
          v-model="agenteId"
          data-testid="modal-select-agente"
          placeholder="ID del agente (GUID)"
        />
        <textarea
          v-if="accionActual === 'resolver' || accionActual === 'cancelar'"
          v-model="motivo"
          data-testid="modal-motivo"
          rows="4"
          placeholder="Motivo..."
        />
        <p v-if="modalError" data-testid="modal-error" class="error">{{ modalError }}</p>
        <div class="modal-actions">
          <button data-testid="modal-confirmar" @click="confirmarAccion">Confirmar</button>
          <button data-testid="modal-cancelar" @click="modalAbierto = false">Cancelar</button>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
.actions {
  display: flex;
  flex-wrap: wrap;
  gap: 0.5rem;
  margin-top: 1rem;
}
.modal {
  position: fixed;
  inset: 0;
  background: rgba(0, 0, 0, 0.4);
  display: grid;
  place-items: center;
}
.modal-content {
  background: white;
  padding: 1.5rem;
  border-radius: 8px;
  width: 420px;
  display: grid;
  gap: 0.75rem;
}
.error {
  color: #b91c1c;
}
</style>
