const paths = {
  code: 'm8 7-5 5 5 5m8-10 5 5-5 5m-3-14-2 18',
  arrow: 'M5 12h14m-6-6 6 6-6 6',
  dashboard: 'M3 3h7v7H3zm11 0h7v7h-7zM3 14h7v7H3zm11 0h7v7h-7z',
  sessions: 'M8 2v4m8-4v4M3 10h18M5 4h14a2 2 0 0 1 2 2v14H3V6a2 2 0 0 1 2-2',
  activities: 'm9 5 1-2h4l1 2h4v16H5V5zm0 6h6m-6 5h4',
  progress: 'M4 20V10m8 10V4m8 16v-7M2 20h20',
  profile: 'M16 7a4 4 0 1 1-8 0 4 4 0 0 1 8 0M4 21v-2a8 8 0 0 1 16 0v2',
  users: 'M10 7a3 3 0 1 1-6 0 3 3 0 0 1 6 0M2 21v-3a5 5 0 0 1 10 0v3M15 4a3 3 0 0 1 0 6m0 3a5 5 0 0 1 7 5v3',
  topics: 'M12 6C9 3 5 3 2 4v15c4-1 7-1 10 2 3-3 6-3 10-2V4c-3-1-7-1-10 2zm0 0v15',
  reports: 'M14 2H5v20h14V7zm0 0v5h5M8 12h8m-8 4h6',
  check: 'm5 12 4 4L19 6',
  spark: 'm12 2 3 7 7 3-7 3-3 7-3-7-7-3 7-3z',
  pause: 'M8 5v14m8-14v14',
  menu: 'M4 6h16M4 12h16M4 18h16',
  close: 'm6 6 12 12M6 18 18 6',
  logout: 'M9 4H4v16h5m5-13 5 5-5 5m-5-5h10',
  lock: 'M6 10h12v11H6zm2 0V6a4 4 0 0 1 8 0v4',
  mail: 'M3 5h18v14H3zm0 0 9 7 9-7',
}

export default function Icon({ name, className = '' }) {
  return (
    <svg className={`icon ${className}`} viewBox="0 0 24 24" fill="none" stroke="currentColor"
      strokeWidth="1.6" strokeLinecap="round" strokeLinejoin="round" aria-hidden="true" focusable="false">
      <path d={paths[name] ?? paths.code} />
    </svg>
  )
}
