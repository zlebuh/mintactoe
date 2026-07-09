import type { SupabaseClient } from "npm:@supabase/supabase-js@2";
import {
  CannotJoinOwnGameError,
  GameAlreadyFullError,
  GameNotFoundError,
} from "../_shared/errors.ts";
import type { GameRow } from "../_shared/gameRow.ts";

export interface JoinGameParams {
  callerId: string;
  gameId: string;
}

export async function joinGame(supabase: SupabaseClient, params: JoinGameParams): Promise<GameRow> {
  const code = params.gameId;

  const isFullUuid = code.length === 36;
  const query = supabase.from("games").select("*");
  const { data: rows, error: fetchError } = isFullUuid
    ? await query.eq("id", code)
    : await query
        .gte("id", `${code.padEnd(8, "0")}-0000-0000-0000-000000000000`)
        .lte("id", `${code.padEnd(8, "f")}-ffff-ffff-ffff-ffffffffffff`);

  if (fetchError) {
    throw fetchError;
  }
  const row = rows?.[0] ?? null;
  if (!row) {
    throw new GameNotFoundError(code);
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
    .eq("id", game.id)
    .select("*")
    .single();

  if (updateError) {
    throw updateError;
  }
  return updated as GameRow;
}
