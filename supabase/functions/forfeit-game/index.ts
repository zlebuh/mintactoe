import { z } from "zod";
import { getCallerId } from "../_shared/auth.ts";
import { corsHeaders } from "../_shared/cors.ts";
import { InvalidRequestError } from "../_shared/errors.ts";
import { errorResponse, json } from "../_shared/http.ts";
import { createAdminClient } from "../_shared/supabaseAdmin.ts";
import { forfeitGame } from "./handler.ts";

const Body = z.object({
  gameId: z.string().trim().min(1, "gameId is required."),
});

Deno.serve(async (req) => {
  if (req.method === "OPTIONS") {
    return new Response("ok", { headers: corsHeaders });
  }

  try {
    const callerId = await getCallerId(req);
    const raw = await req.json().catch(() => ({}));
    const result = Body.safeParse(raw);
    if (!result.success) {
      throw new InvalidRequestError(result.error.issues[0].message);
    }
    const game = await forfeitGame(createAdminClient(), { callerId, gameId: result.data.gameId });
    return json({ game });
  } catch (error) {
    return errorResponse(error);
  }
});
