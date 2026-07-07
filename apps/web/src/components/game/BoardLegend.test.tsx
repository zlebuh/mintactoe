import { describe, expect, it } from 'vitest'
import { render, screen } from '@testing-library/react'
import { BoardLegend } from './BoardLegend'

describe('BoardLegend', () => {
  it('explains players marks, what the number on them means, and exploded mines', () => {
    render(<BoardLegend />)

    expect(screen.getByText("Players' marks")).toBeInTheDocument()
    expect(screen.getByText(/count of live mines nearby/)).toBeInTheDocument()
    expect(screen.getByText('Exploded mine')).toBeInTheDocument()
  })
})
