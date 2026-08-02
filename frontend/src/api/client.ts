import type { ApiProblem } from '../types/api'

const API_BASE = 'http://localhost:5080/api/v1'

export class ApiError extends Error {
  codigo: string
  status: number
  errores?: Record<string, string[]>

  constructor(status: number, problem: ApiProblem) {
    super(problem.detail)
    this.codigo = problem.codigo
    this.status = status
    this.errores = problem.errores
  }
}

function getToken(): string | null {
  return localStorage.getItem('accessToken')
}

export async function apiFetch<T>(
  path: string,
  options: RequestInit = {},
): Promise<T> {
  const headers = new Headers(options.headers)
  headers.set('Content-Type', 'application/json')

  const token = getToken()
  if (token) {
    headers.set('Authorization', `Bearer ${token}`)
  }

  const response = await fetch(`${API_BASE}${path}`, {
    ...options,
    headers,
  })

  if (response.status === 401) {
    localStorage.removeItem('accessToken')
    localStorage.removeItem('usuario')
    if (!window.location.pathname.startsWith('/login')) {
      window.location.href = '/login'
    }
    throw new ApiError(401, { codigo: 'NO_AUTENTICADO', detail: 'Sesión expirada.' })
  }

  if (!response.ok) {
    const problem = (await response.json()) as ApiProblem
    throw new ApiError(response.status, problem)
  }

  if (response.status === 204) {
    return undefined as T
  }

  return (await response.json()) as T
}
