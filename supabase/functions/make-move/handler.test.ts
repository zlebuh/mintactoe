import { assertEquals, assertRejects } from "jsr:@std/assert@1";
import type { SupabaseClient } from "npm:@supabase/supabase-js@2";
import { NotAParticipantError } from "../_shared/errors.ts";
import { NotYourTurnError } from "../../../packages/game-engine/dist/index.js";
import { createGameRow, createMockSupabase } from "../_shared/testSupabase.ts";
import { makeMove } from "./handler.ts";

Deno.test("makeMove - happy path: host (O) moves and the game row is updated", async () => {
  const game = createGameRow({ id: "game-1", host_user_id: "host-1", invited_user_id: "visitor-1" });
  const updated = createGameRow({ id: "game-1", host_user_id: "host-1", invited_user_id: "visitor-1" });
  updated.game_state.gameState.movesPlayed = 1;

  const supabase = createMockSupabase([
    { data: game, error: null }, // fetch
    { data: updated, error: null }, // update
  ]) as unknown as SupabaseClient;

  const result = await makeMove(supabase, {
    callerId: "host-1",
    gameId: "game-1",
    coordinate: { row: 0, col: 0 },
  });

  assertEquals(result.game_state.gameState.movesPlayed, 1);
});

Deno.test("makeMove - rejects a move by someone who isn't a participant", async () => {
  const game = createGameRow({ id: "game-1", host_user_id: "host-1", invited_user_id: "visitor-1" });
  const supabase = createMockSupabase([
    { data: game, error: null },
  ]) as unknown as SupabaseClient;

  await assertRejects(
    () => makeMove(supabase, { callerId: "stranger-1", gameId: "game-1", coordinate: { row: 0, col: 0 } }),
    NotAParticipantError,
  );
});

Deno.test("makeMove - rejects a move when it isn't the caller's turn", async () => {
  const game = createGameRow({ id: "game-1", host_user_id: "host-1", invited_user_id: "visitor-1" });
  game.game_state.gameState.playerOnTurn = "X"; // visitor's turn, not the host's

  const supabase = createMockSupabase([
    { data: game, error: null },
  ]) as unknown as SupabaseClient;

  await assertRejects(
    () => makeMove(supabase, { callerId: "host-1", gameId: "game-1", coordinate: { row: 0, col: 0 } }),
    NotYourTurnError,
  );
});

