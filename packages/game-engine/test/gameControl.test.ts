import { describe, expect, it } from "vitest";
import { initialize } from "../src/gameControl.js";
import { isOnGrid } from "../src/coordinate.js";
import { getField } from "../src/grid.js";

describe("initialize", () => {
  it("starts with no moves played and O on turn", () => {
    const game = initialize();
    expect(game.gameState.movesPlayed).toBe(0);
    expect(game.gameState.playerOnTurn).toBe("O");
  });

  it("pre-allocates every field in the grid, all unoccupied", () => {
    const game = initialize();
    for (let row = 0; row < game.rules.rows; row++) {
      for (let col = 0; col < game.rules.columns; col++) {
        expect(getField(game.gameState.grid, { row, col }).player).toBeNull();
      }
    }
  });
});

describe("isOnGrid", () => {
  it("treats a coordinate as on-grid only when both row and col are within [0, rows) / [0, columns)", () => {
    const game = initialize();
    expect(isOnGrid({ row: 10, col: 10 }, game.rules)).toBe(true);
    expect(isOnGrid({ row: 0, col: 0 }, game.rules)).toBe(true);
    expect(isOnGrid({ row: 5, col: 15 }, game.rules)).toBe(true);
    expect(isOnGrid({ row: game.rules.rows, col: game.rules.columns }, game.rules)).toBe(false);
    expect(isOnGrid({ row: -1, col: -1 }, game.rules)).toBe(false);
  });
});
