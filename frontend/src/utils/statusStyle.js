export function statusStyle(status) {
  switch (status) {
    case 'Completed':
      return 'bg-success-soft text-success'
    case 'InProgress':
      return 'bg-warning-soft text-warning'
    default:
      return 'bg-raised text-muted'
  }
}
