import { describe, expect, it } from 'vitest'
import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { createMemoryRouter, RouterProvider } from 'react-router'
import { HomeMenu } from './HomeMenu'

function renderHomeMenu() {
  const router = createMemoryRouter(
    [
      { path: '/', element: <HomeMenu /> },
      { path: '/local', element: <p>Local game screen</p> },
    ],
    { initialEntries: ['/'] },
  )
  render(<RouterProvider router={router} />)
}

describe('HomeMenu', () => {
  it('navigates to /local when "Play locally" is clicked', async () => {
    const user = userEvent.setup()
    renderHomeMenu()

    await user.click(screen.getByRole('button', { name: /Play locally/ }))

    expect(screen.getByText('Local game screen')).toBeInTheDocument()
  })

  it('opens the online-play dialog shell', async () => {
    const user = userEvent.setup()
    renderHomeMenu()

    await user.click(screen.getByRole('button', { name: /Play online with a friend/ }))

    expect(screen.getByRole('dialog', { name: 'Play online' })).toBeInTheDocument()
    expect(screen.getByRole('button', { name: 'Create game' })).toBeDisabled()
    expect(screen.getByRole('button', { name: 'Join by code' })).toBeDisabled()
  })

  it('shows "Play vs bot" as disabled with a coming-soon badge', () => {
    renderHomeMenu()

    expect(screen.getByRole('button', { name: /Play vs bot/ })).toBeDisabled()
    expect(screen.getByText('Coming soon')).toBeInTheDocument()
  })
})
