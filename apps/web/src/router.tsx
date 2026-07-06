import { createBrowserRouter } from 'react-router'
import { HomeMenu } from './pages/HomeMenu'
import { LocalGamePage } from './pages/LocalGamePage'

export const router = createBrowserRouter([
  { path: '/', element: <HomeMenu /> },
  { path: '/local', element: <LocalGamePage /> },
])
