import { createClient } from "npm:@supabase/supabase-js@2";
import { UnauthenticatedError } from "./errors.ts";

// Resolves the calling `auth.uid()` from the request's bearer token by asking GoTrue to
// verify it (rather than decoding the JWT payload ourselves) so this stays correct even if
// per-function `verify_jwt` config ever changes - it doesn't rely on the gateway having
// already checked the signature.
export async function getCallerId(req: Request): Promise<string> {
  const authHeader = req.headers.get("Authorization") ?? "";
  const token = authHeader.replace(/^Bearer\s+/i, "").trim();
  if (!token) {
    throw new UnauthenticatedError("Missing Authorization bearer token.");
  }

  const url = Deno.env.get("SUPABASE_URL");
  const anonKey = Deno.env.get("SUPABASE_ANON_KEY");
  if (!url || !anonKey) {
    throw new Error("SUPABASE_URL and SUPABASE_ANON_KEY must be set.");
  }

  const authClient = createClient(url, anonKey);
  const { data, error } = await authClient.auth.getUser(token);
  if (error || !data.user) {
    throw new UnauthenticatedError("Invalid or expired token.");
  }
  return data.user.id;
}
