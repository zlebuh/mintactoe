import { useCallback, useEffect, useRef, useState } from 'react'
import type { Session } from '@supabase/supabase-js'
import { deserializeGame, type Coordinate, type Game } from '@mintactoe/game-engine'
import { supabase } from '../lib/supabase'
import { joinGame as joinGameApi, makeGameMove, ApiError, type GameRow } from '../lib/api'

export type OnlineRole = 'host' | 'visitor' | 'spectator'

function deriveRole(row: GameRow, userId: string): OnlineRole {
  if (row.host_user_id === userId) return 'host'
  if (row.invited_user_id === userId) return 'visitor'
  return 'spectator'
}

function fetchGame(gameId: string) {
  const query = supabase.from('games').select('*')
  if (gameId.length === 36) {
    return query.eq('id', gameId).maybeSingle()
  }
  return query
    .gte('id', `${gameId.padEnd(8, '0')}-0000-0000-0000-000000000000`)
    .lte('id', `${gameId.padEnd(8, 'f')}-ffff-ffff-ffff-ffffffffffff`)
    .maybeSingle()
}

export function useOnlineGame(gameId: string, session: Session) {
  const [gameRow, setGameRow] = useState<GameRow | null>(null)
  const [game, setGame] = useState<Game | null>(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [needsJoin, setNeedsJoin] = useState(false)
  const channelRef = useRef<ReturnType<typeof supabase.channel> | null>(null)
  const resolvedIdRef = useRef<string>(gameId)

  const userId = session.user.id
  const token = session.access_token

  const applyRow = useCallback((row: GameRow) => {
    setGameRow(row)
    setGame(deserializeGame(row.game_state))
  }, [])

  const subscribe = useCallback(
    (fullId: string) => {
      if (channelRef.current) return
      const channel = supabase
        .channel(`game:${fullId}`)
        .on(
          'postgres_changes',
          {
            event: 'UPDATE',
            schema: 'public',
            table: 'games',
            filter: `id=eq.${fullId}`,
          },
          (payload) => {
            applyRow(payload.new as unknown as GameRow)
          },
        )
        .subscribe()
      channelRef.current = channel
    },
    [applyRow],
  )

  useEffect(() => {
    let cancelled = false

    async function load() {
      const { data, error: fetchError } = await fetchGame(gameId)

      if (cancelled) return

      if (fetchError || !data) {
        setError(fetchError?.message ?? 'Game not found.')
        setLoading(false)
        return
      }

      const row = data as unknown as GameRow
      const fullId = row.id
      resolvedIdRef.current = fullId

      if (row.invited_user_id === null && row.host_user_id !== userId) {
        applyRow(row)
        setNeedsJoin(true)
        setLoading(false)
        return
      }

      applyRow(row)
      setLoading(false)
      subscribe(fullId)
    }

    load()

    return () => {
      cancelled = true
      if (channelRef.current) {
        channelRef.current.unsubscribe()
        channelRef.current = null
      }
    }
  }, [gameId, userId, token, applyRow, subscribe])

  const join = useCallback(async () => {
    const fullId = resolvedIdRef.current
    try {
      const row = await joinGameApi(token, fullId)
      applyRow(row)
      setNeedsJoin(false)
      subscribe(fullId)
    } catch (err) {
      if (err instanceof ApiError && err.code === 'GameAlreadyFull') {
        const { data: refreshed } = await supabase
          .from('games')
          .select('*')
          .eq('id', fullId)
          .single()
        if (refreshed) {
          applyRow(refreshed as unknown as GameRow)
          setNeedsJoin(false)
          subscribe(fullId)
        }
      } else {
        setError(err instanceof ApiError ? err.message : 'Failed to join game.')
      }
    }
  }, [token, applyRow, subscribe])

  const myRole: OnlineRole | null = gameRow ? deriveRole(gameRow, userId) : null

  const move = useCallback(
    async (coordinate: Coordinate) => {
      await makeGameMove(token, resolvedIdRef.current, coordinate)
    },
    [token],
  )

  return { game, gameRow, myRole, move, loading, error, needsJoin, join }
}
