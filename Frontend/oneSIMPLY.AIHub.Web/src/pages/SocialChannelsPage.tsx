import { type FormEvent, useEffect, useState } from 'react'
import { Link2, Music2, Save, Share2, Trash2 } from 'lucide-react'
import {
  disconnectSocialAccount,
  getSocialAccounts,
  saveSocialAccount,
  type SocialAccount,
} from '../services/socialService'

interface PlatformForm {
  pageId: string
  pageName: string
  accessToken: string
}

const emptyForm: PlatformForm = { pageId: '', pageName: '', accessToken: '' }

export default function SocialChannelsPage() {
  const [accounts, setAccounts] = useState<SocialAccount[]>([])
  const [facebookForm, setFacebookForm] = useState<PlatformForm>(emptyForm)
  const [tiktokForm, setTiktokForm] = useState<PlatformForm>(emptyForm)
  const [loading, setLoading] = useState(true)
  const [saving, setSaving] = useState<string | null>(null)
  const [message, setMessage] = useState('')
  const [error, setError] = useState('')

  const loadAccounts = async () => {
    setLoading(true)
    try {
      const data = await getSocialAccounts()
      setAccounts(data)
    } catch {
      setError('Không tải được danh sách kết nối.')
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => {
    loadAccounts()
  }, [])

  const handleSave = async (e: FormEvent, platform: 'facebook' | 'tiktok') => {
    e.preventDefault()
    setError('')
    setMessage('')
    const form = platform === 'facebook' ? facebookForm : tiktokForm

    if (!form.pageId.trim() || !form.accessToken.trim()) {
      setError('Page ID và Access Token là bắt buộc.')
      return
    }

    setSaving(platform)
    try {
      await saveSocialAccount({
        platform,
        pageId: form.pageId.trim(),
        pageName: form.pageName.trim() || (platform === 'facebook' ? 'Facebook Page' : 'TikTok Account'),
        accessToken: form.accessToken.trim(),
      })
      setMessage(`Đã lưu kết nối ${platform === 'facebook' ? 'Facebook' : 'TikTok'} thành công!`)
      if (platform === 'facebook') setFacebookForm(emptyForm)
      else setTiktokForm(emptyForm)
      await loadAccounts()
    } catch {
      setError('Lưu thất bại. Kiểm tra lại thông tin và thử lại.')
    } finally {
      setSaving(null)
    }
  }

  const handleDisconnect = async (platform: string) => {
    setError('')
    setMessage('')
    try {
      await disconnectSocialAccount(platform)
      setMessage(`Đã ngắt kết nối ${platform}.`)
      await loadAccounts()
    } catch {
      setError('Không thể ngắt kết nối.')
    }
  }

  const fbAccount = accounts.find((a) => a.platform === 'facebook')
  const ttAccount = accounts.find((a) => a.platform === 'tiktok')

  return (
    <div className="p-6 lg:p-8 max-w-4xl">
      <div className="mb-8">
        <h2 className="text-2xl font-bold text-slate-800 flex items-center gap-2">
          <Link2 className="text-brand-blue" />
          Kết nối kênh đăng bài
        </h2>
        <p className="text-sm text-slate-500 mt-1">
          Khai báo Facebook Page ID & Access Token để hệ thống PUSH bài tự động sau khi duyệt.
        </p>
      </div>

      {message && <p className="mb-4 text-sm text-emerald-700 bg-emerald-50 px-4 py-3 rounded-lg">{message}</p>}
      {error && <p className="mb-4 text-sm text-rose-600 bg-rose-50 px-4 py-3 rounded-lg">{error}</p>}

      <div className="space-y-6">
        <div className="bg-white border border-slate-200 rounded-2xl p-6 shadow-sm">
          <div className="flex items-center justify-between mb-4">
            <div className="flex items-center gap-2">
              <Share2 className="text-brand-blue" size={22} />
              <h3 className="font-bold text-lg">Facebook Fanpage</h3>
            </div>
            {fbAccount && (
              <span className="text-xs bg-emerald-100 text-emerald-700 px-2 py-1 rounded-full font-medium">
                Đã kết nối: {fbAccount.pageName}
              </span>
            )}
          </div>

          {fbAccount && (
            <div className="mb-4 p-3 bg-slate-50 rounded-lg text-sm text-slate-600">
              <p><strong>Page ID:</strong> {fbAccount.pageId}</p>
              <p><strong>Token:</strong> {fbAccount.tokenPreview}</p>
              <button
                type="button"
                onClick={() => handleDisconnect('facebook')}
                className="mt-2 flex items-center gap-1 text-rose-600 hover:underline text-xs"
              >
                <Trash2 size={14} /> Ngắt kết nối
              </button>
            </div>
          )}

          <form onSubmit={(e) => handleSave(e, 'facebook')} className="space-y-3">
            <div>
              <label className="block text-sm font-medium text-slate-700 mb-1">Facebook Page ID</label>
              <input
                value={facebookForm.pageId}
                onChange={(e) => setFacebookForm({ ...facebookForm, pageId: e.target.value })}
                className="w-full px-4 py-2.5 border border-slate-200 rounded-lg outline-none focus:border-brand-blue"
                placeholder="VD: 123456789012345"
              />
            </div>
            <div>
              <label className="block text-sm font-medium text-slate-700 mb-1">Tên Fanpage (tuỳ chọn)</label>
              <input
                value={facebookForm.pageName}
                onChange={(e) => setFacebookForm({ ...facebookForm, pageName: e.target.value })}
                className="w-full px-4 py-2.5 border border-slate-200 rounded-lg outline-none focus:border-brand-blue"
                placeholder="VD: Shop Mỹ Phẩm ABC"
              />
            </div>
            <div>
              <label className="block text-sm font-medium text-slate-700 mb-1">Page Access Token</label>
              <textarea
                rows={3}
                value={facebookForm.accessToken}
                onChange={(e) => setFacebookForm({ ...facebookForm, accessToken: e.target.value })}
                className="w-full px-4 py-2.5 border border-slate-200 rounded-lg outline-none focus:border-brand-blue resize-none font-mono text-xs"
                placeholder="EAAB... (lấy từ Facebook Developer / Graph API Explorer)"
              />
              <p className="text-xs text-slate-400 mt-1">
                Tạo token tại{' '}
                <a href="https://developers.facebook.com/tools/explorer/" target="_blank" rel="noreferrer" className="text-brand-blue hover:underline">
                  Graph API Explorer
                </a>
                {' '}với quyền <code className="bg-slate-100 px-1 rounded">pages_manage_posts</code>
              </p>
            </div>
            <button
              type="submit"
              disabled={saving === 'facebook'}
              className="flex items-center gap-2 px-5 py-2.5 bg-brand-blue text-white font-semibold rounded-lg hover:bg-brand-blue-dark disabled:opacity-60"
            >
              <Save size={16} />
              {saving === 'facebook' ? 'Đang lưu...' : 'Lưu kết nối Facebook'}
            </button>
          </form>
        </div>

        <div className="bg-white border border-slate-200 rounded-2xl p-6 shadow-sm">
          <div className="flex items-center justify-between mb-4">
            <div className="flex items-center gap-2">
              <Music2 className="text-brand-orange" size={22} />
              <h3 className="font-bold text-lg">TikTok</h3>
            </div>
            {ttAccount && (
              <span className="text-xs bg-emerald-100 text-emerald-700 px-2 py-1 rounded-full font-medium">
                Đã kết nối: {ttAccount.pageName}
              </span>
            )}
          </div>

          {ttAccount && (
            <div className="mb-4 p-3 bg-slate-50 rounded-lg text-sm text-slate-600">
              <p><strong>Account ID:</strong> {ttAccount.pageId}</p>
              <p><strong>Token:</strong> {ttAccount.tokenPreview}</p>
              <button
                type="button"
                onClick={() => handleDisconnect('tiktok')}
                className="mt-2 flex items-center gap-1 text-rose-600 hover:underline text-xs"
              >
                <Trash2 size={14} /> Ngắt kết nối
              </button>
            </div>
          )}

          <form onSubmit={(e) => handleSave(e, 'tiktok')} className="space-y-3">
            <div>
              <label className="block text-sm font-medium text-slate-700 mb-1">TikTok Open ID / User ID</label>
              <input
                value={tiktokForm.pageId}
                onChange={(e) => setTiktokForm({ ...tiktokForm, pageId: e.target.value })}
                className="w-full px-4 py-2.5 border border-slate-200 rounded-lg outline-none focus:border-brand-orange"
                placeholder="Open ID từ TikTok Developer"
              />
            </div>
            <div>
              <label className="block text-sm font-medium text-slate-700 mb-1">Tên tài khoản (tuỳ chọn)</label>
              <input
                value={tiktokForm.pageName}
                onChange={(e) => setTiktokForm({ ...tiktokForm, pageName: e.target.value })}
                className="w-full px-4 py-2.5 border border-slate-200 rounded-lg outline-none focus:border-brand-orange"
                placeholder="VD: TikTok Shop XYZ"
              />
            </div>
            <div>
              <label className="block text-sm font-medium text-slate-700 mb-1">Access Token</label>
              <textarea
                rows={3}
                value={tiktokForm.accessToken}
                onChange={(e) => setTiktokForm({ ...tiktokForm, accessToken: e.target.value })}
                className="w-full px-4 py-2.5 border border-slate-200 rounded-lg outline-none focus:border-brand-orange resize-none font-mono text-xs"
                placeholder="Token từ TikTok for Developers"
              />
            </div>
            <button
              type="submit"
              disabled={saving === 'tiktok'}
              className="flex items-center gap-2 px-5 py-2.5 bg-brand-orange text-white font-semibold rounded-lg hover:bg-brand-orange-dark disabled:opacity-60"
            >
              <Save size={16} />
              {saving === 'tiktok' ? 'Đang lưu...' : 'Lưu kết nối TikTok'}
            </button>
          </form>
        </div>
      </div>

      {!loading && accounts.length === 0 && (
        <p className="mt-6 text-sm text-amber-700 bg-amber-50 px-4 py-3 rounded-lg">
          Chưa kết nối kênh nào. Cấu hình Facebook/TikTok ở trên trước khi bấm PUSH ở trang Viết bài.
        </p>
      )}
    </div>
  )
}
