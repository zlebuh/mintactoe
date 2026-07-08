import { describe, expect, it, vi } from 'vitest'
import { render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { createMemoryRouter, RouterProvider } from 'react-router'
import { HomeMenu } from './HomeMenu'

vi.mock('../lib/supabase', () => ({
  supabase: {
    auth: {
      getSession: () => Promise.resolve({ data: { session: null } }),
      signInAnonymously: () =>
        Promise.resolve({
          data: {
            session: {
              access_token: 'test-token',
              user: { id: 'test-user-id' },
            },
          },
        }),
      onAuthStateChange: (_cb: unknown) => ({
        data: { subscription: { unsubscribe: vi.fn() } },
      }),
    },
    from: () => ({
      select: () => ({
        or: () => ({
          order: () => ({
            limit: () => Promise.resolve({ data: [] }),
          }),
        }),
      }),
    }),
  },
}))

function renderHomeMenu() {
  const router = createMemoryRouter(
    [
      { path: '/', element: <HomeMenu /> },
      { path: '/local', element: <p>Local game screen</p> },
      { path: '/game/:gameId', element: <p>Online game screen</p> },
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

  it('opens the online-play dialog with create and join options', async () => {
    const user = userEvent.setup()
    renderHomeMenu()

    await user.click(screen.getByRole('button', { name: /Play online with a friend/ }))

    expect(screen.getByRole('dialog', { name: 'Play online' })).toBeInTheDocument()
    await waitFor(() => {
      expect(screen.getByRole('button', { name: /Create game/ })).toBeInTheDocument()
    })
    expect(screen.getByRole('button', { name: /Join by code/ })).toBeInTheDocument()
  })

  it('shows join-by-code input when "Join by code" is clicked', async () => {
    const user = userEvent.setup()
    renderHomeMenu()

    await user.click(screen.getByRole('button', { name: /Play online with a friend/ }))
    await waitFor(() => {
      expect(screen.getByRole('button', { name: /Join by code/ })).toBeEnabled()
    })
    await user.click(screen.getByRole('button', { name: /Join by code/ }))

    expect(screen.getByPlaceholderText(/Paste game code/)).toBeInTheDocument()
    expect(screen.getByRole('button', { name: /Join game/ })).toBeInTheDocument()
  })

  it('shows "Play vs bot" as disabled with a coming-soon badge', () => {
    renderHomeMenu()

    expect(screen.getByRole('button', { name: /Play vs bot/ })).toBeDisabled()
    expect(screen.getByText('Coming soon')).toBeInTheDocument()
  })
})
