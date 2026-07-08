import { useCallback, useEffect, useState } from 'react'
import { Link, useNavigate } from 'react-router'
import { CardButton, CardDescription, CardTitle } from '../components/ui/Card'
import { Badge } from '../components/ui/Badge'
import { Button } from '../components/ui/Button'
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogTitle,
  DialogTrigger,
} from '../components/ui/Dialog'
import { useAuth } from '../hooks/useAuth'
import { createGame, joinGame, forfeitGame, ApiError, type GameRow } from '../lib/api'
import { supabase } from '../lib/supabase'
import { cn } from '../lib/cn'

function Spinner() {
  return (
    <span
      className="inline-block h-4 w-4 animate-spin rounded-full border-2 border-current border-t-transparent"
      aria-hidden
    />
  )
}

function isUnfinished(row: GameRow): boolean {
  return row.game_state.gameState.isGameOver === false
}

function useActiveGame(userId: string | undefined) {
  const [activeGame, setActiveGame] = useState<GameRow | null>(null)
  const [history, setHistory] = useState<GameRow[]>([])
  const [checking, setChecking] = useState(true)

  const refresh = useCallback(async () => {
    if (!userId) return
    setChecking(true)
    const { data } = await supabase
      .from('games')
      .select('*')
      .or(`host_user_id.eq.${userId},invited_user_id.eq.${userId}`)
      .order('created_at', { ascending: false })
      .limit(10)

    const rows = (data as GameRow[] | null) ?? []
    setActiveGame(rows.find(isUnfinished) ?? null)
    setHistory(rows)
    setChecking(false)
  }, [userId])

  useEffect(() => { refresh() }, [refresh])

  return { activeGame, history, checking, refresh }
}

function OnlinePlayDialog() {
  const navigate = useNavigate()
  const { session } = useAuth()
  const userId = session?.user.id
  const token = session?.access_token
  const { activeGame, checking, refresh } = useActiveGame(userId)

  const [mode, setMode] = useState<'menu' | 'join'>('menu')
  const [joinCode, setJoinCode] = useState('')
  const [busy, setBusy] = useState(false)
  const [error, setError] = useState<string | null>(null)

  const handleCreate = async () => {
    if (!token) return
    setBusy(true)
    setError(null)
    try {
      const game = await createGame(token)
      navigate(`/game/${game.id.slice(0, 8)}`)
    } catch (err) {
      setError(err instanceof ApiError ? err.message : 'Failed to create game.')
      setBusy(false)
    }
  }

  const handleJoin = async () => {
    const code = joinCode.trim()
    if (!token || !code) return
    setBusy(true)
    setError(null)
    try {
      const game = await joinGame(token, code)
      navigate(`/game/${game.id.slice(0, 8)}`)
    } catch (err) {
      setError(err instanceof ApiError ? err.message : 'Failed to join game.')
      setBusy(false)
    }
  }

  const handleForfeit = async () => {
    if (!token || !activeGame) return
    setBusy(true)
    setError(null)
    try {
      await forfeitGame(token, activeGame.id)
      await refresh()
    } catch (err) {
      setError(err instanceof ApiError ? err.message : 'Failed to forfeit game.')
    }
    setBusy(false)
  }

  const signingIn = !token
  const loading = signingIn || checking

  if (activeGame) {
    const isHost = activeGame.host_user_id === userId
    const hasOpponent = activeGame.invited_user_id !== null
    return (
      <DialogContent>
        <DialogTitle>Play online</DialogTitle>
        <DialogDescription>
          You already have a game in progress{isHost && !hasOpponent ? ' (waiting for opponent)' : ''}.
        </DialogDescription>
        <div className="mt-5 flex flex-col gap-2">
          <Button onClick={() => navigate(`/game/${activeGame.id.slice(0, 8)}`)}>
            Resume game
          </Button>
          <Button variant="ghost" onClick={handleForfeit} disabled={busy} className="text-black/40">
            {busy ? 'Forfeiting…' : 'Forfeit & start fresh'}
          </Button>
          {error && <p className="text-sm text-red-600">{error}</p>}
        </div>
      </DialogContent>
    )
  }

  return (
    <DialogContent>
      <DialogTitle>Play online</DialogTitle>
      {loading ? (
        <>
          <DialogDescription>
            <span className="flex items-center gap-2">
              <Spinner />
              Connecting to server…
            </span>
          </DialogDescription>
          <div className="mt-5 flex flex-col gap-2">
            <Button disabled>Create game</Button>
            <Button variant="secondary" disabled>Join by code</Button>
          </div>
        </>
      ) : mode === 'menu' ? (
        <>
          <DialogDescription>Create a new game or join an existing one.</DialogDescription>
          <div className="mt-5 flex flex-col gap-2">
            <Button onClick={handleCreate} disabled={busy}>
              {busy ? 'Creating…' : 'Create game'}
            </Button>
            <Button variant="secondary" onClick={() => setMode('join')} disabled={busy}>
              Join by code
            </Button>
            {error && <p className="text-sm text-red-600">{error}</p>}
          </div>
        </>
      ) : (
        <>
          <DialogDescription>Enter the game code from your friend's invite link.</DialogDescription>
          <div className="mt-5 flex flex-col gap-2">
            <input
              type="text"
              value={joinCode}
              onChange={(e) => setJoinCode(e.target.value)}
              placeholder="Paste game code…"
              className="rounded-2xl border-2 border-black/10 px-4 py-3 text-sm outline-none focus:border-brand"
              autoFocus
              onKeyDown={(e) => {
                if (e.key === 'Enter') handleJoin()
              }}
            />
            <Button onClick={handleJoin} disabled={busy || !joinCode.trim()}>
              {busy ? 'Joining…' : 'Join game'}
            </Button>
            <Button variant="ghost" onClick={() => { setMode('menu'); setError(null) }} disabled={busy}>
              ← Back
            </Button>
            {error && <p className="text-sm text-red-600">{error}</p>}
          </div>
        </>
      )}
    </DialogContent>
  )
}

type GameStatus = 'win' | 'loss' | 'tie' | 'active' | 'waiting' | 'cancelled'

function deriveGameStatus(row: GameRow, userId: string): GameStatus {
  const myPlayer = row.host_user_id === userId ? 'O' : 'X'
  const { isGameOver, winner } = row.game_state.gameState

  if (!isGameOver) {
    if (row.invited_user_id === null) return 'waiting'
    return 'active'
  }
  if (row.invited_user_id === null) return 'cancelled'
  if (winner === null) return 'tie'
  return winner === myPlayer ? 'win' : 'loss'
}

const statusLabel: Record<GameStatus, string> = {
  win: 'Win',
  loss: 'Loss',
  tie: 'Tie',
  active: 'In progress',
  waiting: 'Waiting',
  cancelled: 'Cancelled',
}

const statusClass: Record<GameStatus, string> = {
  win: 'text-emerald-700 bg-emerald-50',
  loss: 'text-red-700 bg-red-50',
  tie: 'text-amber-700 bg-amber-50',
  active: 'text-blue-700 bg-blue-50',
  waiting: 'text-black/50 bg-black/5',
  cancelled: 'text-black/40 bg-black/5',
}

function formatDate(iso: string): string {
  const d = new Date(iso)
  const now = new Date()
  const diff = now.getTime() - d.getTime()
  const mins = Math.floor(diff / 60000)
  if (mins < 1) return 'Just now'
  if (mins < 60) return `${mins}m ago`
  const hours = Math.floor(mins / 60)
  if (hours < 24) return `${hours}h ago`
  const days = Math.floor(hours / 24)
  if (days < 7) return `${days}d ago`
  return d.toLocaleDateString(undefined, { month: 'short', day: 'numeric' })
}

function GameHistory({ games, userId }: { games: GameRow[]; userId: string }) {
  if (games.length === 0) return null

  return (
    <div className="mt-6 w-full">
      <h2 className="mb-3 text-sm font-bold uppercase tracking-wide text-black/40">
        Recent games
      </h2>
      <div className="overflow-hidden rounded-2xl bg-white shadow-sm">
        {games.map((row) => {
          const status = deriveGameStatus(row, userId)
          const code = row.id.slice(0, 8)
          const moves = row.game_state.gameState.movesPlayed
          return (
            <Link
              key={row.id}
              to={`/game/${code}`}
              className="flex items-center gap-3 border-b border-black/5 px-4 py-3 last:border-b-0 hover:bg-black/[0.02]"
            >
              <span
                className={cn(
                  'inline-flex min-w-[5.5rem] justify-center rounded-full px-2.5 py-0.5 text-xs font-semibold',
                  statusClass[status],
                )}
              >
                {statusLabel[status]}
              </span>
              <span className="flex-1 font-mono text-xs text-black/40">{code}</span>
              <span className="text-xs text-black/40">{moves} moves</span>
              <span className="text-xs text-black/30">{formatDate(row.created_at)}</span>
            </Link>
          )
        })}
      </div>
    </div>
  )
}

export function HomeMenu() {
  const navigate = useNavigate()
  const { session } = useAuth()
  const userId = session?.user.id
  const { history } = useActiveGame(userId)

  return (
    <main className="mx-auto flex max-w-md flex-col gap-4 px-4 pt-16 pb-10">
      <h1 className="mb-6 text-center text-4xl font-extrabold text-brand">MinTacToe</h1>

      <CardButton onClick={() => navigate('/local')}>
        <CardTitle>Play locally</CardTitle>
        <CardDescription>2 players, same device</CardDescription>
      </CardButton>

      <Dialog>
        <DialogTrigger asChild>
          <CardButton>
            <CardTitle>Play online with a friend</CardTitle>
            <CardDescription>Create a game and send the link, or join one</CardDescription>
          </CardButton>
        </DialogTrigger>
        <OnlinePlayDialog />
      </Dialog>

      <CardButton disabled>
        <div className="flex items-center justify-between gap-2">
          <CardTitle>Play vs bot</CardTitle>
          <Badge>Coming soon</Badge>
        </div>
        <CardDescription>Practice against the computer</CardDescription>
      </CardButton>

      {userId && <GameHistory games={history} userId={userId} />}
    </main>
  )
}
