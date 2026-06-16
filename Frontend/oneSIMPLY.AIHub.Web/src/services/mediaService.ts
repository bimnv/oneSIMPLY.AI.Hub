import apiClient from './apiClient'

const API_BASE = import.meta.env.VITE_API_URL || 'http://localhost:5000'

export function resolveMediaUrl(path?: string | null): string {
  if (!path) return ''
  if (path.startsWith('http')) return path
  return `${API_BASE}${path}`
}

export async function uploadProductImage(file: File): Promise<string> {
  const formData = new FormData()
  formData.append('image', file)

  const { data } = await apiClient.post<{ imageUrl: string }>('/api/media/upload-image', formData, {
    transformRequest: [(payload, headers) => {
      delete headers['Content-Type']
      return payload
    }],
  })

  return data.imageUrl
}
