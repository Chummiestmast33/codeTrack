import { createContext, useContext, useState } from 'react'
import { useTranslation } from 'react-i18next'
import Icon from './Icon.jsx'

const MotionContext = createContext(null)

export function VisualEffects({ children }) {
  const [paused, setPaused] = useState(() => {
    try { return localStorage.getItem('codetrack-motion') === 'paused' } catch { return false }
  })

  function toggle() {
    const next = !paused
    setPaused(next)
    try { localStorage.setItem('codetrack-motion', next ? 'paused' : 'enabled') } catch { /* Optional preference. */ }
  }

  return (
    <MotionContext.Provider value={{ paused, toggle }}>
      <div className="visual-environment" data-motion={paused ? 'paused' : 'enabled'}>
        <div className="ambient-scene" aria-hidden="true">
          <div className="ambient-light ambient-light-green" />
          <div className="ambient-light ambient-light-violet" />
          <div className="ambient-grid" />
        </div>
        {children}
      </div>
    </MotionContext.Provider>
  )
}

export function MotionToggle() {
  const { t } = useTranslation()
  const { paused, toggle } = useContext(MotionContext)
  return (
    <button type="button" className="btn icon-button motion-toggle" onClick={toggle}
      aria-pressed={paused} aria-label={t('visual.pauseMotion')} title={t('visual.pauseMotion')}>
      <Icon name={paused ? 'spark' : 'pause'} />
    </button>
  )
}
