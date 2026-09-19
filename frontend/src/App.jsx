import { AuthProvider } from './features/auth/AuthContext.jsx'
import AppRouter from './routes/router.jsx'

export default function App() {
  return (
    <AuthProvider>
      <AppRouter />
    </AuthProvider>
  )
}
