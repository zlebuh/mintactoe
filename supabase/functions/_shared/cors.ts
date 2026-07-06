// The frontend (apps/web, issue #7/#8) calls these functions directly from the browser,
// so preflight OPTIONS requests need a response and actual responses need these headers.
export const corsHeaders: Record<string, string> = {
  "Access-Control-Allow-Origin": "*",
  "Access-Control-Allow-Headers": "authorization, x-client-info, apikey, content-type",
};
