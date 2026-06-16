import apiClient from './apiClient'

export interface SocialAccount {
  id: number
  platform: string
  pageId: string
  pageName: string
  tokenPreview: string
  updatedAt: string
}

export interface SaveSocialAccountPayload {
  platform: 'facebook' | 'tiktok'
  pageId: string
  pageName: string
  accessToken: string
}

export async function getSocialAccounts(): Promise<SocialAccount[]> {
  const { data } = await apiClient.get<SocialAccount[]>('/api/social/accounts')
  return data
}

export async function saveSocialAccount(payload: SaveSocialAccountPayload): Promise<void> {
  await apiClient.post('/api/social/accounts', payload)
}

export async function disconnectSocialAccount(platform: string): Promise<void> {
  await apiClient.delete(`/api/social/accounts/${platform}`)
}

export function hasPlatformConnected(accounts: SocialAccount[], platform: string): boolean {
  return accounts.some((a) => a.platform.toLowerCase() === platform.toLowerCase())
}
