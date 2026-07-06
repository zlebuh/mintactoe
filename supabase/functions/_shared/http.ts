import { MinTacToeError } from "../../../packages/game-engine/dist/index.js";
import { corsHeaders } from "./cors.ts";
import { HttpError } from "./errors.ts";

export function json(body: unknown, status = 200): Response {
  return new Response(JSON.stringify(body), {
    status,
    headers: { ...corsHeaders, "Content-Type": "application/json" },
  });
}

// Maps both our own orchestration errors (HttpError) and game-engine validation errors
// (MinTacToeError, e.g. NotYourTurnError) to a 400/other client error response; anything
// else is unexpected and comes back as a 500 without leaking internal details.
export function errorResponse(error: unknown): Response {
  if (error instanceof HttpError) {
    return json({ error: error.name, message: error.message }, error.status);
  }
  if (error instanceof MinTacToeError) {
    return json({ error: error.name, message: error.message }, 400);
  }
  console.error(error);
  return json({ error: "InternalError", message: "Something went wrong." }, 500);
}
