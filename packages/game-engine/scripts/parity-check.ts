// One-off parity check, run manually during the C# -> TypeScript engine port (see issue #5).
// Not wired into CI: it depends on a frozen snapshot from the C# engine, which is deleted at
// final cutover (issue #11). The ongoing regression suite going forward is the Vitest suite
// in this package - this script's only job was to catch behavioral drift during the port.
//
// csharp-parity-fixture.json was captured by running the same scenarios below through the real
// C# engine (a throwaway console harness referencing Zlebuh.MinTacToe.GameEngine directly).
// Run with: pnpm --filter @mintactoe/game-engine parity-check

import { readFileSync } from "node:fs";
import { fileURLToPath } from "node:url";
import { dirname, join } from "node:path";
import { isDeepStrictEqual } from "node:util";
import { initialize } from "../src/gameControl.js";
import { makeMove } from "../src/makeMove.js";
import type { Player, Rules } from "../src/types.js";

const __dirname = dirname(fileURLToPath(import.meta.url));

interface MoveSpec {
  player: Player;
  row: number;
  col: number;
}

interface Scenario {
  name: string;
  rules: Partial<Rules>;
  moves: MoveSpec[];
}

// Every scenario uses mineProbability 0 or 1 - the random draw's value literally cannot change
// the outcome at either boundary (`x < 0` is always false, `x < 1` is always true for any x in
// [0,1)), so this is fully deterministic despite using the real, unseeded Math.random.
const scenarios: Scenario[] = [
  {
    name: "surrounding-mines-changes",
    rules: { mineProbability: 1, noMineMoves: 2 },
    moves: [
      { player: "O", row: 10, col: 10 },
      { player: "X", row: 12, col: 12 },
      { player: "O", row: 11, col: 11 },
    ],
  },
  {
    name: "no-mine-moves",
    rules: { mineProbability: 1, noMineMoves: 2 },
    moves: [
      { player: "O", row: 10, col: 10 },
      { player: "X", row: 5, col: 5 },
      { player: "O", row: 11, col: 10 },
      { player: "X", row: 0, col: 0 },
    ],
  },
  {
    name: "bomb-exploded-with-no-surroundings",
    rules: { mineProbability: 1, noMineMoves: 2 },
    moves: [
      { player: "O", row: 10, col: 10 },
      { player: "X", row: 13, col: 13 },
      { player: "O", row: 12, col: 12 },
    ],
  },
  {
    name: "bomb-exploded-and-erases",
    rules: { mineProbability: 1, noMineMoves: 2 },
    moves: [
      { player: "O", row: 10, col: 10 },
      { player: "X", row: 12, col: 12 },
      { player: "O", row: 11, col: 11 },
      { player: "X", row: 10, col: 10 },
    ],
  },
  {
    name: "horizontal-win-3x3",
    rules: { rows: 3, columns: 3, seriesLength: 3, mineProbability: 0 },
    moves: [
      { player: "O", row: 0, col: 0 },
      { player: "X", row: 1, col: 0 },
      { player: "O", row: 0, col: 1 },
      { player: "X", row: 1, col: 1 },
      { player: "O", row: 0, col: 2 },
    ],
  },
  {
    name: "diagonal-win",
    rules: { mineProbability: 0 },
    moves: [
      { player: "O", row: 0, col: 0 },
      { player: "X", row: 10, col: 10 },
      { player: "O", row: 0, col: 1 },
      { player: "X", row: 11, col: 11 },
      { player: "O", row: 0, col: 2 },
      { player: "X", row: 12, col: 12 },
      { player: "O", row: 0, col: 3 },
      { player: "X", row: 13, col: 13 },
      { player: "O", row: 0, col: 10 },
      { player: "X", row: 14, col: 14 },
    ],
  },
  {
    name: "tie-3x3",
    rules: { rows: 3, columns: 3, seriesLength: 3, mineProbability: 0 },
    moves: [
      { player: "O", row: 0, col: 0 },
      { player: "X", row: 1, col: 1 },
      { player: "O", row: 0, col: 1 },
      { player: "X", row: 0, col: 2 },
      { player: "O", row: 2, col: 0 },
      { player: "X", row: 1, col: 0 },
      { player: "O", row: 1, col: 2 },
      { player: "X", row: 2, col: 1 },
      { player: "O", row: 2, col: 2 },
    ],
  },
];

const fixturePath = join(__dirname, "csharp-parity-fixture.json");
const csharpResults: Record<string, unknown> = JSON.parse(readFileSync(fixturePath, "utf-8"));

let mismatches = 0;

for (const scenario of scenarios) {
  const game = initialize(scenario.rules);
  for (const move of scenario.moves) {
    makeMove(game, move.player, { row: move.row, col: move.col });
  }

  const grid: Record<string, unknown> = {};
  for (const [key, field] of game.gameState.grid) {
    const isDefault =
      field.player === null &&
      field.surroundedByNotExplodedMines === 0 &&
      !field.isMine &&
      !field.generated &&
      !field.hasAllNeighboursGenerated;
    if (!isDefault) {
      grid[key] = field;
    }
  }

  const tsResult = {
    movesPlayed: game.gameState.movesPlayed,
    isGameOver: game.gameState.isGameOver,
    winner: game.gameState.winner,
    playerOnTurn: game.gameState.playerOnTurn,
    grid,
  };

  const csharpResult = csharpResults[scenario.name];

  // Structural comparison, not JSON.stringify string comparison - property insertion order
  // differs between the C# harness's anonymous objects and these TS object literals, which
  // would otherwise produce false-positive mismatches despite identical field values.
  if (!isDeepStrictEqual(csharpResult, tsResult)) {
    mismatches++;
    console.error(`MISMATCH in scenario "${scenario.name}"`);
    console.error("  C#:", JSON.stringify(csharpResult));
    console.error("  TS:", JSON.stringify(tsResult));
  } else {
    console.log(`OK   ${scenario.name}`);
  }
}

if (mismatches > 0) {
  console.error(`\n${mismatches} scenario(s) diverged from the C# engine.`);
  process.exit(1);
} else {
  console.log(`\nAll ${scenarios.length} scenarios match the C# engine exactly.`);
}
