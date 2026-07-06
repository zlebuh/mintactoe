import { useCallback, useState } from 'react'
import {
  MinTacToeError,
  initialize,
  makeMove,
  type Coordinate,
  type Game,
  type Rules,
} from '@mintactoe/game-engine'

// Matches the documented production (online) board size (docs/game-rules.md) rather than the
// engine's own 20x20 default, for a consistent feel between local and online play and a better
// fit on a phone screen.
export const LOCAL_RULES: Partial<Rules> = { rows: 16, columns: 16 }

export function useLocalGame() {
  const [game, setGame] = useState<Game>(() => initialize(LOCAL_RULES))

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

  const reset = useCallback(() => setGame(initialize(LOCAL_RULES)), [])

  return { game, move, reset }
}
