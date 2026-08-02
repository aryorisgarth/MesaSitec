<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import { useRouter } from 'vue-router'
import { ApiError } from '../api/client'
import { crearSolicitud, editarSolicitud, getCategorias, getSolicitud } from '../api/solicitudes'
import type { CategoriaDto } from '../types/api'

const props = defineProps<{ id?: string; modo?: 'editar' }>()
const router = useRouter()
const categorias = ref<CategoriaDto[]>([])
const titulo = ref('')
const descripcion = ref('')
const categoriaId = ref('')
const prioridad = ref('Media')
const loading = ref(false)
const errorTitulo = ref('')
const errorDescripcion = ref('')
const errorCategoria = ref('')
const errorGeneral = ref('')

const esEdicion = computed(() => props.modo === 'editar' && !!props.id)

onMounted(async () => {
  categorias.value = await getCategorias()
  if (esEdicion.value && props.id) {
    const s = await getSolicitud(props.id)
    titulo.value = s.titulo
    descripcion.value = s.descripcion
    categoriaId.value = s.categoria.id
    prioridad.value = s.prioridad
  }
})

function validarCliente(): boolean {
  errorTitulo.value = titulo.value.trim().length < 5 ? 'Mínimo 5 caracteres' : ''
  errorDescripcion.value = descripcion.value.trim().length < 10 ? 'Mínimo 10 caracteres' : ''
  errorCategoria.value = categoriaId.value ? '' : 'Seleccione categoría'
  return !errorTitulo.value && !errorDescripcion.value && !errorCategoria.value
}

async function submit() {
  if (!validarCliente()) return
  loading.value = true
  errorGeneral.value = ''
  try {
    const body = {
      titulo: titulo.value.trim(),
      descripcion: descripcion.value.trim(),
      categoriaId: categoriaId.value,
      prioridad: prioridad.value,
    }
    const res = esEdicion.value && props.id
      ? await editarSolicitud(props.id, body)
      : await crearSolicitud(body)
    await router.push(`/solicitudes/${res.id}`)
  } catch (e) {
    if (e instanceof ApiError && e.errores) {
      errorTitulo.value = e.errores.titulo?.[0] ?? ''
      errorDescripcion.value = e.errores.descripcion?.[0] ?? ''
      errorCategoria.value = e.errores.categoriaId?.[0] ?? ''
    } else {
      errorGeneral.value =
        e instanceof ApiError
          ? `[${e.codigo}] ${e.message}`
          : e instanceof Error
            ? e.message
            : 'Error al guardar'
    }
  } finally {
    loading.value = false
  }
}
</script>

<template>
  <form class="form" @submit.prevent="submit">
    <h2>{{ esEdicion ? 'Editar solicitud' : 'Nueva solicitud' }}</h2>
    <label>
      Título
      <input v-model="titulo" data-testid="form-titulo" />
      <span v-if="errorTitulo" data-testid="error-titulo" class="error">{{ errorTitulo }}</span>
    </label>
    <label>
      Descripción
      <textarea v-model="descripcion" data-testid="form-descripcion" rows="5" />
      <span v-if="errorDescripcion" data-testid="error-descripcion" class="error">{{ errorDescripcion }}</span>
    </label>
    <label>
      Categoría
      <select v-model="categoriaId" data-testid="form-categoria">
        <option value="">Seleccione...</option>
        <option v-for="c in categorias" :key="c.id" :value="c.id">{{ c.nombre }}</option>
      </select>
      <span v-if="errorCategoria" data-testid="error-categoria" class="error">{{ errorCategoria }}</span>
    </label>
    <label>
      Prioridad
      <select v-model="prioridad" data-testid="form-prioridad">
        <option value="Baja">Baja</option>
        <option value="Media">Media</option>
        <option value="Alta">Alta</option>
        <option value="Critica">Critica</option>
      </select>
    </label>
    <p v-if="errorGeneral" class="error">{{ errorGeneral }}</p>
    <div class="actions">
      <button data-testid="form-submit" type="submit" :disabled="loading">Guardar</button>
      <button data-testid="form-cancelar" type="button" @click="router.back()">Cancelar</button>
    </div>
  </form>
</template>

<style scoped>
.form {
  max-width: 640px;
  display: grid;
  gap: 1rem;
}
label {
  display: grid;
  gap: 0.25rem;
}
input, textarea, select, button {
  padding: 0.5rem;
}
.actions {
  display: flex;
  gap: 0.5rem;
}
.error {
  color: #b91c1c;
  font-size: 0.875rem;
}
</style>
