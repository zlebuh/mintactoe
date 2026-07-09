import { createClient, type SupabaseClient } from "@supabase/supabase-js";

// No fallback values on purpose: these must come from the actually-running local stack
// (`supabase status -o env`), never a literal string committed to the repo, even a
// non-secret local-dev default. Missing env fails the whole file loudly instead of
// silently running against a guessed value.
function requireEnv(name: string): string {
  const value = process.env[name];
  if (!value) {
    throw new Error(
      `${name} is not set. Run \`supabase start\`, then export its output ` +
        "(see CLAUDE.md's Local development section) before running these tests.",
    );
  }
  return value;
}

export const SUPABASE_URL = requireEnv("SUPABASE_URL");
export const SUPABASE_ANON_KEY = requireEnv("SUPABASE_ANON_KEY");
export const SUPABASE_SERVICE_ROLE_KEY = requireEnv("SUPABASE_SERVICE_ROLE_KEY");

export interface AnonSession {
  client: SupabaseClient;
  userId: string;
  accessToken: string;
}

export async function signInAnonymously(): Promise<AnonSession> {
  const client = createClient(SUPABASE_URL, SUPABASE_ANON_KEY);
  const { data, error } = await client.auth.signInAnonymously();
  if (error || !data.user || !data.session) {
    throw error ?? new Error("anonymous sign-in returned no user/session");
  }
  return { client, userId: data.user.id, accessToken: data.session.access_token };
}

export const serviceClient: SupabaseClient = createClient(SUPABASE_URL, SUPABASE_SERVICE_ROLE_KEY);
