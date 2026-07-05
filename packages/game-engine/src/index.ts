export type { Coordinate, Field, Game, GameState, Grid, Player, Rules } from "./types.js";
export { DEFAULT_RULES } from "./types.js";

export { initialize } from "./gameControl.js";
export { makeMove } from "./makeMove.js";
export { checkPlayerWins, checkTie } from "./gameOverChecks.js";
export { explodeMine } from "./mineExplosion.js";
export { allNeighbors, coordinateKey, isOnGrid, neighborDirections } from "./coordinate.js";
export { createDefaultField, createGrid, getField } from "./grid.js";
export { deserializeGame, serializeGame, type SerializedGame } from "./serialization.js";
export {
  CoordinateOutOfGridError,
  FieldOccupiedError,
  GameIsOverError,
  GameSerializationError,
  MinTacToeError,
  NotYourTurnError,
} from "./errors.js";
