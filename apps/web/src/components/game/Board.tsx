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
      className="grid aspect-square w-full max-w-md gap-[2px] overflow-hidden rounded-2xl bg-black/10 p-[2px]"
      style={{
        gridTemplateColumns: `repeat(${columns}, minmax(0, 1fr))`,
        gridTemplateRows: `repeat(${rows}, minmax(0, 1fr))`,
      }}
    >
      {cells.map((coordinate) => {
        const key = coordinateKey(coordinate)
        const field = getField(game.gameState.grid, coordinate)
        const isEmpty = field.player === null
        // An exploded mine still carries the triggering player's mark in the data model (see
        // packages/game-engine's docs/game-rules.md), but it's a permanent, unowned obstacle -
        // render it as a crater, not as that player's stone.
        const isCrater = field.isMine && field.player !== null
        const flashKey = changedKeys.has(key) ? game.gameState.movesPlayed : 0

        return (
          <button
            key={key}
            type="button"
            role="gridcell"
            aria-label={`Row ${coordinate.row + 1}, column ${coordinate.col + 1}${
              isCrater
                ? ', exploded mine crater'
                : field.player
                  ? `, ${field.player}, ${field.surroundedByNotExplodedMines} mines nearby`
                  : ''
            }`}
            disabled={!isEmpty || game.gameState.isGameOver}
            onClick={() => onCellClick(coordinate)}
            className={cn(
              'flex items-center justify-center bg-white p-[8%]',
              'disabled:pointer-events-none',
              isEmpty && !game.gameState.isGameOver && 'hover:bg-brand/10 active:bg-brand/20',
            )}
          >
            {isCrater ? (
              <span
                key={flashKey}
                aria-hidden="true"
                className="h-full w-full rounded-full bg-crater shadow-[inset_0_2px_5px_rgba(0,0,0,0.6)] animate-[mark-pop_150ms_ease-out]"
              />
            ) : field.player ? (
              <span
                key={flashKey}
                className={cn(
                  'flex h-full w-full items-center justify-center rounded-md text-[min(3.2vw,1rem)] font-extrabold text-white animate-[mark-pop_150ms_ease-out]',
                  field.player === 'O' ? 'bg-player-o' : 'bg-player-x',
                )}
              >
                {field.surroundedByNotExplodedMines}
              </span>
            ) : (
              changedKeys.has(key) && (
                // A mine explosion just erased this cell's mark - flash it so the change
                // (which would otherwise look like nothing happened) is visible.
                <span
                  key={flashKey}
                  aria-hidden="true"
                  className="block h-full w-full rounded-md animate-[cell-flash_400ms_ease-out]"
                />
              )
            )}
          </button>
        )
      })}
    </div>
  )
}
