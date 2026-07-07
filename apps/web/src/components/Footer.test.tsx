import { describe, expect, it } from 'vitest'
import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { Footer } from './Footer'

describe('Footer', () => {
  it('shows the on-board legend', () => {
    render(<Footer />)

    expect(screen.getByText("O's mark")).toBeInTheDocument()
    expect(screen.getByText("X's mark")).toBeInTheDocument()
    expect(screen.getByText('Exploded mine')).toBeInTheDocument()
  })

  it('opens the simplified rules in a dialog', async () => {
    const user = userEvent.setup()
    render(<Footer />)

    await user.click(screen.getByRole('button', { name: 'Game rules' }))

    expect(screen.getByRole('dialog', { name: 'How to play' })).toBeInTheDocument()
    expect(screen.getByText(/permanent crater/)).toBeInTheDocument()
  })
})
