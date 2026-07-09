import type { SupabaseClient } from "npm:@supabase/supabase-js@2";
import {
  deserializeGame,
  makeMove as engineMakeMove,
  serializeGame,
} from "../../../packages/game-engine/dist/index.js";
import { GameNotFoundError, NotAParticipantError } from "../_shared/errors.ts";
import type { Coordinate, GameRow, Player } from "../_shared/gameRow.ts";

export interface MakeMoveParams {
  callerId: string;
  gameId: string;
  coordinate: Coordinate;
}

function resolvePlayer(game: GameRow, callerId: string): Player {
  if (game.host_user_id === callerId) {
    return "O";
  }
  if (game.invited_user_id === callerId) {
    return "X";
  }
  throw new NotAParticipantError();
}

export async function makeMove(supabase: SupabaseClient, params: MakeMoveParams): Promise<GameRow> {

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
  const player = resolvePlayer(gameRow, params.callerId);

  const game = deserializeGame(gameRow.game_state);
  engineMakeMove(game, player, params.coordinate);
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
