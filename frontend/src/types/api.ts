export interface UsuarioDto {
  id: string
  nombre: string
  email: string
  rol: 'Admin' | 'Agente' | 'Solicitante'
  tenantId: string
  tenantNombre: string
}

export interface LoginResponse {
  accessToken: string
  expiraEn: number
  usuario: UsuarioDto
}

export interface CategoriaDto {
  id: string
  nombre: string
  slaHoras: number
}

export interface CategoriaResumenDto {
  id: string
  nombre: string
}

export interface AgenteResumenDto {
  id: string
  nombre: string
}

export interface SolicitudListItemDto {
  id: string
  codigo: string
  titulo: string
  estado: string
  prioridad: string
  categoria: CategoriaResumenDto
  agente: AgenteResumenDto | null
  fechaCreacion: string
  fechaLimiteSla: string
  vencida: boolean
}

export interface SolicitudDetalleDto extends SolicitudListItemDto {
  descripcion: string
  solicitante: UsuarioDto
  fechaResolucion: string | null
  motivoResolucion: string | null
  motivoCancelacion: string | null
}

export interface PaginatedResponse<T> {
  items: T[]
  page: number
  pageSize: number
  total: number
  totalPaginas: number
}

export interface ApiProblem {
  codigo: string
  detail: string
  errores?: Record<string, string[]>
}

export type AccionTransicion =
  | 'asignar'
  | 'iniciar'
  | 'resolver'
  | 'cerrar'
  | 'reabrir'
  | 'cancelar'
