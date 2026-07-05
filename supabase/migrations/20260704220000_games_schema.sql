-- games table replaces the legacy `users` + `games` (with host_token/invited_token) design.
-- Identity is now Supabase Auth (auth.users, including anonymous sessions); access control is
-- enforced by RLS instead of comparing a client-supplied token string in application code.

create table public.games (
  id uuid primary key default gen_random_uuid(),
  created_at timestamptz not null default now(),
  updated_at timestamptz not null default now(),
  host_user_id uuid not null references auth.users (id) on delete cascade,
  invited_user_id uuid references auth.users (id) on delete set null,
  game_state jsonb not null
);

create index games_host_user_id_idx on public.games (host_user_id);
create index games_invited_user_id_idx on public.games (invited_user_id);

create function public.set_games_updated_at()
returns trigger
language plpgsql
as $$
begin
  new.updated_at = now();
  return new;
end;
$$;

create trigger games_set_updated_at
  before update on public.games
  for each row
  execute function public.set_games_updated_at();

-- Table-level GRANTs are required in addition to RLS: Supabase no longer auto-exposes new public
-- tables to the API roles (see `auto_expose_new_tables` in supabase/config.toml), so without these
-- every request gets "permission denied for table games" before RLS even runs. `service_role`
-- bypasses RLS but still needs standard privileges to read/write at all.
grant select on public.games to authenticated;
grant select, insert, update, delete on public.games to service_role;

alter table public.games enable row level security;

-- A row is readable by its host, its invited participant, or anyone (while it's still open,
-- i.e. invited_user_id is null) so a visitor can load the game to join it via the invite link.
create policy "Participants and open invites can read games"
  on public.games for select
  to authenticated
  using (
    host_user_id = (select auth.uid())
    or invited_user_id = (select auth.uid())
    or invited_user_id is null
  );

-- No insert/update/delete policy is defined for `authenticated`, so RLS denies all direct client
-- writes by default. All mutations (create-game, join-game, make-move) go through Edge Functions
-- using the service-role key, which bypasses RLS but is still subject to the GRANTs above.
