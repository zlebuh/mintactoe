import type { Coordinate, Rules } from "./types.js";

export function coordinateKey(coordinate: Coordinate): string {
  return `${coordinate.row},${coordinate.col}`;
}

export function isOnGrid(coordinate: Coordinate, rules: Rules): boolean {
  return (
    rules.rows > coordinate.row &&
    coordinate.row >= 0 &&
    rules.columns > coordinate.col &&
    coordinate.col >= 0
  );
}

export interface NeighborDirection {
  offset: Coordinate;
  // 0 = vertical (up/down), 1 = horizontal (left/right), 2 = diagonal down-right
  // (up-left/down-right), 3 = diagonal down-left (up-right/down-left). Opposite directions
  // share a masterDirection so win-checking can walk both ways along one axis and sum them.
  masterDirection: 0 | 1 | 2 | 3;
}

const NEIGHBOR_DIRECTIONS: readonly NeighborDirection[] = [
  { offset: { row: -1, col: 0 }, masterDirection: 0 }, // up
  { offset: { row: 1, col: 0 }, masterDirection: 0 }, // down
  { offset: { row: 0, col: -1 }, masterDirection: 1 }, // left
  { offset: { row: 0, col: 1 }, masterDirection: 1 }, // right
  { offset: { row: -1, col: -1 }, masterDirection: 2 }, // up-left
  { offset: { row: 1, col: 1 }, masterDirection: 2 }, // down-right
  { offset: { row: -1, col: 1 }, masterDirection: 3 }, // up-right
  { offset: { row: 1, col: -1 }, masterDirection: 3 }, // down-left
];

export function neighborDirections(): readonly NeighborDirection[] {
  return NEIGHBOR_DIRECTIONS;
}

export function allNeighbors(coordinate: Coordinate): Coordinate[] {
  return NEIGHBOR_DIRECTIONS.map((d) => ({
    row: coordinate.row + d.offset.row,
    col: coordinate.col + d.offset.col,
  }));
}
