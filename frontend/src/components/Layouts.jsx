import { useEffect, useRef, useState } from 'react'
import { Link, NavLink, Outlet, useLocation } from 'react-router'
import { useTranslation } from 'react-i18next'
import { useAuth } from '../features/auth/AuthContext.jsx'
import LanguageSelector from './LanguageSelector.jsx'
import Icon from './Icon.jsx'
import { MotionToggle } from './VisualEffects.jsx'

const studentLinks = ['dashboard', 'sessions', 'activities', 'progress', 'profile']
const adminLinks = ['users', 'topics', 'sessions', 'activities', 'progress', 'reports']

function Header({ admin }) {
  const { t } = useTranslation()
  const { user, logout } = useAuth()
  const [open, setOpen] = useState(false)
  const toggle = useRef(null)
  const base = admin ? '/admin' : '/app'
  const links = admin ? adminLinks : studentLinks

  useEffect(() => {
    if (!open) return
    function onKeyDown(event) {
      if (event.key === 'Escape') {
        setOpen(false)
        toggle.current?.focus()
      }
    }
    document.addEventListener('keydown', onKeyDown)
    return () => document.removeEventListener('keydown', onKeyDown)
  }, [open])

  function navigation() {
    return (
      <nav className="navigation-links" aria-label={t('nav.main')}>
        {links.map((name) => (
          <NavLink key={name} to={name === 'dashboard' ? base : `${base}/${name === 'profile' ? 'me' : name}`}
            end={name === 'dashboard'} onClick={() => setOpen(false)}
            className={({ isActive }) => `nav-link${isActive ? ' nav-link-active' : ''}`}>
            <Icon name={name} /><span>{t(`nav.${name}`)}</span>
          </NavLink>
        ))}
      </nav>
    )
  }

  function account() {
    return (
      <div className="account-actions">
        <MotionToggle />
        <LanguageSelector />
        <button type="button" onClick={logout} className="btn logout-button" aria-label={t('auth.logout')} title={t('auth.logout')}><Icon name="logout" /><span>{t('auth.logout')}</span></button>
      </div>
    )
  }

  return (
    <header className="app-header">
      <a className="skip-link btn btn-primary" href="#main-content">{t('nav.skipContent')}</a>
      <div className="container-page header-row">
        <Link to={admin ? '/admin/users' : '/app'} className="brand">
          <span className="brand-symbol"><Icon name="code" /></span>
          <span>{t('app.title')}<span className="brand-caption">{t(admin ? 'nav.admin' : 'visual.workshop')}</span></span>
        </Link>
        <div className="desktop-navigation">{navigation()}{account()}</div>
        <button ref={toggle} type="button" className="btn menu-toggle" aria-expanded={open}
          aria-controls="mobile-navigation" onClick={() => setOpen(!open)}>
          <Icon name={open ? 'close' : 'menu'} />{t(open ? 'nav.closeMenu' : 'nav.openMenu')}
        </button>
      </div>
      <div id="mobile-navigation" hidden={!open} className="container-page mobile-navigation">
        {navigation()}
        {user?.fullName && <p className="account-name mt-4">{user.fullName}</p>}
        {account()}
      </div>
    </header>
  )
}

function Layout({ admin = false }) {
  const location = useLocation()
  const { t } = useTranslation()
  const { user } = useAuth()
  return (
    <div className="app-shell">
      <Header key={location.pathname} admin={admin} />
      <main id="main-content" tabIndex={-1} className="container-page page-content">
        <div className="workspace-context">
          <p className="eyebrow"><span className="status-dot" />{t(admin ? 'visual.adminSpace' : 'visual.studentSpace')}</p>
          <span className="workspace-user"><Icon name="profile" /><span>{user?.fullName}</span></span>
        </div>
        <Outlet />
      </main>
    </div>
  )
}

export function AppLayout() { return <Layout /> }
export function AdminLayout() { return <Layout admin /> }
