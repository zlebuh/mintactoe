import { useEffect, useState } from 'react'
import type { Session } from '@supabase/supabase-js'
import { supabase } from '../lib/supabase'

export function useAuth() {
  const [session, setSession] = useState<Session | null>(null)
  const [loading, setLoading] = useState(true)

  useEffect(() => {
    const { data: { subscription } } = supabase.auth.onAuthStateChange((_event, s) => {
      setSession(s)
      setLoading(false)
    })

    async function init() {
      const { data: { session: existing } } = await supabase.auth.getSession()
      if (existing) {
        const { error } = await supabase.auth.getUser()
        if (error) {
          await supabase.auth.signOut()
          const { data: { session: fresh } } = await supabase.auth.signInAnonymously()
          setSession(fresh)
          setLoading(false)
          return
        }
        setSession(existing)
        setLoading(false)
      } else {
        const { data: { session: fresh } } = await supabase.auth.signInAnonymously()
        setSession(fresh)
        setLoading(false)
      }
    }
    init()

    return () => subscription.unsubscribe()
  }, [])

  return { session, loading }
}
