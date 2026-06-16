import apiClient from './apiClient'

export interface LoginResponse {
  token: string
  email: string
}

export interface UserProfile {
  userId: number
  email: string
  planName: string
  remainingRequests: number
  endDate?: string
  isActive: boolean
}

export async function login(email: string, password: string): Promise<LoginResponse> {
  const { data } = await apiClient.post<LoginResponse>('/api/auth/login', { email, password })
  localStorage.setItem('aihub_token', data.token)
  localStorage.setItem('aihub_email', data.email)
  return data
}

export async function register(email: string, password: string): Promise<void> {
  await apiClient.post('/api/auth/register', { email, password })
}

export function logout(): void {
  localStorage.removeItem('aihub_token')
  localStorage.removeItem('aihub_email')
}

export function isAuthenticated(): boolean {
  return !!localStorage.getItem('aihub_token')
}

export async function getProfile(): Promise<UserProfile> {
  const { data } = await apiClient.get<UserProfile>('/api/user/profile')
  return data
}
