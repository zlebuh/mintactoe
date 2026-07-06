import { describe, expect, it, vi } from 'vitest'
import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { initialize, makeMove } from '@mintactoe/game-engine'
import { Board } from './Board'

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

    await user.click(screen.getByLabelText('Row 1, column 1, O'))

    expect(onCellClick).not.toHaveBeenCalled()
  })
})
