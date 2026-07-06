import type { Coordinate, Field, Game, Player, Rules } from "./types.js";
import { coordinateKey } from "./coordinate.js";
import { createDefaultField, createGrid } from "./grid.js";
import { GameSerializationError } from "./errors.js";

/**
 * Storage format for `game_state` (jsonb). The grid is sparse here (only fields that differ
 * from the default are written) even though the in-memory `Grid` is fully pre-allocated;
 * deserializing rebuilds the full dense grid and overlays these entries on top. See
 * docs/game-rules.md for the full format.
 */
export interface SerializedGame {
  rules: Rules;
  gameState: {
    grid: Record<string, Field>;
    isGameOver: boolean;
    winner: Player | null;
    playerOnTurn: Player | null;
    changes: Coordinate[];
    movesPlayed: number;
  };
}

const DEFAULT_FIELD = createDefaultField();

function isDefaultField(field: Field): boolean {
  return (
    field.player === DEFAULT_FIELD.player &&
    field.surroundedByNotExplodedMines === DEFAULT_FIELD.surroundedByNotExplodedMines &&
    field.isMine === DEFAULT_FIELD.isMine &&
    field.generated === DEFAULT_FIELD.generated &&
    field.hasAllNeighboursGenerated === DEFAULT_FIELD.hasAllNeighboursGenerated
  );
}

export function serializeGame(game: Game): SerializedGame {
  const grid: Record<string, Field> = {};
  for (const [key, field] of game.gameState.grid) {
    if (!isDefaultField(field)) {
      grid[key] = field;
    }
  }

  return {
    rules: game.rules,
    gameState: {
      grid,
      isGameOver: game.gameState.isGameOver,
      winner: game.gameState.winner,
      playerOnTurn: game.gameState.playerOnTurn,
      changes: game.gameState.changes,
      movesPlayed: game.gameState.movesPlayed,
    },
  };
}

export function deserializeGame(serialized: string | SerializedGame): Game {
  let data: SerializedGame;
  if (typeof serialized === "string") {
    if (!serialized.trim()) {
      throw new GameSerializationError("Serialized game string cannot be null or empty.");
    }
    try {
      data = JSON.parse(serialized) as SerializedGame;
    } catch (cause) {
      throw new GameSerializationError("Serialized game string is not valid JSON.");
    }
  } else {
    data = serialized;
  }

  if (!data || typeof data !== "object" || !data.rules || !data.gameState) {
    throw new GameSerializationError("Serialized game is missing rules or gameState.");
  }

  const grid = createGrid(data.rules);
  for (const [key, field] of Object.entries(data.gameState.grid ?? {})) {
    grid.set(key, field);
  }

  return {
    rules: data.rules,
    gameState: {
      grid,
      isGameOver: data.gameState.isGameOver,
      winner: data.gameState.winner,
      playerOnTurn: data.gameState.playerOnTurn,
      changes: data.gameState.changes ?? [],
      movesPlayed: data.gameState.movesPlayed,
    },
  };
}

export { coordinateKey };
