import { describe, expect, it } from "vitest";
import { initialize } from "../src/gameControl.js";
import { makeMove } from "../src/makeMove.js";
import { getField } from "../src/grid.js";

const noMines = () => 1;

describe("win detection", () => {
  it("detects a horizontal win (GameControlTests.GamePlayed_OWins)", () => {
    const game = initialize({ rows: 3, columns: 3, seriesLength: 3, mineProbability: 0 });
    makeMove(game, "O", { row: 0, col: 0 }, noMines);
    makeMove(game, "X", { row: 1, col: 0 }, noMines);
    makeMove(game, "O", { row: 0, col: 1 }, noMines);
    makeMove(game, "X", { row: 1, col: 1 }, noMines);
    makeMove(game, "O", { row: 0, col: 2 }, noMines);

    expect(game.gameState.isGameOver).toBe(true);
    expect(game.gameState.winner).toBe("O");
    expect(game.gameState.playerOnTurn).toBeNull();
  });

  it("detects a diagonal win across the default 20x20 board (GameControlTests.GamePlayed_XWinsDiagonally)", () => {
    const game = initialize({ mineProbability: 0 });
    makeMove(game, "O", { row: 0, col: 0 }, noMines);
    makeMove(game, "X", { row: 10, col: 10 }, noMines);
    makeMove(game, "O", { row: 0, col: 1 }, noMines);
    makeMove(game, "X", { row: 11, col: 11 }, noMines);
    makeMove(game, "O", { row: 0, col: 2 }, noMines);
    makeMove(game, "X", { row: 12, col: 12 }, noMines);
    makeMove(game, "O", { row: 0, col: 3 }, noMines);
    makeMove(game, "X", { row: 13, col: 13 }, noMines);
    makeMove(game, "O", { row: 0, col: 10 }, noMines);
    makeMove(game, "X", { row: 14, col: 14 }, noMines);

    expect(game.gameState.isGameOver).toBe(true);
    expect(game.gameState.winner).toBe("X");
    expect(game.gameState.playerOnTurn).toBeNull();
  });

  it("wins when the last move fills the only remaining gap in a run (GameIsOverCheckTests.LastMissingPlacedInside)", () => {
    const game = initialize({ rows: 10, columns: 10, seriesLength: 5, mineProbability: 0 });
    makeMove(game, "O", { row: 0, col: 0 }, noMines);
    makeMove(game, "X", { row: 1, col: 0 }, noMines);
    makeMove(game, "O", { row: 0, col: 1 }, noMines);
    makeMove(game, "X", { row: 2, col: 0 }, noMines);
    makeMove(game, "O", { row: 0, col: 2 }, noMines);
    makeMove(game, "X", { row: 3, col: 0 }, noMines);
    makeMove(game, "O", { row: 0, col: 4 }, noMines);
    makeMove(game, "X", { row: 4, col: 0 }, noMines);
    makeMove(game, "O", { row: 0, col: 3 }, noMines);

    expect(game.gameState.isGameOver).toBe(true);
    expect(game.gameState.winner).toBe("O");
  });

  it("wins on the very first move when seriesLength is 1", () => {
    // seriesLength: 1 is a documented edge case in the C# engine (CheckPlayerWins returns
    // true unconditionally) - not exercised by the legacy C# test suite, but worth locking in
    // since it's a one-line special case easy to silently drop during a port.
    const game = initialize({ seriesLength: 1, mineProbability: 0 });
    makeMove(game, "O", { row: 0, col: 0 }, noMines);
    expect(game.gameState.isGameOver).toBe(true);
    expect(game.gameState.winner).toBe("O");
  });
});

describe("tie detection", () => {
  it("ties when the whole board fills with no winner (GameControlTests.GamePlayed_Tie)", () => {
    const game = initialize({ rows: 3, columns: 3, seriesLength: 3, mineProbability: 0 });
    makeMove(game, "O", { row: 0, col: 0 }, noMines);
    makeMove(game, "X", { row: 1, col: 1 }, noMines);
    makeMove(game, "O", { row: 0, col: 1 }, noMines);
    makeMove(game, "X", { row: 0, col: 2 }, noMines);
    makeMove(game, "O", { row: 2, col: 0 }, noMines);
    makeMove(game, "X", { row: 1, col: 0 }, noMines);
    makeMove(game, "O", { row: 1, col: 2 }, noMines);
    makeMove(game, "X", { row: 2, col: 1 }, noMines);
    makeMove(game, "O", { row: 2, col: 2 }, noMines);

    expect(game.gameState.isGameOver).toBe(true);
    expect(game.gameState.winner).toBeNull();
    expect(game.gameState.playerOnTurn).toBeNull();
  });

  it("ties when a mocked mine occupies the last empty cell (GameControlTests.GamePlayed_WithBomb_Tie)", () => {
    const game = initialize({ rows: 3, columns: 3, seriesLength: 3, mineProbability: 0 });
    makeMove(game, "O", { row: 2, col: 1 }, noMines);
    makeMove(game, "X", { row: 1, col: 1 }, noMines);
    getField(game.gameState.grid, { row: 0, col: 0 }).isMine = true;
    makeMove(game, "O", { row: 2, col: 0 }, noMines);
    makeMove(game, "X", { row: 2, col: 2 }, noMines);
    makeMove(game, "O", { row: 1, col: 0 }, noMines);
    makeMove(game, "X", { row: 1, col: 2 }, noMines);
    makeMove(game, "O", { row: 0, col: 2 }, noMines);
    makeMove(game, "X", { row: 0, col: 1 }, noMines);

    expect(game.gameState.isGameOver).toBe(true);
    expect(game.gameState.winner).toBeNull();
    expect(game.gameState.playerOnTurn).toBeNull();
  });

  it("ties after a mine explosion leaves a cell empty that later gets overlaid (GameControlTests.GamePlayed_WithExplodedBomb_Tie)", () => {
    const game = initialize({ rows: 3, columns: 3, seriesLength: 3, mineProbability: 0 });
    makeMove(game, "O", { row: 1, col: 0 }, noMines);
    makeMove(game, "X", { row: 0, col: 1 }, noMines);
    getField(game.gameState.grid, { row: 0, col: 0 }).isMine = true;
    makeMove(game, "O", { row: 0, col: 0 }, noMines);

    const mine = getField(game.gameState.grid, { row: 0, col: 0 });
    expect(mine.isMine).toBe(true);
    expect(mine.player).toBe("O");
    const erased = getField(game.gameState.grid, { row: 1, col: 0 });
    expect(erased.isMine).toBe(false);
    expect(erased.player).toBeNull();

    makeMove(game, "X", { row: 1, col: 1 }, noMines);
    makeMove(game, "O", { row: 2, col: 1 }, noMines);
    makeMove(game, "X", { row: 1, col: 0 }, noMines); // overlay
    expect(getField(game.gameState.grid, { row: 1, col: 0 }).player).toBe("X");
    makeMove(game, "O", { row: 1, col: 2 }, noMines);
    makeMove(game, "X", { row: 2, col: 0 }, noMines);
    makeMove(game, "O", { row: 0, col: 2 }, noMines);
    makeMove(game, "X", { row: 2, col: 2 }, noMines);

    expect(game.gameState.isGameOver).toBe(true);
    expect(game.gameState.winner).toBeNull();
    expect(game.gameState.playerOnTurn).toBeNull();
  });
});
