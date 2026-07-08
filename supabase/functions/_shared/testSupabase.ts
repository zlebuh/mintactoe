import type { GameRow } from "./gameRow.ts";

export interface MockResult<T = unknown> {
  data: T | null;
  error: { message: string } | null;
}

function chainable(result: MockResult): unknown {
  const builder = {
    select: () => builder,
    insert: () => builder,
    update: () => builder,
    eq: () => builder,
    gte: () => builder,
    lte: () => builder,
    or: () => builder,
    order: () => builder,
    limit: () => builder,
    single: () => builder,
    maybeSingle: () => builder,
    then: (onFulfilled: (r: MockResult) => unknown, onRejected?: (e: unknown) => unknown) =>
      Promise.resolve(result).then(onFulfilled, onRejected),
  };
  return builder;
}

/**
 * Fakes just the `.from("games")...` entry point of a SupabaseClient. Each handler under
 * test makes a fixed, known sequence of `.from()` calls, so this hands back scripted
 * results in that call order rather than modelling real Postgrest filter/chain semantics.
 */
export function createMockSupabase(results: MockResult[]): unknown {
  const queue = [...results];
  return {
    from: () => {
      const result = queue.shift();
      if (!result) {
        throw new Error("createMockSupabase: ran out of scripted results");
      }
      return chainable(result);
    },
  };
}

export function createGameRow(overrides: Partial<GameRow> = {}): GameRow {
  return {
    id: "game-1",
    host_user_id: "host-1",
    invited_user_id: null,
    game_state: {
      rules: {
        rows: 16,
        columns: 16,
        seriesLength: 5,
        noMineMoves: 6,
        minePower: 1,
        mineProbability: 0.1,
      },
      gameState: {
        grid: {},
        isGameOver: false,
        winner: null,
        playerOnTurn: "O",
        changes: [],
        movesPlayed: 0,
      },
    },
    ...overrides,
  };
}
