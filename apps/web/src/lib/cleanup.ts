const INVITE_LIFETIME_MS = 48 * 60 * 60 * 1000
const GAME_LIFETIME_MS = 30 * 24 * 60 * 60 * 1000
const IMMINENT_MS = 24 * 60 * 60 * 1000

export function getCleanupDeadline(row: {
  invited_user_id: string | null
  created_at: string
  updated_at: string
}): Date {
  if (row.invited_user_id === null) {
    return new Date(new Date(row.created_at).getTime() + INVITE_LIFETIME_MS)
  }
  return new Date(new Date(row.updated_at).getTime() + GAME_LIFETIME_MS)
}

export function isCleanupImminent(deadline: Date): boolean {
  const remaining = deadline.getTime() - Date.now()
  return remaining > 0 && remaining < IMMINENT_MS
}

export function formatDeadline(deadline: Date): string {
  return deadline.toLocaleString(undefined, {
    month: 'short',
    day: 'numeric',
    hour: 'numeric',
    minute: '2-digit',
  })
}
