import type { SupabaseClient } from "npm:@supabase/supabase-js@2";
import {
  deserializeGame,
  makeMove as engineMakeMove,
  serializeGame,
} from "../../../packages/game-engine/dist/index.js";
import { GameNotFoundError, InvalidRequestError, NotAParticipantError } from "../_shared/errors.ts";
import type { Coordinate, GameRow, Player } from "../_shared/gameRow.ts";

export interface MakeMoveParams {
  callerId: string;
  gameId: string;
  coordinate: unknown;
}

// The host always plays first as "O" (see docs/game-rules.md); the invited player is "X".
// This mapping is derived from host_user_id/invited_user_id rather than stored separately.
function resolvePlayer(game: GameRow, callerId: string): Player {
  if (game.host_user_id === callerId) {
    return "O";
  }
  if (game.invited_user_id === callerId) {
    return "X";
  }
  throw new NotAParticipantError();
}

function parseCoordinate(value: unknown): Coordinate {
  const candidate = value as Partial<Coordinate> | null;
  if (
    typeof candidate !== "object" ||
    candidate === null ||
    !Number.isInteger(candidate.row) ||
    !Number.isInteger(candidate.col)
  ) {
    throw new InvalidRequestError("coordinate must be an object with integer row and col.");
  }
  return { row: candidate.row as number, col: candidate.col as number };
}

export async function makeMove(supabase: SupabaseClient, params: MakeMoveParams): Promise<GameRow> {
  if (!params.gameId) {
    throw new InvalidRequestError("gameId is required.");
  }
  const coordinate = parseCoordinate(params.coordinate);

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
  engineMakeMove(game, player, coordinate);
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
