import type { Coordinate, SerializedGame } from '@mintactoe/game-engine'

export interface GameRow {
  id: string
  host_user_id: string
  invited_user_id: string | null
  game_state: SerializedGame
  created_at: string
}

export class ApiError extends Error {
  readonly code: string
  constructor(code: string, message: string) {
    super(message)
    this.name = 'ApiError'
    this.code = code
  }
}

const baseUrl = import.meta.env.VITE_SUPABASE_URL as string

function headers(token: string): HeadersInit {
  return {
    Authorization: `Bearer ${token}`,
    'Content-Type': 'application/json',
  }
}

async function parseResponse<T>(res: Response): Promise<T> {
  const body = await res.json()
  if (!res.ok) {
    throw new ApiError(body.error ?? 'UnknownError', body.message ?? res.statusText)
  }
  return body as T
}

export async function createGame(token: string): Promise<GameRow> {
  const res = await fetch(`${baseUrl}/functions/v1/create-game`, {
    method: 'POST',
    headers: headers(token),
  })
  const { game } = await parseResponse<{ game: GameRow }>(res)
  return game
}

export async function joinGame(token: string, gameId: string): Promise<GameRow> {
  const res = await fetch(`${baseUrl}/functions/v1/join-game`, {
    method: 'POST',
    headers: headers(token),
    body: JSON.stringify({ gameId }),
  })
  const { game } = await parseResponse<{ game: GameRow }>(res)
  return game
}

export async function makeGameMove(
  token: string,
  gameId: string,
  coordinate: Coordinate,
): Promise<GameRow> {
  const res = await fetch(`${baseUrl}/functions/v1/make-move`, {
    method: 'POST',
    headers: headers(token),
    body: JSON.stringify({ gameId, coordinate }),
  })
  const { game } = await parseResponse<{ game: GameRow }>(res)
  return game
}

export async function forfeitGame(token: string, gameId: string): Promise<GameRow> {
  const res = await fetch(`${baseUrl}/functions/v1/forfeit-game`, {
    method: 'POST',
    headers: headers(token),
    body: JSON.stringify({ gameId }),
  })
  const { game } = await parseResponse<{ game: GameRow }>(res)
  return game
}
