import type { Game } from '@mintactoe/game-engine'
import { Button } from '../ui/Button'
import { cn } from '../../lib/cn'

export interface GameInfoProps {
  game: Game
  onReset: () => void
}

export function GameInfo({ game, onReset }: GameInfoProps) {
  const { isGameOver, winner, playerOnTurn, movesPlayed } = game.gameState

  if (isGameOver) {
    return (
      <div className="flex flex-col items-center gap-3 rounded-3xl bg-white p-5 text-center shadow-sm animate-[banner-in_200ms_ease-out]">
        <p className="text-xl font-extrabold">
          {winner ? (
            <>
              <span className={winner === 'O' ? 'text-player-o' : 'text-player-x'}>{winner}</span>{' '}
              wins!
            </>
          ) : (
            "It's a tie!"
          )}
        </p>
        <Button onClick={onReset}>New game</Button>
      </div>
    )
  }

  return (
    <div className="flex items-center justify-between gap-3 rounded-3xl bg-white p-4 shadow-sm">
      <p className="font-semibold">
        Turn:{' '}
        <span
          className={cn(
            'inline-flex h-7 w-7 items-center justify-center rounded-full font-bold text-white',
            playerOnTurn === 'O' ? 'bg-player-o' : 'bg-player-x',
          )}
        >
          {playerOnTurn}
        </span>
      </p>
      <p className="text-sm text-black/60">Moves: {movesPlayed}</p>
      <Button variant="ghost" onClick={onReset}>
        Restart
      </Button>
    </div>
  )
}
