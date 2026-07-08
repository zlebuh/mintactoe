import type { SupabaseClient } from "npm:@supabase/supabase-js@2";
import { initialize, serializeGame } from "../../../packages/game-engine/dist/index.js";
import { isUnfinished, type GameRow } from "../_shared/gameRow.ts";

export interface CreateGameParams {
  callerId: string;
}

/**
 * Idempotent per caller: if `callerId` already hosts an unfinished game, that row is
 * returned instead of inserting a new one. This both caps create-spam from one session
 * (issue #9) and doubles as the "resume my game" mechanism (issue #6/#7).
 */
export async function createGame(
  supabase: SupabaseClient,
  params: CreateGameParams,
): Promise<GameRow> {
  const { data: existingRows, error: fetchError } = await supabase
    .from("games")
    .select("*")
    .eq("host_user_id", params.callerId)
    .order("created_at", { ascending: false });

  if (fetchError) {
    throw fetchError;
  }

  const existing = (existingRows as GameRow[] | null)?.find(isUnfinished);
  if (existing) {
    return existing;
  }

  const serialized = serializeGame(initialize());

  const { data: inserted, error: insertError } = await supabase
    .from("games")
    .insert({ host_user_id: params.callerId, game_state: serialized })
    .select("*")
    .single();

  if (insertError) {
    throw insertError;
  }
  return inserted as GameRow;
}
