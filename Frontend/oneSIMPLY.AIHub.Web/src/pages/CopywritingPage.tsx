import { type ChangeEvent, type FormEvent, useEffect, useRef, useState } from 'react'
import { Copy, ImagePlus, Link2, Rocket, Sparkles, X } from 'lucide-react'
import { Link } from 'react-router-dom'
import apiClient from '../services/apiClient'
import { getProfile, type UserProfile } from '../services/authService'
import { resolveMediaUrl, uploadProductImage } from '../services/mediaService'
import { getSocialAccounts, hasPlatformConnected, type SocialAccount } from '../services/socialService'

const tones = ['Thuyết phục', 'Hài hước', 'Chuyên nghiệp', 'Gần gũi', 'Khẩn cấp / FOMO']
const formulas = ['AIDA', 'PAS']

function streamText(fullText: string, onUpdate: (text: string) => void, onDone: () => void) {
  let index = 0
  const interval = setInterval(() => {
    index += 3
    onUpdate(fullText.slice(0, index))
    if (index >= fullText.length) {
      clearInterval(interval)
      onDone()
    }
  }, 20)
  return () => clearInterval(interval)
}

export default function CopywritingPage() {
  const [profile, setProfile] = useState<UserProfile | null>(null)
  const [productName, setProductName] = useState('')
  const [features, setFeatures] = useState('')
  const [tone, setTone] = useState(tones[0])
  const [formula, setFormula] = useState(formulas[0])
  const [resultText, setResultText] = useState('')
  const [loading, setLoading] = useState(false)
  const [streaming, setStreaming] = useState(false)
  const [error, setError] = useState('')
  const [pushFacebook, setPushFacebook] = useState(true)
  const [pushTiktok, setPushTiktok] = useState(false)
  const [pushing, setPushing] = useState(false)
  const [pushMessage, setPushMessage] = useState('')
  const [imageUrl, setImageUrl] = useState('')
  const [imagePreview, setImagePreview] = useState('')
  const [uploadingImage, setUploadingImage] = useState(false)
  const [socialAccounts, setSocialAccounts] = useState<SocialAccount[]>([])
  const fileInputRef = useRef<HTMLInputElement>(null)

  useEffect(() => {
    getProfile().then(setProfile).catch(() => setProfile(null))
    getSocialAccounts().then(setSocialAccounts).catch(() => setSocialAccounts([]))
  }, [])

  const handleGenerate = async (e: FormEvent) => {
    e.preventDefault()
    setError('')
    setPushMessage('')
    setLoading(true)
    setResultText('')

    try {
      const { data } = await apiClient.post<{ result: string; remainingRequests: number }>(
        '/api/ai/copywriting',
        { productName, features, tone, formula, imageUrl: imageUrl || null }
      )

      setStreaming(true)
      streamText(
        data.result,
        setResultText,
        () => {
          setStreaming(false)
          setProfile((prev) =>
            prev ? { ...prev, remainingRequests: data.remainingRequests } : prev
          )
        }
      )
    } catch (err: unknown) {
      const message =
        typeof err === 'object' && err !== null && 'response' in err
          ? (err as { response?: { data?: { error?: string } } }).response?.data?.error
          : undefined
      setError(message || 'Không thể tạo bài viết. Vui lòng thử lại.')
    } finally {
      setLoading(false)
    }
  }

  const handleCopy = async () => {
    await navigator.clipboard.writeText(resultText)
    setPushMessage('Đã sao chép bài viết!')
  }

  const handleImageSelect = async (e: ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0]
    if (!file) return

    setError('')
    setUploadingImage(true)
    setImagePreview(URL.createObjectURL(file))

    try {
      const url = await uploadProductImage(file)
      setImageUrl(url)
      setPushMessage('Đã tải ảnh sản phẩm lên!')
    } catch (err: unknown) {
      setImagePreview('')
      setImageUrl('')
      const message =
        typeof err === 'object' && err !== null && 'response' in err
          ? (err as { response?: { data?: string | { error?: string } } }).response?.data
          : undefined
      setError(typeof message === 'string' ? message : (message as { error?: string })?.error || 'Không thể tải ảnh. Thử lại.')
    } finally {
      setUploadingImage(false)
      if (fileInputRef.current) fileInputRef.current.value = ''
    }
  }

  const handleRemoveImage = () => {
    setImageUrl('')
    setImagePreview('')
    if (fileInputRef.current) fileInputRef.current.value = ''
  }

  const handlePush = async () => {
    if (!resultText.trim()) {
      setError('Chưa có nội dung để đăng.')
      return
    }

    const channels: string[] = []
    if (pushFacebook) channels.push('facebook')
    if (pushTiktok) channels.push('tiktok')

    if (channels.length === 0) {
      setError('Vui lòng chọn ít nhất một kênh đăng.')
      return
    }

    if (pushFacebook && !hasPlatformConnected(socialAccounts, 'facebook')) {
      setError('Chưa khai báo Facebook. Vào menu "Kết nối kênh đăng" để nhập Page ID & Access Token.')
      return
    }
    if (pushTiktok && !hasPlatformConnected(socialAccounts, 'tiktok')) {
      setError('Chưa khai báo TikTok. Vào menu "Kết nối kênh đăng" để cấu hình tài khoản.')
      return
    }

    setPushing(true)
    setError('')
    try {
      const { data } = await apiClient.post<{ message: string }>('/api/social/publish', {
        content: resultText,
        channels,
        imageUrl: imageUrl || null,
      })
      setPushMessage(data.message)
    } catch (err: unknown) {
      const message =
        typeof err === 'object' && err !== null && 'response' in err
          ? (err as { response?: { data?: string | { error?: string } } }).response?.data
          : undefined
      setError(typeof message === 'string' ? message : (message as { error?: string })?.error || 'Không thể PUSH bài viết. Vui lòng thử lại.')
    } finally {
      setPushing(false)
    }
  }

  return (
    <div className="p-6 lg:p-8">
      <div className="flex flex-wrap items-center justify-between gap-4 mb-6">
        <div>
          <h2 className="text-2xl font-bold text-slate-800">Viết bài bán hàng AI</h2>
          <p className="text-sm text-slate-500">Điền form ngắn — hệ thống tự bọc prompt tâm lý ở backend</p>
        </div>
        {profile && (
          <div className="flex items-center gap-2 px-4 py-2 bg-white border border-slate-200 rounded-xl text-sm">
            <Sparkles size={16} className="text-brand-orange" />
            <span>
              Còn <strong className="text-brand-blue">{profile.remainingRequests}</strong> lượt · {profile.planName}
            </span>
          </div>
        )}
      </div>

      <div className="grid grid-cols-1 xl:grid-cols-2 gap-6">
        <form onSubmit={handleGenerate} className="bg-white rounded-2xl border border-slate-200 p-6 space-y-4 shadow-sm">
          <div>
            <label className="block text-sm font-medium text-slate-700 mb-1">Tên sản phẩm / dịch vụ</label>
            <input
              required
              value={productName}
              onChange={(e) => setProductName(e.target.value)}
              className="w-full px-4 py-2.5 border border-slate-200 rounded-lg focus:ring-2 focus:ring-brand-orange/30 focus:border-brand-orange outline-none"
              placeholder="VD: Kem dưỡng da collagen Hàn Quốc"
            />
          </div>
          <div>
            <label className="block text-sm font-medium text-slate-700 mb-1">Đặc tính nổi bật</label>
            <textarea
              required
              rows={5}
              value={features}
              onChange={(e) => setFeatures(e.target.value)}
              className="w-full px-4 py-2.5 border border-slate-200 rounded-lg focus:ring-2 focus:ring-brand-orange/30 focus:border-brand-orange outline-none resize-none"
              placeholder="Mỗi dòng một ưu điểm..."
            />
          </div>

          <div>
            <label className="block text-sm font-medium text-slate-700 mb-1">Ảnh sản phẩm (tuỳ chọn)</label>
            <input
              ref={fileInputRef}
              type="file"
              accept="image/jpeg,image/png,image/webp"
              className="hidden"
              onChange={handleImageSelect}
            />
            {imagePreview || imageUrl ? (
              <div className="relative border border-slate-200 rounded-lg overflow-hidden bg-slate-50">
                <img
                  src={imagePreview || resolveMediaUrl(imageUrl)}
                  alt="Ảnh sản phẩm"
                  className="w-full max-h-48 object-contain"
                />
                <button
                  type="button"
                  onClick={handleRemoveImage}
                  className="absolute top-2 right-2 p-1.5 bg-white/90 rounded-full text-slate-600 hover:text-rose-600 shadow"
                  title="Xóa ảnh"
                >
                  <X size={16} />
                </button>
              </div>
            ) : (
              <button
                type="button"
                onClick={() => fileInputRef.current?.click()}
                disabled={uploadingImage}
                className="w-full py-8 border-2 border-dashed border-slate-200 rounded-lg text-slate-500 hover:border-brand-orange hover:text-brand-orange transition-colors flex flex-col items-center gap-2"
              >
                <ImagePlus size={28} />
                <span className="text-sm font-medium">
                  {uploadingImage ? 'Đang tải ảnh lên...' : 'Bấm để chọn ảnh sản phẩm'}
                </span>
                <span className="text-xs text-slate-400">JPG, PNG, WEBP · tối đa 5MB</span>
              </button>
            )}
          </div>

          <div className="grid grid-cols-2 gap-4">
            <div>
              <label className="block text-sm font-medium text-slate-700 mb-1">Giọng văn</label>
              <select
                value={tone}
                onChange={(e) => setTone(e.target.value)}
                className="w-full px-4 py-2.5 border border-slate-200 rounded-lg outline-none"
              >
                {tones.map((t) => (
                  <option key={t} value={t}>{t}</option>
                ))}
              </select>
            </div>
            <div>
              <label className="block text-sm font-medium text-slate-700 mb-1">Công thức tâm lý</label>
              <select
                value={formula}
                onChange={(e) => setFormula(e.target.value)}
                className="w-full px-4 py-2.5 border border-slate-200 rounded-lg outline-none"
              >
                {formulas.map((f) => (
                  <option key={f} value={f}>{f}</option>
                ))}
              </select>
            </div>
          </div>
          <button
            type="submit"
            disabled={loading || streaming}
            className="w-full py-3 bg-brand-orange hover:bg-brand-orange-dark text-white font-bold rounded-lg transition-colors disabled:opacity-60"
          >
            {loading ? 'AI đang viết bài...' : 'Tạo bài viết AI'}
          </button>
        </form>

        <div className="bg-white rounded-2xl border border-slate-200 p-6 shadow-sm flex flex-col">
          <div className="flex items-center justify-between mb-3">
            <h3 className="font-semibold text-slate-800">Duyệt bài & Push đa kênh</h3>
            {resultText && (
              <button
                type="button"
                onClick={handleCopy}
                className="flex items-center gap-1 text-sm text-brand-blue hover:underline"
              >
                <Copy size={14} /> Sao chép
              </button>
            )}
          </div>

          <textarea
            value={resultText}
            onChange={(e) => setResultText(e.target.value)}
            rows={14}
            className="flex-1 w-full px-4 py-3 border border-slate-200 rounded-lg focus:ring-2 focus:ring-brand-blue/30 focus:border-brand-blue outline-none resize-none font-mono text-sm leading-relaxed"
            placeholder={streaming ? 'Đang hiển thị kết quả...' : 'Bài viết AI sẽ hiện ở đây...'}
          />

          <div className="mt-4 flex flex-wrap gap-4 items-center">
            <label className="flex items-center gap-2 text-sm cursor-pointer">
              <input type="checkbox" checked={pushFacebook} onChange={(e) => setPushFacebook(e.target.checked)} />
              Facebook Page
              {!hasPlatformConnected(socialAccounts, 'facebook') && (
                <span className="text-xs text-amber-600">(chưa kết nối)</span>
              )}
            </label>
            <label className="flex items-center gap-2 text-sm cursor-pointer">
              <input type="checkbox" checked={pushTiktok} onChange={(e) => setPushTiktok(e.target.checked)} />
              TikTok
              {!hasPlatformConnected(socialAccounts, 'tiktok') && (
                <span className="text-xs text-amber-600">(chưa kết nối)</span>
              )}
            </label>
            <Link
              to="/social"
              className="ml-auto flex items-center gap-1 text-xs text-brand-blue hover:underline"
            >
              <Link2 size={14} /> Kết nối kênh đăng
            </Link>
          </div>

          {imageUrl && (
            <p className="mt-2 text-xs text-emerald-700">Ảnh sản phẩm sẽ được đính kèm khi PUSH.</p>
          )}

          <button
            type="button"
            onClick={handlePush}
            disabled={pushing || !resultText}
            className="mt-4 w-full py-3 bg-brand-blue hover:bg-brand-blue-dark text-white font-bold rounded-lg flex items-center justify-center gap-2 transition-colors disabled:opacity-60"
          >
            <Rocket size={18} />
            {pushing ? 'Đang PUSH...' : 'PUSH ĐĂNG NGAY'}
          </button>

          {error && <p className="mt-3 text-sm text-rose-600 bg-rose-50 px-3 py-2 rounded-lg">{error}</p>}
          {pushMessage && <p className="mt-3 text-sm text-emerald-700 bg-emerald-50 px-3 py-2 rounded-lg">{pushMessage}</p>}
        </div>
      </div>
    </div>
  )
}
