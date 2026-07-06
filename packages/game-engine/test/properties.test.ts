import { describe, expect, it } from "vitest";
import fc from "fast-check";
import { initialize } from "../src/gameControl.js";
import { makeMove } from "../src/makeMove.js";
import { explodeMine } from "../src/mineExplosion.js";
import { getField } from "../src/grid.js";
import { allNeighbors } from "../src/coordinate.js";
import type { Player } from "../src/types.js";

const playerArb = fc.constantFrom<Player>("O", "X");
const cellArb = fc.oneof(fc.constant(null), playerArb);

describe("explodeMine (property-based)", () => {
  it("never erases the opponent's marks, always erases the triggering player's own marks, and decrements every affected counter by exactly 1", () => {
    fc.assert(
      fc.property(
        playerArb,
        fc.array(cellArb, { minLength: 8, maxLength: 8 }),
        fc.array(fc.integer({ min: 0, max: 10 }), { minLength: 8, maxLength: 8 }),
        (triggeringPlayer, neighborMarks, neighborCounts) => {
          const game = initialize({ mineProbability: 0, minePower: 1 });
          const center = { row: 10, col: 10 };
          getField(game.gameState.grid, center).isMine = true;

          const neighbors = allNeighbors(center);
          neighbors.forEach((coordinate, i) => {
            const field = getField(game.gameState.grid, coordinate);
            field.player = neighborMarks[i]!;
            field.surroundedByNotExplodedMines = neighborCounts[i]!;
          });

          explodeMine(game, triggeringPlayer, center);

          neighbors.forEach((coordinate, i) => {
            const field = getField(game.gameState.grid, coordinate);
            const originalMark = neighborMarks[i];
            if (originalMark !== null && originalMark !== triggeringPlayer) {
              expect(field.player).toBe(originalMark);
            }
            if (originalMark === triggeringPlayer) {
              expect(field.player).toBeNull();
            }
            expect(field.surroundedByNotExplodedMines).toBe(neighborCounts[i]! - 1);
          });

          const mineField = getField(game.gameState.grid, center);
          expect(mineField.isMine).toBe(true);
        },
      ),
      { numRuns: 200 },
    );
  });
});

describe("makeMove (property-based)", () => {
  it("increments movesPlayed by exactly one and alternates turn, for any order of moves, until the game ends", () => {
    const allCoordinates = Array.from({ length: 25 }, (_, i) => ({
      row: Math.floor(i / 5),
      col: i % 5,
    }));

    fc.assert(
      fc.property(fc.shuffledSubarray(allCoordinates, { minLength: 25, maxLength: 25 }), (coordinates) => {
        // seriesLength higher than the board can ever hold: a win is impossible, so the game can
        // only end by tie once the board fills - isolates the turn/move-count invariant from
        // win-detection entirely.
        const game = initialize({ rows: 5, columns: 5, seriesLength: 100, mineProbability: 0 });
        let expectedPlayer: Player = "O";
        let movesMade = 0;

        for (const coordinate of coordinates) {
          if (game.gameState.isGameOver) break;
          makeMove(game, expectedPlayer, coordinate, () => 1);
          movesMade++;
          expect(game.gameState.movesPlayed).toBe(movesMade);
          expectedPlayer = expectedPlayer === "O" ? "X" : "O";
        }

        expect(game.gameState.isGameOver).toBe(true);
        expect(game.gameState.winner).toBeNull();
      }),
      { numRuns: 50 },
    );
  });
});
