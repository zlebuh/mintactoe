import { describe, expect, it, vi } from 'vitest'
import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { getField, initialize, makeMove } from '@mintactoe/game-engine'
import { Board } from './Board'

const noMines = () => 1

describe('Board', () => {
  it('renders one cell per field in the grid', () => {
    const game = initialize({ rows: 3, columns: 4 })
    render(<Board game={game} onCellClick={vi.fn()} />)

    expect(screen.getAllByRole('gridcell')).toHaveLength(12)
  })

  it('calls onCellClick with the clicked coordinate for an empty cell', async () => {
    const user = userEvent.setup()
    const game = initialize({ rows: 3, columns: 3 })
    const onCellClick = vi.fn()
    render(<Board game={game} onCellClick={onCellClick} />)

    await user.click(screen.getByLabelText('Row 1, column 1'))

    expect(onCellClick).toHaveBeenCalledWith({ row: 0, col: 0 })
  })

  it('does not fire for an already-occupied cell', async () => {
    const user = userEvent.setup()
    const game = initialize({ rows: 3, columns: 3 })
    makeMove(game, 'O', { row: 0, col: 0 })
    const onCellClick = vi.fn()
    render(<Board game={game} onCellClick={onCellClick} />)

    await user.click(screen.getByLabelText(/Row 1, column 1, O/))

    expect(onCellClick).not.toHaveBeenCalled()
  })

  it('renders an exploded mine as a crater and does not fire for it', async () => {
    const user = userEvent.setup()
    const game = initialize({ rows: 3, columns: 3, mineProbability: 0 })
    const mineField = getField(game.gameState.grid, { row: 0, col: 0 })
    mineField.isMine = true
    mineField.generated = true // prevents makeMove() from recomputing isMine on first visit
    makeMove(game, 'O', { row: 0, col: 0 }, noMines) // explodes - crater at (0, 0)
    const onCellClick = vi.fn()
    render(<Board game={game} onCellClick={onCellClick} />)

    const crater = screen.getByLabelText(/Row 1, column 1, exploded mine crater/)
    expect(crater).toBeDisabled()

    await user.click(crater)
    expect(onCellClick).not.toHaveBeenCalled()
  })
})
