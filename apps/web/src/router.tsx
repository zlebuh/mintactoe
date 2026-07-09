import { createBrowserRouter } from 'react-router'
import { Layout } from './components/Layout'
import { HomeMenu } from './pages/HomeMenu'
import { LocalGamePage } from './pages/LocalGamePage'
import { OnlineGamePage } from './pages/OnlineGamePage'

export const router = createBrowserRouter([
  {
    element: <Layout />,
    children: [
      { path: '/', element: <HomeMenu /> },
      { path: '/local', element: <LocalGamePage /> },
      { path: '/game/:gameId', element: <OnlineGamePage /> },
    ],
  },
])
