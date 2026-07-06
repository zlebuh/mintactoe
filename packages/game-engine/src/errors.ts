import type { Coordinate, Player } from "./types.js";

export abstract class MinTacToeError extends Error {}

export class GameIsOverError extends MinTacToeError {
  constructor() {
    super("The game is already over.");
    this.name = "GameIsOverError";
  }
}

export class NotYourTurnError extends MinTacToeError {
  readonly playerOnTurn: Player;

  constructor(playerOnTurn: Player) {
    super(`${playerOnTurn} is on turn.`);
    this.name = "NotYourTurnError";
    this.playerOnTurn = playerOnTurn;
  }
}

export class CoordinateOutOfGridError extends MinTacToeError {
  readonly coordinate: Coordinate;
  readonly rows: number;
  readonly columns: number;

  constructor(coordinate: Coordinate, rows: number, columns: number) {
    super(
      `Coordinate (${coordinate.row}, ${coordinate.col}) is out of grid bounds. ` +
        `Rows: ${rows}, Columns: ${columns}`,
    );
    this.name = "CoordinateOutOfGridError";
    this.coordinate = coordinate;
    this.rows = rows;
    this.columns = columns;
  }
}

export class FieldOccupiedError extends MinTacToeError {
  readonly occupiedCoordinate: Coordinate;
  readonly occupyingPlayer: Player;

  constructor(player: Player, coordinate: Coordinate) {
    super(`Field (0-based) [r${coordinate.row}, c${coordinate.col}] is occupied by player: ${player}.`);
    this.name = "FieldOccupiedError";
    this.occupiedCoordinate = coordinate;
    this.occupyingPlayer = player;
  }
}

export class GameSerializationError extends MinTacToeError {
  constructor(message = "Failed to (de)serialize game state.") {
    super(message);
    this.name = "GameSerializationError";
  }
}
