import { assertEquals, assertRejects } from "jsr:@std/assert@1";
import type { SupabaseClient } from "npm:@supabase/supabase-js@2";
import {
  CannotJoinOwnGameError,
  GameAlreadyFullError,
  GameNotFoundError,
} from "../_shared/errors.ts";
import { createGameRow, createMockSupabase } from "../_shared/testSupabase.ts";
import { joinGame } from "./handler.ts";

Deno.test("joinGame - happy path sets the caller as the invited player", async () => {
  const open = createGameRow({ id: "game-1", host_user_id: "host-1", invited_user_id: null });
  const updated = createGameRow({ id: "game-1", host_user_id: "host-1", invited_user_id: "visitor-1" });

  const supabase = createMockSupabase([
    { data: open, error: null }, // fetch
    { data: updated, error: null }, // update
  ]) as unknown as SupabaseClient;

  const result = await joinGame(supabase, { callerId: "visitor-1", gameId: "game-1" });

  assertEquals(result.invited_user_id, "visitor-1");
});

Deno.test("joinGame - rejects joining a game that already has an invited player", async () => {
  const full = createGameRow({ id: "game-1", host_user_id: "host-1", invited_user_id: "visitor-1" });
  const supabase = createMockSupabase([
    { data: full, error: null },
  ]) as unknown as SupabaseClient;

  await assertRejects(
    () => joinGame(supabase, { callerId: "stranger-1", gameId: "game-1" }),
    GameAlreadyFullError,
  );
});

Deno.test("joinGame - rejects the host joining their own game", async () => {
  const open = createGameRow({ id: "game-1", host_user_id: "host-1", invited_user_id: null });
  const supabase = createMockSupabase([
    { data: open, error: null },
  ]) as unknown as SupabaseClient;

  await assertRejects(
    () => joinGame(supabase, { callerId: "host-1", gameId: "game-1" }),
    CannotJoinOwnGameError,
  );
});

Deno.test("joinGame - rejects joining a game that doesn't exist", async () => {
  const supabase = createMockSupabase([
    { data: null, error: null },
  ]) as unknown as SupabaseClient;

  await assertRejects(
    () => joinGame(supabase, { callerId: "visitor-1", gameId: "missing-game" }),
    GameNotFoundError,
  );
});
