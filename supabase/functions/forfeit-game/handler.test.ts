import { assertEquals, assertRejects } from "jsr:@std/assert@1";
import type { SupabaseClient } from "npm:@supabase/supabase-js@2";
import { GameNotFoundError, NotAParticipantError } from "../_shared/errors.ts";
import { createGameRow, createMockSupabase } from "../_shared/testSupabase.ts";
import { forfeitGame } from "./handler.ts";

Deno.test("forfeitGame - host forfeits and visitor (X) wins", async () => {
  const game = createGameRow({ id: "game-1", host_user_id: "host-1", invited_user_id: "visitor-1" });
  const updated = createGameRow({ id: "game-1", host_user_id: "host-1", invited_user_id: "visitor-1" });
  updated.game_state.gameState.isGameOver = true;
  updated.game_state.gameState.winner = "X";
  updated.game_state.gameState.playerOnTurn = null;

  const supabase = createMockSupabase([
    { data: game, error: null }, // fetch
    { data: updated, error: null }, // update
  ]) as unknown as SupabaseClient;

  const result = await forfeitGame(supabase, { callerId: "host-1", gameId: "game-1" });

  assertEquals(result.game_state.gameState.isGameOver, true);
  assertEquals(result.game_state.gameState.winner, "X");
  assertEquals(result.game_state.gameState.playerOnTurn, null);
});

Deno.test("forfeitGame - visitor forfeits and host (O) wins", async () => {
  const game = createGameRow({ id: "game-1", host_user_id: "host-1", invited_user_id: "visitor-1" });
  const updated = createGameRow({ id: "game-1", host_user_id: "host-1", invited_user_id: "visitor-1" });
  updated.game_state.gameState.isGameOver = true;
  updated.game_state.gameState.winner = "O";
  updated.game_state.gameState.playerOnTurn = null;

  const supabase = createMockSupabase([
    { data: game, error: null },
    { data: updated, error: null },
  ]) as unknown as SupabaseClient;

  const result = await forfeitGame(supabase, { callerId: "visitor-1", gameId: "game-1" });

  assertEquals(result.game_state.gameState.isGameOver, true);
  assertEquals(result.game_state.gameState.winner, "O");
});

Deno.test("forfeitGame - rejects forfeit by a non-participant", async () => {
  const game = createGameRow({ id: "game-1", host_user_id: "host-1", invited_user_id: "visitor-1" });
  const supabase = createMockSupabase([
    { data: game, error: null },
  ]) as unknown as SupabaseClient;

  await assertRejects(
    () => forfeitGame(supabase, { callerId: "stranger-1", gameId: "game-1" }),
    NotAParticipantError,
  );
});

Deno.test("forfeitGame - rejects forfeit when game does not exist", async () => {
  const supabase = createMockSupabase([
    { data: null, error: null },
  ]) as unknown as SupabaseClient;

  await assertRejects(
    () => forfeitGame(supabase, { callerId: "host-1", gameId: "missing-game" }),
    GameNotFoundError,
  );
});
