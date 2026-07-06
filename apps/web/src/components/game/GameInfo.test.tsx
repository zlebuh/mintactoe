import { describe, expect, it, vi } from 'vitest'
import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { initialize, type Game } from '@mintactoe/game-engine'
import { GameInfo } from './GameInfo'

function withGameState(overrides: Partial<Game['gameState']>): Game {
  const game = initialize({ rows: 3, columns: 3 })
  Object.assign(game.gameState, overrides)
  return game
}

describe('GameInfo', () => {
  it('shows whose turn it is and the move count while the game is in progress', () => {
    const game = withGameState({ playerOnTurn: 'X', movesPlayed: 3 })
    render(<GameInfo game={game} onReset={vi.fn()} />)

    expect(screen.getByText(/Turn:/)).toBeInTheDocument()
    expect(screen.getByText('X')).toBeInTheDocument()
    expect(screen.getByText('Moves: 3')).toBeInTheDocument()
  })

  it('shows a winner banner and lets the player start a new game', async () => {
    const user = userEvent.setup()
    const game = withGameState({ isGameOver: true, winner: 'O', playerOnTurn: null })
    const onReset = vi.fn()
    render(<GameInfo game={game} onReset={onReset} />)

    expect(screen.getByText(/wins!/)).toBeInTheDocument()

    await user.click(screen.getByRole('button', { name: 'New game' }))
    expect(onReset).toHaveBeenCalledOnce()
  })

  it('shows a tie banner when the game ends without a winner', () => {
    const game = withGameState({ isGameOver: true, winner: null, playerOnTurn: null })
    render(<GameInfo game={game} onReset={vi.fn()} />)

    expect(screen.getByText("It's a tie!")).toBeInTheDocument()
  })
})
