import { NavLink, Outlet, useNavigate } from 'react-router-dom'
import { CreditCard, History, Link2, LogOut, PenLine, Sparkles } from 'lucide-react'
import { logout } from '../services/authService'

const navItems = [
  { to: '/copywriting', label: 'Viết bài bán hàng', icon: PenLine },
  { to: '/social', label: 'Kết nối kênh đăng', icon: Link2 },
  { to: '/billing', label: 'Gói cước & Nạp tiền', icon: CreditCard },
  { to: '/history', label: 'Lịch sử sử dụng', icon: History },
]

export default function Layout() {
  const navigate = useNavigate()
  const email = localStorage.getItem('aihub_email') || 'User'

  const handleLogout = () => {
    logout()
    navigate('/login')
  }

  return (
    <div className="min-h-screen flex bg-slate-50">
      <aside className="w-64 bg-white border-r border-slate-200 flex flex-col shrink-0">
        <div className="p-5 border-b border-slate-100">
          <div className="flex items-center gap-3">
            <div className="w-10 h-10 rounded-xl bg-gradient-to-tr from-brand-blue to-brand-orange flex items-center justify-center text-white font-black text-sm shadow">
              1S
            </div>
            <div>
              <h1 className="font-extrabold text-brand-blue leading-tight">
                oneSIMPLY <span className="text-brand-orange">AI Hub</span>
              </h1>
              <p className="text-[10px] uppercase tracking-wider text-slate-400">Growth AI Marketing</p>
            </div>
          </div>
        </div>

        <nav className="flex-1 p-4 space-y-1">
          {navItems.map(({ to, label, icon: Icon }) => (
            <NavLink
              key={to}
              to={to}
              className={({ isActive }) =>
                `flex items-center gap-3 px-3 py-2.5 rounded-lg text-sm font-medium transition-colors ${
                  isActive
                    ? 'bg-brand-blue/10 text-brand-blue'
                    : 'text-slate-600 hover:bg-slate-50 hover:text-slate-900'
                }`
              }
            >
              <Icon size={18} />
              {label}
            </NavLink>
          ))}
        </nav>

        <div className="p-4 border-t border-slate-100">
          <div className="flex items-center gap-2 mb-3 px-2">
            <Sparkles size={16} className="text-brand-orange" />
            <span className="text-xs text-slate-500 truncate">{email}</span>
          </div>
          <button
            type="button"
            onClick={handleLogout}
            className="w-full flex items-center justify-center gap-2 px-3 py-2 text-sm text-slate-600 hover:text-rose-600 hover:bg-rose-50 rounded-lg transition-colors"
          >
            <LogOut size={16} />
            Đăng xuất
          </button>
        </div>
      </aside>

      <main className="flex-1 overflow-auto">
        <Outlet />
      </main>
    </div>
  )
}
