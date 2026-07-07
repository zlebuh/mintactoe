import { useNavigate } from 'react-router'
import { CardButton, CardDescription, CardTitle } from '../components/ui/Card'
import { Badge } from '../components/ui/Badge'
import { Button } from '../components/ui/Button'
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogTitle,
  DialogTrigger,
} from '../components/ui/Dialog'

export function HomeMenu() {
  const navigate = useNavigate()

  return (
    <main className="mx-auto flex max-w-md flex-col gap-4 px-4 pt-16 pb-10">
      <h1 className="mb-6 text-center text-4xl font-extrabold text-brand">MinTacToe</h1>

      <CardButton onClick={() => navigate('/local')}>
        <CardTitle>Play locally</CardTitle>
        <CardDescription>2 players, same device</CardDescription>
      </CardButton>

      <Dialog>
        <DialogTrigger asChild>
          <CardButton>
            <CardTitle>Play online with a friend</CardTitle>
            <CardDescription>Create a game and send the link, or join one</CardDescription>
          </CardButton>
        </DialogTrigger>
        <DialogContent>
          <DialogTitle>Play online</DialogTitle>
          <DialogDescription>Coming in a future update.</DialogDescription>
          <div className="mt-5 flex flex-col gap-2">
            <Button disabled>Create game</Button>
            <Button variant="secondary" disabled>
              Join by code
            </Button>
          </div>
        </DialogContent>
      </Dialog>

      <CardButton disabled>
        <div className="flex items-center justify-between gap-2">
          <CardTitle>Play vs bot</CardTitle>
          <Badge>Coming soon</Badge>
        </div>
        <CardDescription>Practice against the computer</CardDescription>
      </CardButton>
    </main>
  )
}
