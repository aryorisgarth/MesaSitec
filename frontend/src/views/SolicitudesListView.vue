<script setup lang="ts">
import { computed, onMounted, ref, watch } from 'vue'
import { useRouter } from 'vue-router'
import { getCategorias, getSolicitudes } from '../api/solicitudes'
import type { CategoriaDto, SolicitudListItemDto } from '../types/api'

const router = useRouter()
const items = ref<SolicitudListItemDto[]>([])
const categorias = ref<CategoriaDto[]>([])
const loading = ref(true)
const error = ref('')
const page = ref(1)
const pageSize = ref(20)
const total = ref(0)
const totalPaginas = ref(0)

const filtroEstado = ref('')
const filtroPrioridad = ref('')
const filtroCategoria = ref('')
const filtroVencidas = ref(false)
const filtroBusqueda = ref('')

const paginacionInfo = computed(
  () => `Página ${page.value} de ${totalPaginas.value} — ${total.value} resultados`,
)

async function cargar() {
  loading.value = true
  error.value = ''
  try {
    const res = await getSolicitudes({
      estado: filtroEstado.value || undefined,
      prioridad: filtroPrioridad.value || undefined,
      categoriaId: filtroCategoria.value || undefined,
      vencidas: filtroVencidas.value || undefined,
      q: filtroBusqueda.value || undefined,
      page: page.value,
      pageSize: pageSize.value,
      sort: '-fechaCreacion',
    })
    items.value = res.items
    total.value = res.total
    totalPaginas.value = res.totalPaginas
  } catch (e) {
    error.value = e instanceof Error ? e.message : 'Error al cargar solicitudes'
  } finally {
    loading.value = false
  }
}

function limpiarFiltros() {
  filtroEstado.value = ''
  filtroPrioridad.value = ''
  filtroCategoria.value = ''
  filtroVencidas.value = false
  filtroBusqueda.value = ''
  page.value = 1
  void cargar()
}

watch([filtroEstado, filtroPrioridad, filtroCategoria, filtroVencidas], () => {
  page.value = 1
  void cargar()
})

onMounted(async () => {
  categorias.value = await getCategorias()
  await cargar()
})
</script>

<template>
  <div>
    <div class="toolbar">
      <button data-testid="btn-nueva-solicitud" @click="router.push('/solicitudes/nueva')">
        Nueva solicitud
      </button>
      <select v-model="filtroEstado" data-testid="filtro-estado">
        <option value="">Estado</option>
        <option value="Nueva">Nueva</option>
        <option value="Asignada">Asignada</option>
        <option value="EnProceso">EnProceso</option>
        <option value="Resuelta">Resuelta</option>
        <option value="Cerrada">Cerrada</option>
        <option value="Cancelada">Cancelada</option>
      </select>
      <select v-model="filtroPrioridad" data-testid="filtro-prioridad">
        <option value="">Prioridad</option>
        <option value="Baja">Baja</option>
        <option value="Media">Media</option>
        <option value="Alta">Alta</option>
        <option value="Critica">Critica</option>
      </select>
      <select v-model="filtroCategoria" data-testid="filtro-categoria">
        <option value="">Categoría</option>
        <option v-for="c in categorias" :key="c.id" :value="c.id">{{ c.nombre }}</option>
      </select>
      <label>
        <input v-model="filtroVencidas" data-testid="filtro-vencidas" type="checkbox" />
        Vencidas
      </label>
      <input
        v-model="filtroBusqueda"
        data-testid="filtro-busqueda"
        placeholder="Buscar..."
        @keyup.enter="page = 1; cargar()"
      />
      <button data-testid="btn-limpiar-filtros" @click="limpiarFiltros">Limpiar</button>
    </div>

    <p v-if="loading" data-testid="listado-cargando">Cargando solicitudes...</p>
    <p v-else-if="error" class="error">{{ error }}</p>
    <p v-else-if="items.length === 0" data-testid="listado-vacio">No hay solicitudes.</p>

    <table v-else data-testid="tabla-solicitudes">
      <thead>
        <tr>
          <th>Código</th>
          <th>Título</th>
          <th>Estado</th>
          <th>Prioridad</th>
          <th>SLA</th>
        </tr>
      </thead>
      <tbody>
        <tr
          v-for="item in items"
          :key="item.id"
          data-testid="fila-solicitud"
          :data-codigo="item.codigo"
          class="clickable"
          @click="router.push(`/solicitudes/${item.id}`)"
        >
          <td data-testid="celda-codigo">{{ item.codigo }}</td>
          <td>{{ item.titulo }}</td>
          <td data-testid="celda-estado">{{ item.estado }}</td>
          <td data-testid="celda-prioridad">{{ item.prioridad }}</td>
          <td data-testid="celda-sla">
            {{ new Date(item.fechaLimiteSla).toLocaleString() }}
            <span v-if="item.vencida" data-testid="badge-vencida">Vencida</span>
          </td>
        </tr>
      </tbody>
    </table>

    <div v-if="!loading && totalPaginas > 0" class="paginacion">
      <button
        data-testid="paginacion-anterior"
        :disabled="page <= 1"
        @click="page--; cargar()"
      >
        Anterior
      </button>
      <span data-testid="paginacion-info">{{ paginacionInfo }}</span>
      <button
        data-testid="paginacion-siguiente"
        :disabled="page >= totalPaginas"
        @click="page++; cargar()"
      >
        Siguiente
      </button>
    </div>
  </div>
</template>

<style scoped>
.toolbar {
  display: flex;
  flex-wrap: wrap;
  gap: 0.5rem;
  margin-bottom: 1rem;
}
table {
  width: 100%;
  border-collapse: collapse;
}
th, td {
  border: 1px solid #ddd;
  padding: 0.5rem;
}
.clickable {
  cursor: pointer;
}
.paginacion {
  display: flex;
  gap: 1rem;
  align-items: center;
  margin-top: 1rem;
}
.error {
  color: #b91c1c;
}
</style>
