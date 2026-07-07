import type { Coordinate, Game, Player } from "./types.js";
import { getField } from "./grid.js";
import { allNeighbors, isOnGrid } from "./coordinate.js";
import { checkPlayerWins, checkTie } from "./gameOverChecks.js";
import { explodeMine } from "./mineExplosion.js";
import {
  CoordinateOutOfGridError,
  FieldOccupiedError,
  GameIsOverError,
  NotYourTurnError,
} from "./errors.js";

/**
 * Mutates `game` in place.
 *
 * `random` defaults to `Math.random` (unseeded) and is injectable for deterministic tests.
 *
 * Non-obvious rule (see docs/game-rules.md): only the *directly clicked* field respects
 * `noMineMoves` - fields generated as a side effect of being someone else's neighbor can
 * become mines from move 1 onward, regardless of `noMineMoves`.
 */
export function makeMove(
  game: Game,
  player: Player,
  coordinate: Coordinate,
  random: () => number = Math.random,
): void {
  if (game.gameState.isGameOver || game.gameState.playerOnTurn === null) {
    throw new GameIsOverError();
  }
  if (game.gameState.playerOnTurn !== player) {
    throw new NotYourTurnError(game.gameState.playerOnTurn);
  }
  if (!isOnGrid(coordinate, game.rules)) {
    throw new CoordinateOutOfGridError(coordinate, game.rules.rows, game.rules.columns);
  }

  const field = getField(game.gameState.grid, coordinate);
  if (field.player !== null) {
    throw new FieldOccupiedError(field.player, coordinate);
  }

  if (!field.generated) {
    field.isMine =
      game.gameState.movesPlayed >= game.rules.noMineMoves && random() < game.rules.mineProbability;
    field.generated = true;
  }

  if (!field.hasAllNeighboursGenerated) {
    for (const neighborCoordinate of allNeighbors(coordinate)) {
      if (!isOnGrid(neighborCoordinate, game.rules)) {
        continue;
      }
      const neighbor = getField(game.gameState.grid, neighborCoordinate);
      if (!neighbor.generated) {
        neighbor.isMine = random() < game.rules.mineProbability;
        neighbor.generated = true;
      }
      if (neighbor.isMine) {
        field.surroundedByNotExplodedMines++;
      }
    }
    field.hasAllNeighboursGenerated = true;
  }

  const changedCoordinates: Coordinate[] = [];
  if (field.isMine) {
    changedCoordinates.push(...explodeMine(game, player, coordinate));
  }
  changedCoordinates.push(coordinate);
  field.player = player;

  // An exploded mine cell is still marked with the triggering player (matches the C# original -
  // see GameMakeMove.cs), but it must not count as a win for them: checkPlayerWins() always
  // credits the just-placed coordinate as "1" without re-checking whether *it* is a mine (it
  // only checks isMine while walking outward to neighbors), so a move that only detonates a
  // mine needs to skip the win check entirely rather than relying on that function to reject it.
  const playerWins = !field.isMine && checkPlayerWins(game, player, coordinate);
  const isTie = checkTie(game);
  const gameOver = playerWins || isTie;

  game.gameState.isGameOver = gameOver;
  game.gameState.winner = playerWins ? player : null;
  game.gameState.playerOnTurn = gameOver ? null : player === "O" ? "X" : "O";
  game.gameState.changes = changedCoordinates;
  game.gameState.movesPlayed++;
}
