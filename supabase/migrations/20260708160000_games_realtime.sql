-- Enable Supabase Realtime change events for the games table so the frontend's
-- postgres_changes subscription actually receives updates (e.g. when a visitor
-- joins or a move is made).
alter publication supabase_realtime add table public.games;
