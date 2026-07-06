import { coordinateKey, getField, type Coordinate, type Game } from '@mintactoe/game-engine'
import { cn } from '../../lib/cn'

export interface BoardProps {
  game: Game
  onCellClick: (coordinate: Coordinate) => void
}

export function Board({ game, onCellClick }: BoardProps) {
  const { rows, columns } = game.rules
  const changedKeys = new Set(game.gameState.changes.map(coordinateKey))
  const cells: Coordinate[] = []
  for (let row = 0; row < rows; row++) {
    for (let col = 0; col < columns; col++) {
      cells.push({ row, col })
    }
  }

  return (
    <div
      role="grid"
      aria-label="Game board"
      className="grid w-full max-w-md gap-[2px] rounded-2xl bg-black/10 p-[2px]"
      style={{ gridTemplateColumns: `repeat(${columns}, minmax(0, 1fr))` }}
    >
      {cells.map((coordinate) => {
        const key = coordinateKey(coordinate)
        const field = getField(game.gameState.grid, coordinate)
        const isEmpty = field.player === null
        const flashKey = changedKeys.has(key) ? game.gameState.movesPlayed : 0

        return (
          <button
            key={key}
            type="button"
            role="gridcell"
            aria-label={`Row ${coordinate.row + 1}, column ${coordinate.col + 1}${
              field.player ? `, ${field.player}` : ''
            }`}
            disabled={!isEmpty || game.gameState.isGameOver}
            onClick={() => onCellClick(coordinate)}
            className={cn(
              'flex aspect-square items-center justify-center bg-white text-[min(4vw,1.1rem)] font-bold',
              'disabled:pointer-events-none',
              isEmpty && !game.gameState.isGameOver && 'hover:bg-brand/10 active:bg-brand/20',
              field.player === 'O' && 'text-player-o',
              field.player === 'X' && 'text-player-x',
            )}
          >
            {field.player ? (
              <span key={flashKey} className="inline-block animate-[mark-pop_150ms_ease-out]">
                {field.player}
              </span>
            ) : (
              changedKeys.has(key) && (
                // A mine explosion just erased this cell's mark - flash it so the change
                // (which would otherwise look like nothing happened) is visible.
                <span
                  key={flashKey}
                  aria-hidden="true"
                  className="block h-2/3 w-2/3 rounded-full animate-[cell-flash_400ms_ease-out]"
                />
              )
            )}
          </button>
        )
      })}
    </div>
  )
}
