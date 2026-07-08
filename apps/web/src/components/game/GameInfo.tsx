import type { Game, Player } from '@mintactoe/game-engine'
import { Button } from '../ui/Button'
import { cn } from '../../lib/cn'

export interface GameInfoProps {
  game: Game
  onReset: () => void
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

export function GameInfo({ game, onReset }: GameInfoProps) {
  const { isGameOver, winner, playerOnTurn, movesPlayed } = game.gameState

  return (
    <div className="flex w-full max-w-md items-center justify-between gap-3 rounded-3xl bg-white p-4 shadow-sm">
      <p className="flex items-center gap-2 font-semibold">
        {isGameOver ? (
          winner ? (
            <>
              <Dot player={winner} /> wins!
            </>
          ) : (
            "It's a tie!"
          )
        ) : (
          playerOnTurn && (
            <>
              Turn: <Dot player={playerOnTurn} />
            </>
          )
        )}
      </p>
      <p className="text-sm text-black/60">Moves: {movesPlayed}</p>
      <Button variant="ghost" onClick={onReset}>
        {isGameOver ? 'New game' : 'Restart'}
      </Button>
    </div>
  )
}
