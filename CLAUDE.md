# CLAUDE.md

Persistent context for AI agents (and humans) working on this repo. Keep this file current — any PR that changes the architecture updates this file in the same diff.

## What this project is

A multiplayer board game combining Minesweeper and 5-in-a-row (Gomoku). Two players alternate marks on a grid; some fields are hidden mines that erase *the triggering player's own* nearby marks when hit (the opponent's marks are untouched); first to 5-in-a-row wins. See [README.md](README.md) for the short product description and [docs/game-rules.md](docs/game-rules.md) for the full rules spec.

## Current status: mid-rewrite

This repo is being rewritten in place. Two stacks currently coexist:

- **Legacy stack (on `master`, currently in production)**: Blazor WASM frontend (`src/Zlebuh.MinTacToe.UI`) on Netlify, a C# ASP.NET API (`src/Zlebuh.MinTacToe.API`) on Fly.io, Supabase (Postgres + Realtime) as the data/transport layer with its schema managed only via the Supabase dashboard (not in this repo).
- **New stack (being built on the `refactor/supabase-react` branch)**: Supabase remains the DB/Realtime layer but its schema/config now lives in this repo, the C# API is eliminated in favor of Supabase Edge Functions, and the frontend is rewritten in React + TypeScript.

**Do not modify `src/` (the legacy .NET solution) as part of the rewrite** — it stays untouched and deployable from `master` until the final cutover issue. All rewrite work happens on `refactor/supabase-react` and is deleted only in the last cutover step.

## Why this rewrite (decisions + rationale)

- **Supabase stays, but becomes version-controlled.** The DB schema, RLS policies, and Supabase project config previously existed only in the dashboard. Now they're migrations under `supabase/migrations/`, deployed via the Supabase CLI in CI.
- **The C# API is replaced by Supabase Edge Functions**, not by a self-hosted alternative. The API's only real job was authoritative move validation; everything else it did (a hand-rolled `users` table, per-game host/visitor tokens) was working around not having real auth. Removing it removes a whole deployment target (Fly.io) and a whole set of "is this in sync with the DB" problems.
- **Anonymous Auth + RLS replaces the token scheme.** The old design checked a client-supplied token string in application code to decide who could move. The new design uses `supabase.auth.signInAnonymously()` for a stable `auth.uid()` per browser, and Postgres Row-Level Security actually enforces who can read/write a game row. Writes to `game_state` go only through Edge Functions using the service-role key — RLS denies direct client writes.
- **Blazor is replaced by React + TypeScript**, per explicit user preference (better ecosystem for visual design/styling).
- **The game engine is ported to TypeScript** (`packages/game-engine`) rather than kept in C#, so the exact same validation logic runs both server-side (Edge Functions, authoritative) and client-side (the local/offline 2-player mode, and optimistic UI). One implementation, not two to keep in sync.

## Target repository layout

```
apps/web/              React + TypeScript + Vite frontend. Tailwind CSS v4 + a small hand-built
                          component set (components/ui/ - Button/Card/Badge/Dialog, the last wrapping
                          @radix-ui/react-dialog for accessibility) rather than shadcn/ui's CLI, to
                          avoid its config surface (components.json, ESLint-oriented tooling) on a
                          repo that otherwise runs oxlint. react-router (data mode: createBrowserRouter
                          + RouterProvider, see src/router.tsx) for routing. components/game/ holds the
                          board/turn-info presentational components; hooks/useLocalGame.ts wraps
                          packages/game-engine directly for the local (offline) 2-player mode - no
                          network involved, unlike the online flow (issue #8).
packages/game-engine/  Shared TS port of the game rules (grid, mines, win detection, serialization)
                          used by both Edge Functions and apps/web (local mode)
supabase/
  config.toml           Local dev + project config
  migrations/            SQL schema + RLS policies — the source of truth for the DB
  functions/
    deno.json              Shared Deno config for all functions (test file discovery, see below)
    _shared/                Auth/error/response helpers + game-row typing shared by all four functions
    create-game/
    join-game/
    make-move/
    forfeit-game/
.github/workflows/
  ci.yml                 Lint/build/test on PRs into refactor/supabase-react (new, separate from legacy CI)
  deploy-supabase.yml     supabase db push + functions deploy — gated to push on master only
  deploy-web.yml          Build + deploy apps/web to Netlify — gated to push on master only
```

`src/`, `Dockerfile`, `fly.toml`, `render.yaml`, and the legacy `deploy-api-flyio.yml`/`deploy.yml` workflows are removed in the final cutover PR, not before.

## Delivery process

- All rewrite work targets the **`refactor/supabase-react`** branch, not `master`. `master` keeps deploying the legacy stack untouched throughout.
- The work is broken into GitHub issues, each with its own PR against `refactor/supabase-react`. **Work on the next issue does not start until the current PR is reviewed and merged by the repo owner.** See issues in this repo for the full sequence (roughly: docs → monorepo/CI scaffold → Supabase schema/RLS → game engine port → Edge Functions → frontend core/local mode → frontend online flow → hardening → production deploy automation → cutover).
- **Every unit of work starts on its own new branch, branched from `origin/refactor/supabase-react`** (not from `master`, and not stacked on another in-progress work branch). Open the PR for that branch against `refactor/supabase-react`.
- CI is new and separate from the legacy workflows: `.github/workflows/ci.yml` runs on PRs and pushes to both `refactor/supabase-react` and `master` (so the cutover PR gets validated before merge). The production deploy workflows (`deploy-supabase.yml`, `deploy-web.yml`) are gated to `push` on `master` only, so they stay dormant until the feature branch is finally merged — that merge *is* the production cutover, not a separate step.
- The full architecture/testing plan lives in the project's plan history; this file and `docs/game-rules.md` are the durable, always-current summary of it.

## Local development

One command runs the full new stack locally (Supabase + frontend) against each other:

```
pnpm dev:full
```

This just chains `pnpm --filter @mintactoe/game-engine build && supabase start && docker compose up --build` (see root `package.json`). Equivalent manual steps:

```
pnpm --filter @mintactoe/game-engine build   # builds packages/game-engine/dist - supabase/functions/*
                                              # import from there (not src), and must exist before the
                                              # next step or the local Edge Runtime fails to boot them
supabase start                          # starts the local Supabase stack (Postgres/Auth/Realtime/Studio/
                                         # Edge Functions), applying supabase/migrations/ automatically
supabase status                         # shows the local anon key and API URL
cp .env.example .env                    # then fill in VITE_SUPABASE_ANON_KEY from the above (one-time)
docker compose up                       # builds and runs apps/web in a dev container with hot reload,
                                         # pointed at the local Supabase stack via host.docker.internal
```

If you edit `packages/game-engine` while a local stack is already running, re-run the build and then `supabase stop && supabase start` - the local Edge Runtime doesn't pick up dist changes made after it started.

The Supabase stack itself is run via `supabase start`, not reimplemented in `docker-compose.yml` — the Supabase CLI already manages its own docker compose stack, version-matched to `supabase/config.toml`, with migrations auto-applied and a Studio UI. `docker-compose.yml` at the repo root only wraps `apps/web`, so the whole app can be exercised end-to-end (including from a phone/another device on the same network, or without a local Node install) without hand-wiring the Supabase containers ourselves.

Running the frontend natively instead of in Docker also works: `pnpm --filter web dev` (after `supabase start`), with the same env vars in `apps/web/.env` — Docker is optional, not required.

## Testing conventions

- `packages/game-engine`: Vitest, near-100% coverage expected (pure logic, no I/O) - run `pnpm --filter @mintactoe/game-engine test:coverage` to check (CI does this on every push). Includes property-based tests (`fast-check`) for core invariants, not just example-based tests, and a one-off `scripts/parity-check.ts` (not in CI, see the script header) that verified the TS port against the real C# engine during the port itself.
- `packages/supabase-tests`: integration tests that hit a real local Supabase stack via `@supabase/supabase-js` (real anonymous sign-ins, real REST calls) to prove RLS + table grants are actually enforced, not just assumed from reading the migration SQL. **Requires `supabase start` running first** — this is the one package where `pnpm test`/`pnpm -r test` needs live local infra, unlike `game-engine`/`web`. Runs in CI as its own job (`.github/workflows/ci.yml`), which starts the stack itself via `supabase/setup-cli` + `supabase start`.
  - There are deliberately **no fallback/default values** for `SUPABASE_URL`/`SUPABASE_ANON_KEY`/`SUPABASE_SERVICE_ROLE_KEY` in the test source, even the well-known local-dev demo keys — missing env throws immediately instead of the suite silently running against a guessed value. To run locally: `supabase start`, then export the three vars from the running stack before invoking the tests:
    ```
    eval "$(supabase status -o env)"
    export SUPABASE_URL="$API_URL" SUPABASE_ANON_KEY="$ANON_KEY" SUPABASE_SERVICE_ROLE_KEY="$SERVICE_ROLE_KEY"
    pnpm --filter @mintactoe/supabase-tests test
    ```
    CI does the equivalent itself as a step, after `supabase start`.
- `supabase/functions/*`: each function is a thin `index.ts` (`Deno.serve`, extracts the caller from the JWT via `_shared/auth.ts`, calls the handler, maps errors to HTTP status) delegating to an exported `handler.ts` orchestration function that takes a `SupabaseClient` as a parameter — that's what makes it unit-testable without booting the serve loop. Unit tests (`handler.test.ts`, Deno's built-in test runner, not Vitest — this code runs on Deno, not Node) use `_shared/testSupabase.ts`, a minimal fluent fake of the `.from("games")...` chain that hands back scripted `{ data, error }` results in call order, rather than a full Postgrest mock. Run with `deno test --allow-env --allow-net --config supabase/functions/deno.json supabase/functions` (the explicit `--config` matters: Deno's auto-discovery walks up from the cwd and stops at the root `package.json`/`pnpm-workspace.yaml` instead of finding this one).
  - **These functions import `packages/game-engine/dist` (built output), not `src`.** That package's own internal imports use the `./foo.js`-pointing-at-`./foo.ts` convention (valid under its `tsconfig.json`'s `"moduleResolution": "Bundler"`, for Node/Vite consumers) — plain Deno's module graph resolution can't follow that, and critically, the *deployed* `supabase-edge-runtime` (unlike the plain `deno` CLI) doesn't support the `sloppy-imports` flag that would otherwise paper over it, so pointing at `src` breaks the function's actual boot, not just local type-checking. Run `pnpm --filter @mintactoe/game-engine build` before `supabase start` or `deno test`/`deno check` against this directory (see "Local development" above); CI does this in both the `edge-functions` and `supabase` jobs.
  - **The `Player`/`Coordinate`/`Rules`/`SerializedGame` types are mirrored locally in `_shared/gameRow.ts`**, not imported from `packages/game-engine`, even though the values (`initialize`, `serializeGame`, `deserializeGame`, `makeMove`, the `*Error` classes) are imported normally from `dist/index.js`. Deno's checker infers value types loosely straight from the plain compiled JS but doesn't resolve a `type`-only export re-exported through a `.js` specifier back to its real declaration — and, as above, it can't be pointed at `src` either. These types are small and frozen (see `docs/game-rules.md`), so the duplication is a deliberate, documented trade-off, not an oversight.
  - Integration tests live in `packages/supabase-tests/src/edge-functions.test.ts` (Vitest, real `fetch` calls to `${SUPABASE_URL}/functions/v1/<name>` with real anonymous-session bearer tokens) — `supabase start` serves local functions automatically (config.toml's `[edge_runtime]` block), no separate `supabase functions serve` needed for CI/testing purposes.
- `apps/web`: component tests via Vitest + `@testing-library/react` (`pnpm --filter web test`; jsdom environment + setup file configured in `vite.config.ts`'s `test` block per Vitest's own recommended pattern, since this app otherwise has no separate Jest/Vitest config file). Not using `globals: true` (consistent with the rest of the repo's explicit-import style), so `src/test/setup.ts` registers `@testing-library/react`'s `cleanup()` on `afterEach` itself — without `globals: true`, that package can't auto-detect a global `afterEach` to hook into. `packages/game-engine`'s `makeMove()` mutates in place; `useLocalGame` (see above) clones with `structuredClone` before each move so React re-renders correctly and an illegal move (caught via `MinTacToeError`) can be discarded without corrupting the previous state. Playwright e2e for the full *online* flow (two browser contexts playing a real game against the local Supabase stack) is issue #8's scope, not built yet.

## Generated types

`apps/web/src/lib/database.types.ts` is generated by `supabase gen types typescript --local` (wrapped as `pnpm gen:types`). It types the Supabase client in `apps/web/src/lib/supabase.ts` via `createClient<Database>(...)`, giving typed query results throughout the frontend. Regenerate and commit after any schema migration. CI checks for drift in the `supabase` job (regenerates against the local stack and diffs).

Edge Functions keep their own hand-mirrored types in `_shared/gameRow.ts` rather than importing the generated file — see the Deno resolution constraints documented above.

## Known security gaps

- **Rate limiting:** The `create-game` idempotency (one active game per `auth.uid()`) stops accidental spam from a single session, but anonymous auth has no persistent identity beyond the browser — clearing localStorage or using incognito yields a fresh `auth.uid()` for free. True abuse-resistance would require IP-based throttling (e.g. a table keyed by hashed IP + sliding window). Deliberately deferred unless it becomes an actual problem — real complexity for a free hobby project against a threat that likely won't materialize.

## Game cleanup

A `pg_cron` job (`supabase/migrations/20260709120000_cleanup_cron.sql`) runs hourly and deletes stale games in two tiers:
- **Never-joined invites** (`invited_user_id IS NULL`): removed 48 hours after `created_at`.
- **All other games** (in-progress or finished): removed 30 days after `updated_at` (any move resets the clock).

The frontend shows an amber "Expires [date]" notice when a game enters its last 24 hours — both in the recent-games list (`HomeMenu`) and on the game page itself (`OnlineGamePage`). The deadline is computed client-side from the same timestamps the cron job uses; keep the intervals in `apps/web/src/lib/cleanup.ts` in sync with the migration SQL if either changes.
