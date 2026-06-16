import { useEffect, useState } from 'react'
import apiClient from '../services/apiClient'

interface UsageLogItem {
  id: number
  modelUsed: string
  taskType: string
  promptTokens: number
  completionTokens: number
  createdAt: string
}

export default function HistoryPage() {
  const [logs, setLogs] = useState<UsageLogItem[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')

  useEffect(() => {
    apiClient
      .get<UsageLogItem[]>('/api/usage/history')
      .then(({ data }) => setLogs(data))
      .catch(() => setError('Không tải được lịch sử.'))
      .finally(() => setLoading(false))
  }, [])

  const formatDate = (value: string) =>
    new Date(value).toLocaleString('vi-VN', {
      day: '2-digit',
      month: '2-digit',
      year: 'numeric',
      hour: '2-digit',
      minute: '2-digit',
    })

  return (
    <div className="p-6 lg:p-8">
      <div className="mb-6">
        <h2 className="text-2xl font-bold text-slate-800">Lịch sử sử dụng AI</h2>
        <p className="text-sm text-slate-500">50 lượt gọi AI gần nhất</p>
      </div>

      {loading && <p className="text-slate-500">Đang tải...</p>}
      {error && <p className="text-rose-600 bg-rose-50 px-4 py-3 rounded-lg">{error}</p>}

      {!loading && !error && logs.length === 0 && (
        <div className="bg-white border border-slate-200 rounded-2xl p-12 text-center text-slate-500">
          Chưa có lịch sử. Hãy thử tạo bài viết AI!
        </div>
      )}

      {!loading && logs.length > 0 && (
        <div className="bg-white border border-slate-200 rounded-2xl overflow-hidden shadow-sm">
          <table className="w-full text-sm">
            <thead className="bg-slate-50 border-b border-slate-200">
              <tr>
                <th className="text-left px-4 py-3 font-semibold text-slate-600">Thời gian</th>
                <th className="text-left px-4 py-3 font-semibold text-slate-600">Loại tác vụ</th>
                <th className="text-left px-4 py-3 font-semibold text-slate-600">Model</th>
                <th className="text-right px-4 py-3 font-semibold text-slate-600">Tokens</th>
              </tr>
            </thead>
            <tbody>
              {logs.map((log) => (
                <tr key={log.id} className="border-b border-slate-100 hover:bg-slate-50/50">
                  <td className="px-4 py-3 text-slate-600">{formatDate(log.createdAt)}</td>
                  <td className="px-4 py-3">
                    <span className="px-2 py-0.5 bg-brand-blue/10 text-brand-blue rounded text-xs font-medium">
                      {log.taskType}
                    </span>
                  </td>
                  <td className="px-4 py-3 text-slate-700">{log.modelUsed}</td>
                  <td className="px-4 py-3 text-right text-slate-500 font-mono text-xs">
                    {log.promptTokens + log.completionTokens}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </div>
  )
}
