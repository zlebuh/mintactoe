import { assertEquals, assertNotEquals } from "jsr:@std/assert@1";
import type { SupabaseClient } from "npm:@supabase/supabase-js@2";
import { createGameRow, createMockSupabase } from "../_shared/testSupabase.ts";
import { createGame } from "./handler.ts";

Deno.test("createGame - inserts a new game when the caller hosts none", async () => {
  const inserted = createGameRow({ id: "new-game" });
  const supabase = createMockSupabase([
    { data: [], error: null }, // lookup for an existing unfinished game
    { data: inserted, error: null }, // insert
  ]) as unknown as SupabaseClient;

  const result = await createGame(supabase, { callerId: "host-1" });

  assertEquals(result.id, "new-game");
});

Deno.test("createGame - returns the caller's existing unfinished game instead of inserting", async () => {
  const existing = createGameRow({ id: "existing-game" });
  const supabase = createMockSupabase([
    { data: [existing], error: null },
  ]) as unknown as SupabaseClient;

  const result = await createGame(supabase, { callerId: "host-1" });

  assertEquals(result.id, "existing-game");
});

Deno.test("createGame - ignores the caller's finished games and creates a new one", async () => {
  const finished = createGameRow({ id: "finished-game" });
  finished.game_state.gameState.isGameOver = true;
  const inserted = createGameRow({ id: "new-game" });

  const supabase = createMockSupabase([
    { data: [finished], error: null },
    { data: inserted, error: null },
  ]) as unknown as SupabaseClient;

  const result = await createGame(supabase, { callerId: "host-1" });

  assertEquals(result.id, "new-game");
  assertNotEquals(result.id, "finished-game");
});
