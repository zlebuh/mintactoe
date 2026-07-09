import type { Player } from '@mintactoe/game-engine'
import { cn } from '../../lib/cn'

function NumberSwatch({ player, value }: { player: Player; value: number }) {
  return (
    <span
      aria-hidden="true"
      className={cn(
        'flex h-4 w-4 items-center justify-center rounded text-[0.6rem] font-bold text-white',
        player === 'O' ? 'bg-player-o' : 'bg-player-x',
      )}
    >
      {value}
    </span>
  )
}

export function BoardLegend() {
  return (
    <div className="w-full max-w-md text-center text-xs text-black/60">
      <p className="mb-1.5 font-semibold text-black/70">On the board</p>
      <div className="flex flex-col items-center gap-1.5">
        <div className="flex items-center gap-1.5">
          <NumberSwatch player="O" value={0} />
          <NumberSwatch player="X" value={2} />
          <span>Players' marks with a number of live mines nearby</span>
        </div>
        <div className="flex items-center gap-1">
          <span aria-hidden="true" className="inline-block h-2.5 w-2.5 rounded-full bg-crater" />
          Exploded mine
        </div>
      </div>
    </div>
  )
}
