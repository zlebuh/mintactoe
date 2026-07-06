import { describe, expect, it } from "vitest";
import { SUPABASE_URL, SUPABASE_ANON_KEY, signInAnonymously, type AnonSession } from "./testEnv.js";

interface FunctionResponse {
  status: number;
  body: any;
}

async function callFunction(name: string, session: AnonSession, body: unknown = {}): Promise<FunctionResponse> {
  const response = await fetch(`${SUPABASE_URL}/functions/v1/${name}`, {
    method: "POST",
    headers: {
      Authorization: `Bearer ${session.accessToken}`,
      apikey: SUPABASE_ANON_KEY,
      "Content-Type": "application/json",
    },
    body: JSON.stringify(body),
  });
  return { status: response.status, body: await response.json() };
}

describe("Edge Functions (create-game, join-game, make-move)", () => {
  it("create-game is idempotent per caller and returns an unfinished game on repeat calls", async () => {
    const host = await signInAnonymously();

    const first = await callFunction("create-game", host);
    expect(first.status).toBe(200);
    expect(first.body.game.host_user_id).toBe(host.userId);
    expect(first.body.game.invited_user_id).toBeNull();
    expect(first.body.game.game_state.gameState.isGameOver).toBe(false);

    const second = await callFunction("create-game", host);
    expect(second.status).toBe(200);
    expect(second.body.game.id).toBe(first.body.game.id);
  });

  it("join-game rejects the host joining their own game, then lets a visitor join, then rejects a third player", async () => {
    const host = await signInAnonymously();
    const visitor = await signInAnonymously();
    const stranger = await signInAnonymously();

    const created = await callFunction("create-game", host);
    const gameId = created.body.game.id as string;

    const selfJoin = await callFunction("join-game", host, { gameId });
    expect(selfJoin.status).toBe(409);

    const join = await callFunction("join-game", visitor, { gameId });
    expect(join.status).toBe(200);
    expect(join.body.game.invited_user_id).toBe(visitor.userId);

    const full = await callFunction("join-game", stranger, { gameId });
    expect(full.status).toBe(409);
  });

  it("make-move rejects non-participants and malformed coordinates, then enforces turn order across a real move", async () => {
    const host = await signInAnonymously();
    const visitor = await signInAnonymously();
    const stranger = await signInAnonymously();

    const created = await callFunction("create-game", host);
    const gameId = created.body.game.id as string;
    await callFunction("join-game", visitor, { gameId });

    const byStranger = await callFunction("make-move", stranger, { gameId, coordinate: { row: 0, col: 0 } });
    expect(byStranger.status).toBe(403);

    const malformed = await callFunction("make-move", host, { gameId, coordinate: { row: "0", col: 0 } });
    expect(malformed.status).toBe(400);

    const hostMove = await callFunction("make-move", host, { gameId, coordinate: { row: 0, col: 0 } });
    expect(hostMove.status).toBe(200);
    expect(hostMove.body.game.game_state.gameState.movesPlayed).toBe(1);

    const hostMovesAgain = await callFunction("make-move", host, { gameId, coordinate: { row: 1, col: 1 } });
    expect(hostMovesAgain.status).toBe(400);
    expect(hostMovesAgain.body.error).toBe("NotYourTurnError");

    const visitorMove = await callFunction("make-move", visitor, { gameId, coordinate: { row: 1, col: 1 } });
    expect(visitorMove.status).toBe(200);
    expect(visitorMove.body.game.game_state.gameState.movesPlayed).toBe(2);
  });

  it("rejects a make-move on a game the caller was never invited to", async () => {
    const host = await signInAnonymously();
    const stranger = await signInAnonymously();

    const created = await callFunction("create-game", host);
    const gameId = created.body.game.id as string;

    const result = await callFunction("make-move", stranger, { gameId, coordinate: { row: 0, col: 0 } });
    expect(result.status).toBe(403);
  });
});
