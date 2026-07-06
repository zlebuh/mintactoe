import type { SupabaseClient } from "npm:@supabase/supabase-js@2";
import {
  CannotJoinOwnGameError,
  GameAlreadyFullError,
  GameNotFoundError,
  InvalidRequestError,
} from "../_shared/errors.ts";
import type { GameRow } from "../_shared/gameRow.ts";

export interface JoinGameParams {
  callerId: string;
  gameId: string;
}

export async function joinGame(supabase: SupabaseClient, params: JoinGameParams): Promise<GameRow> {
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

  const game = row as GameRow;
  if (game.host_user_id === params.callerId) {
    throw new CannotJoinOwnGameError();
  }
  if (game.invited_user_id !== null) {
    throw new GameAlreadyFullError();
  }

  const { data: updated, error: updateError } = await supabase
    .from("games")
    .update({ invited_user_id: params.callerId })
    .eq("id", params.gameId)
    .select("*")
    .single();

  if (updateError) {
    throw updateError;
  }
  return updated as GameRow;
}
