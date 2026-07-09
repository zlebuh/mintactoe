import type { Coordinate, Game, Player } from "./types.js";
import { getField } from "./grid.js";
import { isOnGrid, neighborDirections } from "./coordinate.js";

export function checkPlayerWins(game: Game, player: Player, coordinate: Coordinate): boolean {
  if (game.rules.seriesLength === 1) {
    return true;
  }

  // One counter per axis (0-3), starting at 1 to count the just-placed cell itself.
  const masterDirectionSums = [1, 1, 1, 1];

  for (const direction of neighborDirections()) {
    let row = coordinate.row;
    let col = coordinate.col;

    for (;;) {
      row += direction.offset.row;
      col += direction.offset.col;
      const candidate: Coordinate = { row, col };

      if (!isOnGrid(candidate, game.rules)) {
        break;
      }

      const field = getField(game.gameState.grid, candidate);
      if (!field.generated || field.isMine) {
        break;
      }

      if (field.player === player) {
        masterDirectionSums[direction.masterDirection]++;
        if (masterDirectionSums[direction.masterDirection] === game.rules.seriesLength) {
          return true;
        }
      } else {
        break;
      }
    }
  }

  return false;
}

export function checkTie(game: Game): boolean {
  for (let row = 0; row < game.rules.rows; row++) {
    for (let col = 0; col < game.rules.columns; col++) {
      const field = getField(game.gameState.grid, { row, col });
      if (!field.isMine && field.player === null) {
        return false;
      }
    }
  }
  return true;
}
