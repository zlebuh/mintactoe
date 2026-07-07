import type { Player } from '@mintactoe/game-engine'
import { cn } from '../../lib/cn'

function NumberSwatch({ player, value }: { player: Player; value: number }) {
  return (
    <span
      aria-hidden="true"
      className={cn(
        'flex h-6 w-6 items-center justify-center rounded-md text-xs font-bold text-white',
        player === 'O' ? 'bg-player-o' : 'bg-player-x',
      )}
    >
      {value}
    </span>
  )
}

export function BoardLegend() {
  return (
    <div className="w-full max-w-md text-center text-sm text-black/60">
      <p className="mb-2 font-semibold text-black/70">On the board</p>
      <div className="flex flex-col items-center gap-1">
        <div className="flex items-center gap-2">
          <NumberSwatch player="O" value={0} />
          <NumberSwatch player="X" value={2} />
          <span>Players' marks</span>
        </div>
        <div className="flex items-center gap-2">
          <span aria-hidden="true" className="flex h-6 w-6 items-center justify-center text-black/40">
            ↑
          </span>
          <span aria-hidden="true" className="flex h-6 w-6 items-center justify-center text-black/40">
            ↑
          </span>
          <span>= count of live mines nearby</span>
        </div>
        <div className="mt-2 flex items-center gap-1.5">
          <span aria-hidden="true" className="inline-block h-3 w-3 rounded-full bg-crater" />
          Exploded mine
        </div>
      </div>
    </div>
  )
}
