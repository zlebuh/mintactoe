import type { SupabaseClient } from "npm:@supabase/supabase-js@2";
import { deserializeGame, serializeGame } from "../../../packages/game-engine/dist/index.js";
import { GameNotFoundError, InvalidRequestError, NotAParticipantError } from "../_shared/errors.ts";
import type { GameRow, Player } from "../_shared/gameRow.ts";

export interface ForfeitGameParams {
  callerId: string;
  gameId: string;
}

function resolveOpponent(game: GameRow, callerId: string): Player {
  if (game.host_user_id === callerId) return "X";
  if (game.invited_user_id === callerId) return "O";
  throw new NotAParticipantError();
}

export async function forfeitGame(supabase: SupabaseClient, params: ForfeitGameParams): Promise<GameRow> {
  if (!params.gameId) {
    throw new InvalidRequestError("gameId is required.");
  }

  const { data: row, error: fetchError } = await supabase
    .from("games")
    .select("*")
    .eq("id", params.gameId)
    .maybeSingle();

  if (fetchError) {
    throw fetchError;
  }
  if (!row) {
    throw new GameNotFoundError(params.gameId);
  }

  const gameRow = row as GameRow;
  const winner = resolveOpponent(gameRow, params.callerId);

  const game = deserializeGame(gameRow.game_state);
  game.gameState.isGameOver = true;
  game.gameState.winner = winner;
  game.gameState.playerOnTurn = null;
  const serialized = serializeGame(game);

  const { data: updated, error: updateError } = await supabase
    .from("games")
    .update({ game_state: serialized })
    .eq("id", params.gameId)
    .select("*")
    .single();

  if (updateError) {
    throw updateError;
  }
  return updated as GameRow;
}
