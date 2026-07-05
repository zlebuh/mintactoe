import { describe, expect, it } from "vitest";
import { createClient, type SupabaseClient } from "@supabase/supabase-js";

// Fixed local-dev demo keys - identical for every fresh `supabase init` project using the
// default JWT secret, not a real secret. Overridable via env for a non-default local stack.
const SUPABASE_URL = process.env.SUPABASE_URL ?? "http://127.0.0.1:54321";
const ANON_KEY =
  process.env.SUPABASE_ANON_KEY ??
  "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZS1kZW1vIiwicm9sZSI6ImFub24iLCJleHAiOjE5ODM4MTI5OTZ9.CRXP1A7WOeoJeXxjNni43kdQwgnWNReilDMblYTn_I0";
const SERVICE_ROLE_KEY =
  process.env.SUPABASE_SERVICE_ROLE_KEY ??
  "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZS1kZW1vIiwicm9sZSI6InNlcnZpY2Vfcm9sZSIsImV4cCI6MTk4MzgxMjk5Nn0.EGIM96RAZx35lJzdJsyH-qQwv8Hdp7fsn3W0YpN81IU";

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
