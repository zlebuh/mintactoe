import { describe, expect, it } from "vitest";
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

const SUPABASE_URL = requireEnv("SUPABASE_URL");
const ANON_KEY = requireEnv("SUPABASE_ANON_KEY");
const SERVICE_ROLE_KEY = requireEnv("SUPABASE_SERVICE_ROLE_KEY");

interface AnonSession {
  client: SupabaseClient;
  userId: string;
}

async function signInAnonymously(): Promise<AnonSession> {
  const client = createClient(SUPABASE_URL, ANON_KEY);
  const { data, error } = await client.auth.signInAnonymously();
  if (error || !data.user) {
    throw error ?? new Error("anonymous sign-in returned no user");
  }
  return { client, userId: data.user.id };
}

const serviceClient = createClient(SUPABASE_URL, SERVICE_ROLE_KEY);

describe("games RLS + grants", () => {
  it("gates reads/writes across host, invited participant, and an uninvolved third party", async () => {
    const host = await signInAnonymously();
    const visitor = await signInAnonymously();
    const stranger = await signInAnonymously();

    const created = await serviceClient
      .from("games")
      .insert({ host_user_id: host.userId, game_state: {} })
      .select()
      .single();
    expect(created.error).toBeNull();
    const gameId = created.data!.id as string;

    const hostRead = await host.client.from("games").select().eq("id", gameId);
    expect(hostRead.data).toHaveLength(1);

    const openInviteRead = await visitor.client.from("games").select().eq("id", gameId);
    expect(openInviteRead.data).toHaveLength(1);

    const directJoinAttempt = await visitor.client
      .from("games")
      .update({ invited_user_id: visitor.userId })
      .eq("id", gameId);
    expect(directJoinAttempt.error?.code).toBe("42501");

    const join = await serviceClient
      .from("games")
      .update({ invited_user_id: visitor.userId })
      .eq("id", gameId);
    expect(join.error).toBeNull();

    const strangerReadAfterClose = await stranger.client.from("games").select().eq("id", gameId);
    expect(strangerReadAfterClose.data).toHaveLength(0);

    const participantRead = await visitor.client.from("games").select().eq("id", gameId);
    expect(participantRead.data).toHaveLength(1);

    const directMoveAttempt = await visitor.client
      .from("games")
      .update({ game_state: { hacked: true } })
      .eq("id", gameId);
    expect(directMoveAttempt.error?.code).toBe("42501");
  });

  it("lets a participant list their own games without seeing strangers' open lobbies", async () => {
    const me = await signInAnonymously();
    const someoneElse = await signInAnonymously();

    const mine = await serviceClient
      .from("games")
      .insert({ host_user_id: me.userId, game_state: {} })
      .select()
      .single();
    expect(mine.error).toBeNull();

    const theirsOpen = await serviceClient
      .from("games")
      .insert({ host_user_id: someoneElse.userId, game_state: {} })
      .select()
      .single();
    expect(theirsOpen.error).toBeNull();

    const myGames = await me.client
      .from("games")
      .select()
      .or(`host_user_id.eq.${me.userId},invited_user_id.eq.${me.userId}`);

    expect(myGames.data?.map((g) => g.id)).toContain(mine.data!.id);
    expect(myGames.data?.map((g) => g.id)).not.toContain(theirsOpen.data!.id);
  });
});
