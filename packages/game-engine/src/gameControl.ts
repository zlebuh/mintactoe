import type { Game, Rules } from "./types.js";
import { DEFAULT_RULES } from "./types.js";
import { createGrid } from "./grid.js";

export function initialize(rules: Partial<Rules> = {}): Game {
  const resolvedRules: Rules = { ...DEFAULT_RULES, ...rules };
  return {
    rules: resolvedRules,
    gameState: {
      grid: createGrid(resolvedRules),
      isGameOver: false,
      winner: null,
      playerOnTurn: "O",
      changes: [],
      movesPlayed: 0,
    },
  };
}
