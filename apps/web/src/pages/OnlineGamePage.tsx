import { useCallback, useState } from 'react'
import { Link, useNavigate, useParams } from 'react-router'
import type { Coordinate, Player } from '@mintactoe/game-engine'
import { useAuth } from '../hooks/useAuth'
import { useOnlineGame, type OnlineRole } from '../hooks/useOnlineGame'
import { forfeitGame, ApiError } from '../lib/api'
import { getCleanupDeadline, isCleanupImminent, formatDeadline } from '../lib/cleanup'
import { Board } from '../components/game/Board'
import { BoardLegend } from '../components/game/BoardLegend'
import { Button } from '../components/ui/Button'
import { CopyButton } from '../components/ui/CopyButton'
import { Card } from '../components/ui/Card'
import { isInAppBrowser } from '../lib/inAppBrowser'
import { cn } from '../lib/cn'

function roleToPlayer(role: OnlineRole): Player | null {
  if (role === 'host') return 'O'
  if (role === 'visitor') return 'X'
  return null
}

function Spinner() {
  return (
    <span
      className="inline-block h-5 w-5 animate-spin rounded-full border-2 border-brand border-t-transparent"
      role="status"
      aria-label="Loading"
    />
  )
}

function Dot({ player }: { player: Player }) {
  return (
    <span
      className={cn(
        'inline-flex h-5 w-5 shrink-0 items-center justify-center rounded-full',
        player === 'O' ? 'bg-player-o' : 'bg-player-x',
      )}
    >
      <span className="sr-only">{player}</span>
    </span>
  )
}

function PreJoinGate({ onContinue }: { onContinue: () => void }) {
  const inApp = isInAppBrowser()
  const url = window.location.href
  const [showCopied, setShowCopied] = useState(false)

  const copyAndHint = async () => {
    await navigator.clipboard.writeText(url)
    setShowCopied(true)
  }

  return (
    <Card className="flex w-full flex-col gap-4 text-center">
      <h2 className="text-lg font-bold">You've been invited to a game!</h2>
      {inApp ? (
        <>
          <p className="text-sm text-black/60">
            For the best experience, open this link in your browser instead of this in-app
            viewer.
          </p>
          <Button onClick={copyAndHint}>
            {showCopied ? 'Link copied — paste in your browser' : 'Copy link & open in browser'}
          </Button>
          <Button variant="secondary" onClick={onContinue}>
            Continue here
          </Button>
        </>
      ) : (
        <>
          <p className="text-sm text-black/60">Tap below to join the game.</p>
          <Button onClick={onContinue}>Join game</Button>
        </>
      )}
    </Card>
  )
}

function shortCode(gameId: string) {
  return gameId.slice(0, 8)
}

function ExpiryNotice({ gameRow }: { gameRow: { invited_user_id: string | null; created_at: string; updated_at: string } }) {
  const deadline = getCleanupDeadline(gameRow)
  if (!isCleanupImminent(deadline)) return null
  return (
    <p className="rounded-xl bg-amber-50 px-4 py-2 text-center text-xs font-medium text-amber-700">
      This game expires {formatDeadline(deadline)}
    </p>
  )
}

function WaitingForOpponent({ gameId, gameRow }: { gameId: string; gameRow: { invited_user_id: string | null; created_at: string; updated_at: string } }) {
  const code = shortCode(gameId)
  const gameUrl = `${window.location.origin}/game/${code}`

  return (
    <Card className="flex w-full flex-col gap-4 text-center">
      <div className="flex flex-col items-center gap-2">
        <Spinner />
        <h2 className="text-lg font-bold">Waiting for opponent…</h2>
      </div>
      <ExpiryNotice gameRow={gameRow} />

      <div className="flex flex-col gap-3">
        <p className="text-sm font-semibold">Share the link</p>
        <div className="rounded-2xl bg-black/5 px-4 py-3">
          <p className="break-all font-mono text-xs">{gameUrl}</p>
        </div>
        <CopyButton text={gameUrl} label="Copy link" variant="secondary" />
      </div>

      <div className="border-t border-black/10 pt-3">
        <p className="text-sm font-semibold">Or share the game code</p>
        <div className="mt-2 rounded-2xl bg-black/5 px-4 py-3">
          <p className="font-mono text-lg font-bold tracking-widest select-all">{code}</p>
        </div>
        <div className="mt-2">
          <CopyButton text={code} label="Copy code" variant="secondary" />
        </div>
      </div>
    </Card>
  )
}

function OnlineGameInfo({
  myRole,
  isGameOver,
  winner,
  playerOnTurn,
  movesPlayed,
  onForfeit,
  forfeiting,
  onBackToMenu,
}: {
  myRole: OnlineRole
  isGameOver: boolean
  winner: Player | null
  playerOnTurn: Player | null
  movesPlayed: number
  onForfeit?: () => void
  forfeiting?: boolean
  onBackToMenu?: () => void
}) {
  const myPlayer = roleToPlayer(myRole)
  const [confirmForfeit, setConfirmForfeit] = useState(false)

  const showForfeit = onForfeit && myPlayer !== null && !isGameOver

  const handleForfeitClick = () => {
    if (!confirmForfeit) {
      setConfirmForfeit(true)
      return
    }
    onForfeit?.()
    setConfirmForfeit(false)
  }

  return (
    <div className="flex w-full max-w-md items-center justify-between gap-3 rounded-3xl bg-white p-4 shadow-sm">
      <p className="flex items-center gap-2 font-semibold">
        {isGameOver ? (
          winner ? (
            winner === myPlayer ? (
              <>
                <Dot player={winner} /> You win!
              </>
            ) : myPlayer ? (
              <>
                <Dot player={winner} /> You lose
              </>
            ) : (
              <>
                <Dot player={winner} /> wins!
              </>
            )
          ) : (
            "It's a tie!"
          )
        ) : playerOnTurn ? (
          myRole === 'spectator' ? (
            <>
              Turn: <Dot player={playerOnTurn} />
            </>
          ) : playerOnTurn === myPlayer ? (
            <>
              <Dot player={playerOnTurn} /> Your turn
            </>
          ) : (
            <>
              <Dot player={playerOnTurn} /> Opponent's turn
            </>
          )
        ) : null}
      </p>
      <p className="text-sm text-black/60">Moves: {movesPlayed}</p>
      {showForfeit && (
        <Button
          variant="ghost"
          onClick={handleForfeitClick}
          onBlur={() => setConfirmForfeit(false)}
          disabled={forfeiting}
        >
          {forfeiting ? 'Forfeiting…' : confirmForfeit ? 'Are you sure?' : 'Forfeit'}
        </Button>
      )}
      {isGameOver && onBackToMenu && (
        <Button variant="ghost" onClick={onBackToMenu}>
          Back to menu
        </Button>
      )}
    </div>
  )
}

function GameView({ gameId }: { gameId: string }) {
  const { session, loading: authLoading } = useAuth()

  if (authLoading || !session) {
    return (
      <div className="flex items-center gap-2 text-sm text-black/60">
        <Spinner /> Signing in…
      </div>
    )
  }

  return <GameViewWithSession gameId={gameId} session={session} />
}

function GameViewWithSession({
  gameId,
  session,
}: {
  gameId: string
  session: NonNullable<ReturnType<typeof useAuth>['session']>
}) {
  const { game, gameRow, myRole, move, loading, error, needsJoin, join } = useOnlineGame(
    gameId,
    session,
  )
  const [moveError, setMoveError] = useState<string | null>(null)
  const [forfeiting, setForfeiting] = useState(false)
  const navigate = useNavigate()

  const handleForfeit = useCallback(async () => {
    if (!gameRow) return
    setForfeiting(true)
    try {
      await forfeitGame(session.access_token, gameRow.id)
    } catch (err) {
      setMoveError(err instanceof ApiError ? err.message : 'Failed to forfeit.')
      setForfeiting(false)
    }
  }, [gameRow, session.access_token])

  if (loading) {
    return (
      <div className="flex items-center gap-2 text-sm text-black/60">
        <Spinner /> Loading game…
      </div>
    )
  }

  if (error || !game || !gameRow || !myRole) {
    return (
      <Card className="w-full text-center">
        <p className="font-semibold text-red-600">{error ?? 'Game not found.'}</p>
        <Link to="/" className="mt-3 inline-block text-sm font-semibold text-brand">
          ← Back to menu
        </Link>
      </Card>
    )
  }

  if (needsJoin) {
    return <PreJoinGate onContinue={join} />
  }

  const isWaiting = myRole === 'host' && gameRow.invited_user_id === null

  if (isWaiting) {
    return <WaitingForOpponent gameId={gameId} gameRow={gameRow} />
  }

  const myPlayer = roleToPlayer(myRole)
  const canMove =
    myPlayer !== null &&
    !game.gameState.isGameOver &&
    game.gameState.playerOnTurn === myPlayer

  const handleCellClick = async (coordinate: Coordinate) => {
    if (!canMove) return
    setMoveError(null)
    try {
      await move(coordinate)
    } catch (err) {
      if (err instanceof Error) {
        setMoveError(err.message)
      }
    }
  }

  return (
    <>
      <OnlineGameInfo
        myRole={myRole}
        isGameOver={game.gameState.isGameOver}
        winner={game.gameState.winner}
        playerOnTurn={game.gameState.playerOnTurn}
        movesPlayed={game.gameState.movesPlayed}
        onForfeit={handleForfeit}
        forfeiting={forfeiting}
        onBackToMenu={() => navigate('/')}
      />
      {myRole === 'spectator' && (
        <p className="text-sm text-black/60">You are spectating this game.</p>
      )}
      <Board game={game} onCellClick={handleCellClick} />
      {moveError && <p className="text-sm text-red-600">{moveError}</p>}
      <ExpiryNotice gameRow={gameRow} />
      <BoardLegend />
    </>
  )
}

export function OnlineGamePage() {
  const { gameId } = useParams<{ gameId: string }>()

  if (!gameId) {
    return (
      <main className="mx-auto flex max-w-md flex-col items-center gap-4 px-4 pt-16">
        <p className="font-semibold text-red-600">No game ID in URL.</p>
        <Link to="/" className="text-sm font-semibold text-brand">
          ← Back to menu
        </Link>
      </main>
    )
  }

  return (
    <main className="mx-auto flex max-w-md flex-col items-center gap-4 px-4 py-6">
      <div className="flex w-full items-center justify-between">
        <Link to="/" className="text-sm font-semibold text-brand">
          ← Home
        </Link>
        <h1 className="text-lg font-extrabold">Online game</h1>
        <div className="w-12" />
      </div>

      <GameView gameId={gameId} />
    </main>
  )
}
