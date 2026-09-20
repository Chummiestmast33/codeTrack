// Deployment-time display values (never hardcode them in JSX/locales).
// Set via frontend/.env (VITE_*) — see frontend/.env.example.
export const PERIOD = (import.meta.env.VITE_PERIOD ?? '').trim()
