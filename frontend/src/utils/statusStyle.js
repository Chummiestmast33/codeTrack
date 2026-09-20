export function statusStyle(status) {
  switch (status) {
    case 'Completed':
      return 'bg-green-100 text-green-800'
    case 'InProgress':
      return 'bg-amber-100 text-amber-800'
    default:
      return 'bg-slate-200 text-slate-600'
  }
}
