import type { Coordinate, Field, Grid, Rules } from "./types.js";
import { coordinateKey } from "./coordinate.js";

export function createDefaultField(): Field {
  return {
    player: null,
    surroundedByNotExplodedMines: 0,
    isMine: false,
    generated: false,
    hasAllNeighboursGenerated: false,
  };
}

// Every coordinate in bounds is allocated up front (mirrors GameControl.Initialize in the C#
// engine, which pre-allocates the full rows*columns dictionary). CheckTie already scans the
// entire board on every move, so a "lazy/sparse" in-memory grid would densify on the first move
// anyway - pre-allocating is simpler and removes an entire class of "was this coordinate ever
// touched" bugs. Sparseness is instead applied at the serialization boundary (see serialization.ts).
export function createGrid(rules: Rules): Grid {
  const grid: Grid = new Map();
  for (let row = 0; row < rules.rows; row++) {
    for (let col = 0; col < rules.columns; col++) {
      grid.set(coordinateKey({ row, col }), createDefaultField());
    }
  }
  return grid;
}

export function getField(grid: Grid, coordinate: Coordinate): Field {
  const field = grid.get(coordinateKey(coordinate));
  if (!field) {
    // Unreachable in practice: every caller checks isOnGrid() before calling getField(), and
    // createGrid() pre-allocates every in-bounds coordinate. Kept as a defensive invariant check
    // rather than a silent `!` non-null assertion, in case that invariant is ever violated.
    /* v8 ignore next */
    throw new Error(`Field at ${coordinateKey(coordinate)} was not initialized`);
  }
  return field;
}
