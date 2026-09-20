import { AuthProvider } from './features/auth/AuthContext.jsx'
import AppRouter from './routes/router.jsx'
import { VisualEffects } from './components/VisualEffects.jsx'

export default function App() {
  return (
    <AuthProvider>
      <VisualEffects><AppRouter /></VisualEffects>
    </AuthProvider>
  )
}
