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

// Default rules for a new game. 16x16 matches the actual production board size (see
// docs/game-rules.md) - it's the engine's own default rather than something every caller has
// to override separately.
export const DEFAULT_RULES: Rules = {
  rows: 16,
  columns: 16,
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
