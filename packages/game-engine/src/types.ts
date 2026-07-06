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

// Default rules for a new game. Production games override rows/columns to 16x16 at the API
// layer (see docs/game-rules.md) - that's a caller concern, not an engine default.
export const DEFAULT_RULES: Rules = {
  rows: 20,
  columns: 20,
  seriesLength: 5,
  noMineMoves: 6,
  minePower: 1,
  mineProbability: 0.1,
};

export type Grid = Map<string, Field>;

export interface GameState {
  grid: Grid;
  isGameOver: boolean;
  winner: Player | null;
  playerOnTurn: Player | null;
  changes: Coordinate[];
  movesPlayed: number;
}

export interface Game {
  gameState: GameState;
  rules: Rules;
}
