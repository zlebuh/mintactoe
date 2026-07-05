import { describe, expect, it } from "vitest";
import { initialize } from "./gameControl.js";
import { makeMove } from "./makeMove.js";
import { getField } from "./grid.js";
import {
  CoordinateOutOfGridError,
  FieldOccupiedError,
  GameIsOverError,
  NotYourTurnError,
} from "./errors.js";

// `random() < mineProbability` with mineProbability 0: never true, regardless of what
// `random()` returns. Used throughout to keep move sequences deterministic.
const noMines = () => 1;

describe("makeMove (GameControlTests.PlacingAMove)", () => {
  it("places a mark, alternates turn, and records the change", () => {
    const game = initialize({ mineProbability: 0 });

    makeMove(game, "O", { row: 0, col: 0 }, noMines);
    expect(game.gameState.playerOnTurn).toBe("X");
    expect(game.gameState.changes).toEqual([{ row: 0, col: 0 }]);
    expect(getField(game.gameState.grid, { row: 0, col: 0 }).player).toBe("O");

    makeMove(game, "X", { row: 1, col: 0 }, noMines);
    expect(game.gameState.playerOnTurn).toBe("O");
    expect(game.gameState.changes).toEqual([{ row: 1, col: 0 }]);
    expect(getField(game.gameState.grid, { row: 1, col: 0 }).player).toBe("X");
  });

  it("throws CoordinateOutOfGridError for an out-of-bounds move", () => {
    const game = initialize();
    expect(() => makeMove(game, "O", { row: 40, col: 0 })).toThrow(CoordinateOutOfGridError);
  });

  it("throws NotYourTurnError when it isn't the caller's turn", () => {
    const game = initialize();
    expect(() => makeMove(game, "X", { row: 0, col: 0 })).toThrow(NotYourTurnError);
  });

  it("throws FieldOccupiedError for an already-occupied field", () => {
    const game = initialize();
    makeMove(game, "O", { row: 0, col: 0 }, noMines);
    expect(() => makeMove(game, "X", { row: 0, col: 0 }, noMines)).toThrow(FieldOccupiedError);
  });

  it("throws GameIsOverError once the game has ended", () => {
    const game = initialize({ seriesLength: 2, mineProbability: 0 });
    makeMove(game, "O", { row: 0, col: 0 }, noMines);
    makeMove(game, "X", { row: 0, col: 1 }, noMines);
    makeMove(game, "O", { row: 1, col: 0 }, noMines);
    expect(game.gameState.isGameOver).toBe(true);
    expect(() => makeMove(game, "X", { row: 1, col: 1 }, noMines)).toThrow(GameIsOverError);
  });
});
