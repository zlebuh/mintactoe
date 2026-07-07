import { cn } from '../lib/cn'
import { Dialog, DialogContent, DialogDescription, DialogTitle, DialogTrigger } from './ui/Dialog'

function LegendItem({ swatchClassName, label }: { swatchClassName: string; label: string }) {
  return (
    <span className="inline-flex items-center gap-1.5">
      <span aria-hidden="true" className={cn('inline-block h-3 w-3 rounded-full', swatchClassName)} />
      {label}
    </span>
  )
}

export function Footer() {
  return (
    <footer className="mx-auto w-full max-w-md px-4 py-8 text-center text-sm text-black/60">
      <p className="mb-3 font-semibold text-black/70">On the board</p>
      <div className="mb-6 flex flex-wrap items-center justify-center gap-x-4 gap-y-2">
        <LegendItem swatchClassName="bg-player-o" label="O's mark" />
        <LegendItem swatchClassName="bg-player-x" label="X's mark" />
        <LegendItem swatchClassName="bg-crater" label="Exploded mine" />
        <span>Number = live mines nearby</span>
      </div>

      <Dialog>
        <DialogTrigger asChild>
          <button type="button" className="font-semibold text-brand hover:underline">
            Game rules
          </button>
        </DialogTrigger>
        <DialogContent>
          <DialogTitle>How to play</DialogTitle>
          <DialogDescription>The short version - open this anytime while playing.</DialogDescription>
          <ul className="mt-3 list-disc space-y-2 pl-5 text-left text-sm text-black/70">
            <li>Take turns placing your mark. Get 5 in a row - horizontally, vertically, or diagonally - to win.</li>
            <li>Some fields secretly hide a mine.</li>
            <li>Hit one, and your own nearby marks disappear - your opponent's are untouched.</li>
            <li>The number on each mark shows how many live mines are still nearby.</li>
            <li>An exploded mine becomes a permanent crater - nobody can play there again.</li>
          </ul>
        </DialogContent>
      </Dialog>
    </footer>
  )
}
