import { getCallerId } from "../_shared/auth.ts";
import { corsHeaders } from "../_shared/cors.ts";
import { errorResponse, json } from "../_shared/http.ts";
import { createAdminClient } from "../_shared/supabaseAdmin.ts";
import { createGame } from "./handler.ts";

Deno.serve(async (req) => {
  if (req.method === "OPTIONS") {
    return new Response("ok", { headers: corsHeaders });
  }

  try {
    const callerId = await getCallerId(req);
    const game = await createGame(createAdminClient(), { callerId });
    return json({ game });
  } catch (error) {
    return errorResponse(error);
  }
});
