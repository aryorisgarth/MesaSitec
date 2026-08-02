import { apiFetch } from './client'
import type {
  CategoriaDto,
  LoginResponse,
  PaginatedResponse,
  SolicitudDetalleDto,
  SolicitudListItemDto,
  UsuarioDto,
} from '../types/api'

export function login(email: string, password: string) {
  return apiFetch<LoginResponse>('/auth/login', {
    method: 'POST',
    body: JSON.stringify({ email, password }),
  })
}

export function getMe() {
  return apiFetch<UsuarioDto>('/me')
}

export function getCategorias() {
  return apiFetch<CategoriaDto[]>('/categorias')
}

export interface SolicitudFilters {
  estado?: string
  prioridad?: string
  categoriaId?: string
  vencidas?: boolean
  q?: string
  page?: number
  pageSize?: number
  sort?: string
}

export function getSolicitudes(filters: SolicitudFilters = {}) {
  const params = new URLSearchParams()
  Object.entries(filters).forEach(([key, value]) => {
    if (value !== undefined && value !== '' && value !== false) {
      params.set(key, String(value))
    }
  })
  const query = params.toString()
  return apiFetch<PaginatedResponse<SolicitudListItemDto>>(
    `/solicitudes${query ? `?${query}` : ''}`,
  )
}

export function getSolicitud(id: string) {
  return apiFetch<SolicitudDetalleDto>(`/solicitudes/${id}`)
}

export function crearSolicitud(body: {
  titulo: string
  descripcion: string
  categoriaId: string
  prioridad: string
}) {
  return apiFetch<SolicitudDetalleDto>('/solicitudes', {
    method: 'POST',
    body: JSON.stringify(body),
  })
}

export function editarSolicitud(
  id: string,
  body: { titulo: string; descripcion: string; categoriaId: string; prioridad: string },
) {
  return apiFetch<SolicitudDetalleDto>(`/solicitudes/${id}`, {
    method: 'PUT',
    body: JSON.stringify(body),
  })
}

export function transicionar(
  id: string,
  body: { accion: string; agenteId?: string; motivo?: string },
) {
  return apiFetch<SolicitudDetalleDto>(`/solicitudes/${id}/transiciones`, {
    method: 'POST',
    body: JSON.stringify(body),
  })
}
