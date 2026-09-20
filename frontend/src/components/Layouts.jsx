import { Link, NavLink, Outlet } from 'react-router'
import { useTranslation } from 'react-i18next'
import { useAuth } from '../features/auth/AuthContext.jsx'
import LanguageSelector from '../components/LanguageSelector.jsx'

function linkClass({ isActive }) {
  return `rounded-md px-3 py-2 text-sm font-medium ${
    isActive ? 'bg-indigo-100 text-indigo-700' : 'text-slate-600 hover:bg-slate-100'
  }`
}

export function AppLayout() {
  const { t } = useTranslation()
  const { user, logout } = useAuth()

  return (
    <div className="min-h-screen bg-slate-50 text-slate-900">
      <header className="border-b border-slate-200 bg-white">
        <div className="mx-auto flex max-w-5xl items-center justify-between gap-4 px-6 py-3">
          <Link to="/app" className="text-lg font-bold tracking-tight">
            {t('app.title')}
          </Link>
          <nav className="flex items-center gap-1">
            <NavLink to="/app" end className={linkClass}>
              {t('nav.dashboard')}
            </NavLink>
            <NavLink to="/app/sessions" className={linkClass}>
              {t('nav.sessions')}
            </NavLink>
            <NavLink to="/app/me" className={linkClass}>
              {t('nav.profile')}
            </NavLink>
          </nav>
          <div className="flex items-center gap-3">
            <span className="hidden text-sm text-slate-500 sm:inline">{user?.fullName}</span>
            <LanguageSelector />
            <button
              onClick={logout}
              className="rounded-md border border-slate-300 px-3 py-1.5 text-sm text-slate-700 hover:bg-slate-100"
            >
              {t('auth.logout')}
            </button>
          </div>
        </div>
      </header>
      <main className="mx-auto max-w-5xl px-6 py-8">
        <Outlet />
      </main>
    </div>
  )
}

export function AdminLayout() {
  const { t } = useTranslation()
  const { logout } = useAuth()

  return (
    <div className="min-h-screen bg-slate-50 text-slate-900">
      <header className="border-b border-slate-200 bg-slate-900 text-white">
        <div className="mx-auto flex max-w-6xl items-center justify-between gap-4 px-6 py-3">
          <Link to="/admin/users" className="text-lg font-bold tracking-tight">
            {t('app.title')} · {t('nav.admin')}
          </Link>
          <nav className="flex items-center gap-1">
            <NavLink
              to="/admin/users"
              className={({ isActive }) =>
                `rounded-md px-3 py-2 text-sm font-medium ${
                  isActive ? 'bg-slate-700 text-white' : 'text-slate-300 hover:bg-slate-800'
                }`
              }
            >
              {t('nav.users')}
            </NavLink>
            <NavLink
              to="/admin/topics"
              className={({ isActive }) =>
                `rounded-md px-3 py-2 text-sm font-medium ${
                  isActive ? 'bg-slate-700 text-white' : 'text-slate-300 hover:bg-slate-800'
                }`
              }
            >
              {t('nav.topics')}
            </NavLink>
            <NavLink
              to="/admin/sessions"
              className={({ isActive }) =>
                `rounded-md px-3 py-2 text-sm font-medium ${
                  isActive ? 'bg-slate-700 text-white' : 'text-slate-300 hover:bg-slate-800'
                }`
              }
            >
              {t('nav.sessions')}
            </NavLink>
          </nav>
          <div className="flex items-center gap-3">
            <LanguageSelector />
            <button
              onClick={logout}
              className="rounded-md border border-slate-600 px-3 py-1.5 text-sm text-slate-200 hover:bg-slate-800"
            >
              {t('auth.logout')}
            </button>
          </div>
        </div>
      </header>
      <main className="mx-auto max-w-6xl px-6 py-8">
        <Outlet />
      </main>
    </div>
  )
}
