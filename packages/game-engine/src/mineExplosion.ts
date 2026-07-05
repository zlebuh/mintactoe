import type { Coordinate, Game, Player } from "./types.js";
import { getField } from "./grid.js";
import { isOnGrid } from "./coordinate.js";

// Erases the *triggering player's own* marks within minePower of the mine (the opponent's
// marks are left untouched - see docs/game-rules.md). Decrements surroundedByNotExplodedMines
// on every field in the blast radius except the mine cell itself, mines included, matching the
// C# engine exactly even though decrementing a mine's own counter has no other effect.
export function explodeMine(game: Game, player: Player, mineCoordinate: Coordinate): Coordinate[] {
  const minePower = game.rules.minePower;
  const affected: Coordinate[] = [];

  for (let row = mineCoordinate.row - minePower; row <= mineCoordinate.row + minePower; row++) {
    for (let col = mineCoordinate.col - minePower; col <= mineCoordinate.col + minePower; col++) {
      const candidate: Coordinate = { row, col };

      if (!isOnGrid(candidate, game.rules)) {
        continue;
      }
      if (candidate.row === mineCoordinate.row && candidate.col === mineCoordinate.col) {
        continue;
      }

      const field = getField(game.gameState.grid, candidate);

      if (!field.isMine) {
        if (field.player !== null) {
          affected.push(candidate);
          if (field.player === player) {
            field.player = null;
          }
        }
      }
      field.surroundedByNotExplodedMines--;
    }
  }

  return affected;
}
