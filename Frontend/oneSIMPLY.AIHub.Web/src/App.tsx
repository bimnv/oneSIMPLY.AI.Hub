import { Navigate, Route, Routes } from 'react-router-dom'
import Layout from './components/Layout'
import { isAuthenticated } from './services/authService'
import BillingPage from './pages/BillingPage'
import CopywritingPage from './pages/CopywritingPage'
import HistoryPage from './pages/HistoryPage'
import LoginPage from './pages/LoginPage'
import RegisterPage from './pages/RegisterPage'
import SocialChannelsPage from './pages/SocialChannelsPage'

function PrivateRoute({ children }: { children: React.ReactNode }) {
  return isAuthenticated() ? children : <Navigate to="/login" replace />
}

export default function App() {
  return (
    <Routes>
      <Route path="/login" element={<LoginPage />} />
      <Route path="/register" element={<RegisterPage />} />
      <Route
        path="/"
        element={
          <PrivateRoute>
            <Layout />
          </PrivateRoute>
        }
      >
        <Route index element={<Navigate to="/copywriting" replace />} />
        <Route path="copywriting" element={<CopywritingPage />} />
        <Route path="social" element={<SocialChannelsPage />} />
        <Route path="billing" element={<BillingPage />} />
        <Route path="history" element={<HistoryPage />} />
      </Route>
      <Route path="*" element={<Navigate to="/copywriting" replace />} />
    </Routes>
  )
}
