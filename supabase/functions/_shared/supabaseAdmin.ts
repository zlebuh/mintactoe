import { createClient, type SupabaseClient } from "npm:@supabase/supabase-js@2";

// SUPABASE_URL and SUPABASE_SERVICE_ROLE_KEY are provided automatically by the Edge Functions
// runtime (both locally via `supabase start`/`functions serve` and when deployed) - no secrets
// setup needed. This client bypasses RLS, which is exactly why writes to `games` only happen
// through these functions (see supabase/migrations/20260704220000_games_schema.sql).
export function createAdminClient(): SupabaseClient {
  const url = Deno.env.get("SUPABASE_URL");
  const serviceRoleKey = Deno.env.get("SUPABASE_SERVICE_ROLE_KEY");
  if (!url || !serviceRoleKey) {
    throw new Error("SUPABASE_URL and SUPABASE_SERVICE_ROLE_KEY must be set.");
  }
  return createClient(url, serviceRoleKey, { auth: { persistSession: false } });
}
