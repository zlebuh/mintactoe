import { Link } from 'react-router'
import { Board } from '../components/game/Board'
import { BoardLegend } from '../components/game/BoardLegend'
import { GameInfo } from '../components/game/GameInfo'
import { useLocalGame } from '../hooks/useLocalGame'

export function LocalGamePage() {
  const { game, move, reset } = useLocalGame()

  return (
    <main className="mx-auto flex max-w-md flex-col items-center gap-4 px-4 py-6">
      <div className="flex w-full items-center justify-between">
        <Link to="/" className="text-sm font-semibold text-brand">
          ← Home
        </Link>
        <h1 className="text-lg font-extrabold">Local game</h1>
        <div className="w-12" />
      </div>

      <GameInfo game={game} onReset={reset} />
      <Board game={game} onCellClick={move} />
      <BoardLegend />
    </main>
  )
}
