import { Dialog, DialogContent, DialogDescription, DialogTitle, DialogTrigger } from './ui/Dialog'

export function Footer() {
  return (
    <footer className="mx-auto w-full max-w-md px-4 py-8 text-center text-sm text-black/60">
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
            <li>Once the game ends, every mine nobody found is revealed.</li>
          </ul>
        </DialogContent>
      </Dialog>
    </footer>
  )
}
