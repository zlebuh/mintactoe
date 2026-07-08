import { Outlet } from 'react-router'
import { Footer } from './Footer'

export function Layout() {
  return (
    <div className="flex min-h-dvh flex-col">
      <div className="flex-1">
        <Outlet />
      </div>
      <Footer />
    </div>
  )
}
