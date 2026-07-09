// These mirror packages/game-engine/src/types.ts and serialization.ts exactly (the wire
// format is frozen - see docs/game-rules.md) rather than importing them from there: Deno's
// checker can't resolve a `type`-only export re-exported through a .js specifier back to its
// real declaration (it infers loosely from the plain compiled JS instead), and the deployed
// edge-runtime can't resolve packages/game-engine/src directly - it graph-walks every import
// regardless of the `type` keyword, and that source uses .js-specifiers pointing at sibling
// .ts files (fine for Node/Vite's "Bundler" resolution, not for plain Deno). Only
// packages/game-engine/dist's compiled value exports cross that boundary cleanly (see
// supabase/functions/deno.json and the handler.ts files that import from dist/index.js).
export type Player = "O" | "X";

export interface Coordinate {
  row: number;
  col: number;
}

export interface Field {
  player: Player | null;
  surroundedByNotExplodedMines: number;
  isMine: boolean;
  generated: boolean;
  hasAllNeighboursGenerated: boolean;
}

export interface Rules {
  rows: number;
  columns: number;
  seriesLength: number;
  noMineMoves: number;
  minePower: number;
  mineProbability: number;
}

export interface SerializedGame {
  rules: Rules;
  gameState: {
    grid: Record<string, Field>;
    isGameOver: boolean;
    winner: Player | null;
    playerOnTurn: Player | null;
    changes: Coordinate[];
    movesPlayed: number;
  };
}

// Shape of a row in `public.games` (see supabase/migrations/20260704220000_games_schema.sql).
export interface GameRow {
  id: string;
  created_at: string;
  updated_at: string;
  host_user_id: string;
  invited_user_id: string | null;
  game_state: SerializedGame;
}

export function isUnfinished(row: GameRow): boolean {
  return row.game_state.gameState.isGameOver === false;
}
