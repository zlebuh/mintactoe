import { describe, expect, it } from "vitest";
import { initialize } from "../src/gameControl.js";
import { makeMove } from "../src/makeMove.js";
import { getField } from "../src/grid.js";
import { deserializeGame, serializeGame } from "../src/serialization.js";
import { GameSerializationError } from "../src/errors.js";

const noMines = () => 1;

describe("serializeGame / deserializeGame", () => {
  it("round-trips a game with real move history", () => {
    const game = initialize({ rows: 5, columns: 5, mineProbability: 0 });
    makeMove(game, "O", { row: 0, col: 0 }, noMines);
    makeMove(game, "X", { row: 1, col: 2 }, noMines);

    const json = JSON.stringify(serializeGame(game));
    const restored = deserializeGame(json);

    expect(restored.gameState.isGameOver).toBe(game.gameState.isGameOver);
    expect(restored.gameState.winner).toBe(game.gameState.winner);
    expect(restored.gameState.playerOnTurn).toBe(game.gameState.playerOnTurn);
    expect(restored.gameState.movesPlayed).toBe(2);
    expect(getField(restored.gameState.grid, { row: 0, col: 0 }).player).toBe("O");
    expect(getField(restored.gameState.grid, { row: 1, col: 2 }).player).toBe("X");
    expect(restored.gameState.changes).toEqual(game.gameState.changes);
    expect(restored.rules).toEqual(game.rules);
  });

  it("omits untouched fields from the serialized grid but reconstructs them as defaults", () => {
    const game = initialize({ rows: 5, columns: 5, mineProbability: 0 });
    makeMove(game, "O", { row: 0, col: 0 }, noMines);

    const serialized = serializeGame(game);
    // 5x5 = 25 cells; one placed move generates itself + up to 8 neighbors, so well under 25
    // non-default entries should be written.
    expect(Object.keys(serialized.gameState.grid).length).toBeLessThan(25);

    const restored = deserializeGame(serialized);
    expect(getField(restored.gameState.grid, { row: 4, col: 4 }).generated).toBe(false);
  });

  it("preserves a null winner and the starting player on turn", () => {
    const game = initialize({ mineProbability: 0 });
    const restored = deserializeGame(serializeGame(game));
    expect(restored.gameState.winner).toBeNull();
    expect(restored.gameState.playerOnTurn).toBe("O");
  });

  it("throws GameSerializationError for an empty or blank string", () => {
    expect(() => deserializeGame("")).toThrow(GameSerializationError);
    expect(() => deserializeGame("   ")).toThrow(GameSerializationError);
  });

  it("throws GameSerializationError for malformed JSON", () => {
    expect(() => deserializeGame("{not valid json")).toThrow(GameSerializationError);
  });

  it("throws GameSerializationError when rules or gameState are missing", () => {
    expect(() => deserializeGame(JSON.stringify({}))).toThrow(GameSerializationError);
  });

  it("defaults to an empty grid/changes when a permissively-parsed gameState omits them", () => {
    const game = initialize({ rows: 3, columns: 3, mineProbability: 0 });
    const restored = deserializeGame({
      rules: game.rules,
      // grid/changes deliberately omitted, as if reading state from an older/partial record
      gameState: { isGameOver: false, winner: null, playerOnTurn: "O", movesPlayed: 0 } as never,
    });
    expect(restored.gameState.changes).toEqual([]);
    expect(getField(restored.gameState.grid, { row: 0, col: 0 }).generated).toBe(false);
  });
});
