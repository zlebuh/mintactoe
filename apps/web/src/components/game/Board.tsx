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
        // Once the game ends, reveal every mine nobody ever triggered - generated (isMine can
        // only ever be true on a generated field) and never played on.
        const isRevealedMine = game.gameState.isGameOver && field.generated && field.isMine && field.player === null
        // Only true for the one render right after this exact cell was affected by a move - an
        // occupied cell can never re-enter `changes` on a later move (only the just-placed
        // coordinate or a newly-erased cell can), so the animation classes below are added once
        // and never re-added, rather than being retriggered by a `key`-based remount.
        const justChanged = changedKeys.has(key)
        // The container clips to rounded-2xl via overflow-hidden; without matching rounding on
        // the corner cells themselves, that clip cuts a curved bite out of their sharp corners,
        // exposing the page background underneath in a crescent shape at each of the 4 corners.
        const isTop = coordinate.row === 0
        const isBottom = coordinate.row === rows - 1
        const isLeft = coordinate.col === 0
        const isRight = coordinate.col === columns - 1

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
                  : isRevealedMine
                    ? ', hidden mine'
                    : ''
            }`}
            disabled={!isEmpty || game.gameState.isGameOver}
            onClick={() => onCellClick(coordinate)}
            className={cn(
              'flex items-center justify-center p-[8%]',
              'disabled:pointer-events-none',
              // Lingers on the fields from the last move (not just the brief pop/flash above) -
              // naturally clears itself once the *next* move updates game.gameState.changes.
              justChanged ? 'bg-mine-flash/15' : 'bg-white',
              isEmpty && !game.gameState.isGameOver && 'hover:bg-brand/10 active:bg-brand/20',
              isTop && isLeft && 'rounded-tl-2xl',
              isTop && isRight && 'rounded-tr-2xl',
              isBottom && isLeft && 'rounded-bl-2xl',
              isBottom && isRight && 'rounded-br-2xl',
            )}
          >
            {isCrater ? (
              <span
                aria-hidden="true"
                className={cn(
                  'h-full w-full rounded-full bg-crater shadow-[inset_0_2px_5px_rgba(0,0,0,0.6)]',
                  justChanged && 'animate-[mark-pop_150ms_ease-out]',
                )}
              />
            ) : field.player ? (
              <span
                className={cn(
                  'flex h-full w-full items-center justify-center rounded-md text-center text-[min(3.2vw,1rem)] leading-none font-bold text-white',
                  field.player === 'O' ? 'bg-player-o' : 'bg-player-x',
                  justChanged && 'animate-[mark-pop_150ms_ease-out]',
                )}
              >
                {field.surroundedByNotExplodedMines}
              </span>
            ) : isRevealedMine ? (
              // The game is over - show where every mine nobody triggered actually was.
              <span
                aria-hidden="true"
                className="flex h-full w-full items-center justify-center rounded-full bg-mine-flash/25 animate-[mark-pop_150ms_ease-out]"
              >
                <span className="h-[45%] w-[45%] rounded-full bg-crater" />
              </span>
            ) : (
              justChanged && (
                // A mine explosion just erased this cell's mark - flash it so the change
                // (which would otherwise look like nothing happened) is visible. This element
                // only ever exists for the one render where it just got erased (mounts fresh
                // each time, per the conditional above), so no key trick is needed here.
                <span
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
