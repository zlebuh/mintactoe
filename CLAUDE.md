# CLAUDE.md

Persistent context for AI agents (and humans) working on this repo. Keep this file current — any PR that changes the architecture updates this file in the same diff.

## What this project is

A multiplayer board game combining Minesweeper and 5-in-a-row (Gomoku). Two players alternate marks on a grid; some fields are hidden mines that erase the opponent's nearby marks when hit; first to 5-in-a-row wins. See [docs/game-rules.md](docs/game-rules.md) for the full rules spec.

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
apps/web/              React + TypeScript + Vite frontend
packages/game-engine/  Shared TS port of the game rules (grid, mines, win detection, serialization)
                          used by both Edge Functions and apps/web (local mode)
supabase/
  config.toml           Local dev + project config
  migrations/            SQL schema + RLS policies — the source of truth for the DB
  functions/
    create-game/
    join-game/
    make-move/
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
- CI is new and separate from the legacy workflows: `.github/workflows/ci.yml` runs on PRs into the feature branch. The real production deploy workflows (`deploy-supabase.yml`, `deploy-web.yml`) are gated to `master` only, so they stay dormant until the feature branch is finally merged — that merge *is* the production cutover, not a separate step.
- The full architecture/testing plan lives in the project's plan history; this file and `docs/game-rules.md` are the durable, always-current summary of it.

## Local development

One command runs the full new stack locally (Supabase + frontend) against each other:

```
pnpm dev:full
```

This just chains `supabase start && docker compose up --build` (see root `package.json`). Equivalent manual steps:

```
supabase start                          # starts the local Supabase stack (Postgres/Auth/Realtime/Studio),
                                         # applying supabase/migrations/ automatically
supabase status                         # shows the local anon key and API URL
cp .env.example .env                    # then fill in VITE_SUPABASE_ANON_KEY from the above (one-time)
docker compose up                       # builds and runs apps/web in a dev container with hot reload,
                                         # pointed at the local Supabase stack via host.docker.internal
```

The Supabase stack itself is run via `supabase start`, not reimplemented in `docker-compose.yml` — the Supabase CLI already manages its own docker compose stack, version-matched to `supabase/config.toml`, with migrations auto-applied and a Studio UI. `docker-compose.yml` at the repo root only wraps `apps/web`, so the whole app can be exercised end-to-end (including from a phone/another device on the same network, or without a local Node install) without hand-wiring the Supabase containers ourselves.

Running the frontend natively instead of in Docker also works: `pnpm --filter web dev` (after `supabase start`), with the same env vars in `apps/web/.env` — Docker is optional, not required.

## Testing conventions

- `packages/game-engine`: Vitest, near-100% coverage expected (pure logic, no I/O). Includes property-based tests (`fast-check`) for core invariants, not just example-based tests.
- `packages/supabase-tests`: integration tests that hit a real local Supabase stack via `@supabase/supabase-js` (real anonymous sign-ins, real REST calls) to prove RLS + table grants are actually enforced, not just assumed from reading the migration SQL. **Requires `supabase start` running first** — this is the one package where `pnpm test`/`pnpm -r test` needs live local infra, unlike `game-engine`/`web`. Runs in CI as its own job (`.github/workflows/ci.yml`), which starts the stack itself via `supabase/setup-cli` + `supabase start`.
  - There are deliberately **no fallback/default values** for `SUPABASE_URL`/`SUPABASE_ANON_KEY`/`SUPABASE_SERVICE_ROLE_KEY` in the test source, even the well-known local-dev demo keys — missing env throws immediately instead of the suite silently running against a guessed value. To run locally: `supabase start`, then export the three vars from the running stack before invoking the tests:
    ```
    eval "$(supabase status -o env)"
    export SUPABASE_URL="$API_URL" SUPABASE_ANON_KEY="$ANON_KEY" SUPABASE_SERVICE_ROLE_KEY="$SERVICE_ROLE_KEY"
    pnpm --filter @mintactoe/supabase-tests test
    ```
    CI does the equivalent itself as a step, after `supabase start`.
- `supabase/functions/*` (once they exist): unit-test the orchestration logic with a mocked Supabase client; extend `packages/supabase-tests` (or a similar suite) to integration-test the deployed functions against the real local stack.
- `apps/web`: component tests (Vitest + React Testing Library) plus Playwright e2e for the full online flow (two browser contexts playing a real game against the local Supabase stack).
