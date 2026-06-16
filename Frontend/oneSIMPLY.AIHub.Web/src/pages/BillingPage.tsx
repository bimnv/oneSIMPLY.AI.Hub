import { useEffect, useState } from 'react'
import { Check, QrCode, X } from 'lucide-react'
import apiClient from '../services/apiClient'

interface SubscriptionPlan {
  id: number
  name: string
  price: number
  maxRequests: number
  durationDays: number
}

interface QrResponse {
  qrUrl: string
  transactionCode: string
  amount: number
  subscriptionName?: string
}

export default function BillingPage() {
  const [plans, setPlans] = useState<SubscriptionPlan[]>([])
  const [loading, setLoading] = useState(true)
  const [modalOpen, setModalOpen] = useState(false)
  const [qrData, setQrData] = useState<QrResponse | null>(null)
  const [countdown, setCountdown] = useState(300)
  const [paymentSuccess, setPaymentSuccess] = useState(false)
  const [error, setError] = useState('')

  useEffect(() => {
    apiClient
      .get<SubscriptionPlan[]>('/api/subscriptions')
      .then(({ data }) => setPlans(data))
      .catch(() => setError('Không tải được danh sách gói cước.'))
      .finally(() => setLoading(false))
  }, [])

  useEffect(() => {
    if (!modalOpen || !qrData || paymentSuccess) return

    const timer = setInterval(() => {
      setCountdown((c) => (c > 0 ? c - 1 : 0))
    }, 1000)

    const poller = setInterval(async () => {
      try {
        const { data } = await apiClient.get<{ status: string }>(
          `/api/payment/status/${encodeURIComponent(qrData.transactionCode)}`
        )
        if (data.status === 'Success') {
          setPaymentSuccess(true)
          clearInterval(poller)
        }
      } catch {
        // ignore polling errors
      }
    }, 5000)

    return () => {
      clearInterval(timer)
      clearInterval(poller)
    }
  }, [modalOpen, qrData, paymentSuccess])

  const handlePurchase = async (plan: SubscriptionPlan) => {
    if (plan.price === 0) {
      setError('Gói Free Trial được kích hoạt tự động khi đăng ký.')
      return
    }

    setError('')
    try {
      const { data } = await apiClient.post<QrResponse>('/api/payment/generate-qr', {
        subscriptionId: plan.id,
      })
      setQrData(data)
      setCountdown(300)
      setPaymentSuccess(false)
      setModalOpen(true)
    } catch {
      setError('Không tạo được mã QR. Vui lòng đăng nhập và thử lại.')
    }
  }

  const formatPrice = (price: number) =>
    price === 0 ? 'Miễn phí' : `${price.toLocaleString('vi-VN')}đ/tháng`

  const formatTime = (seconds: number) => {
    const m = Math.floor(seconds / 60)
    const s = seconds % 60
    return `${m}:${s.toString().padStart(2, '0')}`
  }

  return (
    <div className="p-6 lg:p-8">
      <div className="mb-8">
        <h2 className="text-2xl font-bold text-slate-800">Gói cước & Nạp tiền</h2>
        <p className="text-sm text-slate-500">Quét mã VietQR — webhook tự động cộng hạn mức</p>
      </div>

      {error && <p className="mb-4 text-sm text-rose-600 bg-rose-50 px-4 py-3 rounded-lg">{error}</p>}

      {loading ? (
        <p className="text-slate-500">Đang tải gói cước...</p>
      ) : (
        <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
          {plans.map((plan, index) => (
            <div
              key={plan.id}
              className={`bg-white rounded-2xl border p-6 shadow-sm relative ${
                index === 1 ? 'border-brand-blue border-2' : 'border-slate-200'
              }`}
            >
              {index === 1 && (
                <span className="absolute -top-3 right-4 bg-brand-orange text-white text-[10px] font-bold px-2 py-0.5 rounded-full uppercase">
                  Bán chạy
                </span>
              )}
              <h3 className="text-lg font-bold text-slate-800">{plan.name}</h3>
              <p className="text-2xl font-extrabold text-brand-blue mt-2">{formatPrice(plan.price)}</p>
              <ul className="mt-4 space-y-2 text-sm text-slate-600">
                <li className="flex items-center gap-2">
                  <Check size={16} className="text-emerald-500" />
                  {plan.maxRequests} lượt AI / {plan.durationDays} ngày
                </li>
                <li className="flex items-center gap-2">
                  <Check size={16} className="text-emerald-500" />
                  Hybrid Routing tiết kiệm chi phí
                </li>
              </ul>
              <button
                type="button"
                onClick={() => handlePurchase(plan)}
                disabled={plan.price === 0}
                className={`mt-6 w-full py-2.5 rounded-lg font-semibold transition-colors ${
                  plan.price === 0
                    ? 'bg-slate-100 text-slate-400 cursor-not-allowed'
                    : index === 1
                      ? 'bg-brand-blue hover:bg-brand-blue-dark text-white'
                      : 'bg-slate-800 hover:bg-slate-900 text-white'
                }`}
              >
                {plan.price === 0 ? 'Đã bao gồm khi đăng ký' : 'Mua gói này'}
              </button>
            </div>
          ))}
        </div>
      )}

      {modalOpen && qrData && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/50 p-4">
          <div className="bg-white rounded-2xl shadow-2xl max-w-md w-full p-6 relative">
            <button
              type="button"
              onClick={() => setModalOpen(false)}
              className="absolute top-4 right-4 text-slate-400 hover:text-slate-600"
            >
              <X size={20} />
            </button>

            {paymentSuccess ? (
              <div className="text-center py-8">
                <div className="w-16 h-16 bg-emerald-100 rounded-full flex items-center justify-center mx-auto mb-4">
                  <Check size={32} className="text-emerald-600" />
                </div>
                <h3 className="text-xl font-bold text-emerald-700">Thanh toán thành công!</h3>
                <p className="text-sm text-slate-500 mt-2">Gói cước đã được kích hoạt tự động.</p>
              </div>
            ) : (
              <>
                <div className="flex items-center gap-2 mb-4">
                  <QrCode className="text-brand-blue" />
                  <h3 className="text-lg font-bold">Quét mã VietQR</h3>
                </div>
                <div className="bg-slate-50 rounded-xl p-4 text-center">
                  <img src={qrData.qrUrl} alt="VietQR" className="mx-auto w-56 h-56 object-contain" />
                  <p className="mt-3 text-lg font-bold text-brand-orange">
                    {qrData.amount.toLocaleString('vi-VN')} VNĐ
                  </p>
                  <p className="text-sm text-slate-600 mt-1">
                    Nội dung CK: <code className="bg-white px-2 py-0.5 rounded font-mono text-xs">{qrData.transactionCode}</code>
                  </p>
                  <p className="text-xs text-slate-400 mt-3">
                    Chờ ngân hàng xác nhận... {formatTime(countdown)}
                  </p>
                </div>
              </>
            )}
          </div>
        </div>
      )}
    </div>
  )
}
