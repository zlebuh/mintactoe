import { describe, expect, it } from "vitest";
import { initialize } from "./gameControl.js";
import { makeMove } from "./makeMove.js";
import { getField } from "./grid.js";
import type { Player } from "./types.js";

const noMines = () => 1;

describe("mine explosion (GameControlTests.PlacingAMoveThatCauseMineExplosion_PutsGameToCorrectState)", () => {
  it("erases only the triggering player's own marks; the mine keeps isMine and gets the triggering player's mark", () => {
    const game = initialize({ mineProbability: 0 });

    makeMove(game, "O", { row: 0, col: 0 }, noMines);
    makeMove(game, "X", { row: 0, col: 1 }, noMines);
    makeMove(game, "O", { row: 0, col: 2 }, noMines);
    makeMove(game, "X", { row: 1, col: 2 }, noMines);
    makeMove(game, "O", { row: 1, col: 0 }, noMines);
    makeMove(game, "X", { row: 2, col: 0 }, noMines);
    makeMove(game, "O", { row: 2, col: 1 }, noMines);
    makeMove(game, "X", { row: 2, col: 2 }, noMines);

    // mock, mirroring the C# test's direct field mutation (not achievable via legal moves alone)
    getField(game.gameState.grid, { row: 1, col: 1 }).isMine = true;
    for (let i = 0; i <= 2; i++) {
      for (let j = 0; j <= 2; j++) {
        if (i === 1 && j === 1) continue;
        getField(game.gameState.grid, { row: i, col: j }).surroundedByNotExplodedMines += 1;
      }
    }

    // OXO
    // OMX
    // XOX
    makeMove(game, "O", { row: 1, col: 1 }, noMines);
    // -X-
    // -MX
    // X-X

    expect(game.gameState.changes).toHaveLength(9); // 8 exploded fields + the mine cell itself

    const mine = getField(game.gameState.grid, { row: 1, col: 1 });
    expect(mine.isMine).toBe(true);
    expect(mine.player).toBe("O");

    const expected: Array<[number, number, number, Player | null]> = [
      [0, 0, 0, null], // O - erased (triggering player's own mark)
      [0, 1, 0, "X"], // X - survives (opponent's mark)
      [0, 2, 0, null], // O - erased
      [1, 0, 0, null], // O - erased
      [1, 2, 0, "X"], // X - survives
      [2, 0, 0, "X"], // X - survives
      [2, 1, 0, null], // O - erased
      [2, 2, 0, "X"], // X - survives
    ];
    for (const [row, col, expectedCount, expectedPlayer] of expected) {
      const field = getField(game.gameState.grid, { row, col });
      expect(field.surroundedByNotExplodedMines, `(${row},${col}) count`).toBe(expectedCount);
      expect(field.player, `(${row},${col}) player`).toBe(expectedPlayer);
    }
  });
});

describe("mine generation and counters (GameControlTests.SurroundingMinesChanges)", () => {
  it("only the directly-clicked field respects noMineMoves - neighbor-generated fields ignore it", () => {
    const game = initialize({ mineProbability: 1, noMineMoves: 2 });

    const coorO = { row: 10, col: 10 };
    makeMove(game, "O", coorO);
    const fO = getField(game.gameState.grid, coorO);
    expect(fO.isMine).toBe(false);
    expect(fO.surroundedByNotExplodedMines).toBe(8);

    const coorX = { row: 12, col: 12 };
    makeMove(game, "X", coorX);
    const fX = getField(game.gameState.grid, coorX);
    expect(fX.isMine).toBe(false);
    expect(fX.surroundedByNotExplodedMines).toBe(8);

    const coorO2 = { row: 11, col: 11 };
    makeMove(game, "O", coorO2);
    const fO2 = getField(game.gameState.grid, coorO2);
    expect(fO2.isMine).toBe(true);
    expect(fO2.player).toBe("O");
    expect(fO2.surroundedByNotExplodedMines).toBe(6);
    expect(fO.surroundedByNotExplodedMines).toBe(7);
    expect(fX.surroundedByNotExplodedMines).toBe(7);
  });
});

describe("noMineMoves (GameControlTests.Mines_NoMineMovesTest)", () => {
  it("guarantees the first noMineMoves directly-clicked cells are safe", () => {
    const game = initialize({ mineProbability: 1, noMineMoves: 2 });

    makeMove(game, "O", { row: 10, col: 10 });
    expect(getField(game.gameState.grid, { row: 10, col: 10 }).isMine).toBe(false);
    makeMove(game, "X", { row: 5, col: 5 });
    expect(getField(game.gameState.grid, { row: 5, col: 5 }).isMine).toBe(false);
    makeMove(game, "O", { row: 11, col: 10 });
    expect(getField(game.gameState.grid, { row: 11, col: 10 }).isMine).toBe(true);
    makeMove(game, "X", { row: 0, col: 0 });
    expect(getField(game.gameState.grid, { row: 0, col: 0 }).isMine).toBe(true);
  });
});

describe("mine explosion edge cases", () => {
  it("handles exploding with no own marks nearby to erase (GameControlTests.BombExplodedWithNoSurroundings)", () => {
    const game = initialize({ mineProbability: 1, noMineMoves: 2 });

    makeMove(game, "O", { row: 10, col: 10 });
    makeMove(game, "X", { row: 13, col: 13 });
    makeMove(game, "O", { row: 12, col: 12 });

    const f1010 = getField(game.gameState.grid, { row: 10, col: 10 });
    const f1313 = getField(game.gameState.grid, { row: 13, col: 13 });
    const f1212 = getField(game.gameState.grid, { row: 12, col: 12 });

    expect(f1010.player).toBe("O");
    expect(f1212.isMine).toBe(true);
    expect(f1212.player).toBe("O");
    expect(f1313.player).toBe("X");
  });

  it("erases the triggering player's own nearby mark, then allows the freed field to be overlaid (GameControlTests.BombExplodedAndErases)", () => {
    const game = initialize({ mineProbability: 1, noMineMoves: 2 });

    makeMove(game, "O", { row: 10, col: 10 });
    makeMove(game, "X", { row: 12, col: 12 });
    makeMove(game, "O", { row: 11, col: 11 });

    const f1010 = getField(game.gameState.grid, { row: 10, col: 10 });
    const f1111 = getField(game.gameState.grid, { row: 11, col: 11 });
    const f1212 = getField(game.gameState.grid, { row: 12, col: 12 });

    expect(f1010.player).toBeNull();
    expect(f1111.isMine).toBe(true);
    expect(f1111.player).toBe("O");
    expect(f1212.player).toBe("X");

    makeMove(game, "X", { row: 10, col: 10 });
    expect(getField(game.gameState.grid, { row: 10, col: 10 }).player).toBe("X");
  });
});
