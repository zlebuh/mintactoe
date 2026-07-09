-- Scheduled cleanup of stale games (issue #9). Two retention windows:
--   1. Never-joined invites (invited_user_id IS NULL): 48 hours from created_at
--   2. Everything else (joined, in-progress, or finished): 30 days from updated_at
-- Runs hourly so the deadline time shown in the UI is accurate to ~1 hour.

create extension if not exists pg_cron with schema pg_catalog;

select cron.schedule(
  'cleanup-stale-games',
  '0 * * * *',
  $$
  delete from public.games
  where
    (invited_user_id is null
      and created_at < now() - interval '48 hours')
    or
    (updated_at < now() - interval '30 days');
  $$
);
