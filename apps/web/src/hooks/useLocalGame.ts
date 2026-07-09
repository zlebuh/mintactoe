import { useCallback, useState } from 'react'
import { MinTacToeError, initialize, makeMove, type Coordinate, type Game } from '@mintactoe/game-engine'

export function useLocalGame() {
  const [game, setGame] = useState<Game>(() => initialize())

  const move = useCallback((coordinate: Coordinate) => {
    setGame((previous) => {
      if (previous.gameState.playerOnTurn === null) {
        return previous
      }
      // makeMove() mutates its argument in place, which isn't React-state-friendly - clone
      // first so React only re-renders on a genuinely new object, and so an illegal move
      // (e.g. clicking an occupied cell) can be discarded without corrupting `previous`.
      const next = structuredClone(previous)
      try {
        makeMove(next, previous.gameState.playerOnTurn, coordinate)
      } catch (error) {
        if (error instanceof MinTacToeError) {
          return previous
        }
        throw error
      }
      return next
    })
  }, [])

  const reset = useCallback(() => setGame(initialize()), [])

  return { game, move, reset }
}
