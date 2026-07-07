# Game rules & domain spec

This is the reference spec for the game logic, extracted from the current (legacy) C# implementation in `src/Zlebuh.MinTacToe.GameModel`, `src/Zlebuh.MinTacToe.GameEngine`, and `src/Zlebuh.MinTacToe.GameSerialization`. It is the spec the TypeScript port in `packages/game-engine` must match, verified by porting the existing test suites (`GameEngine.Tests`, `GameSerialization.Tests`) case-for-case.

## Board & rules parameters

The `Rules` type carries all tunable parameters. Production games (created via the legacy API's `GameProxy`) use:

| Parameter | Production value | Meaning |
|---|---|---|
| `Rows` / `Columns` | 16 / 16 | Board dimensions |
| `SeriesLength` | 5 | Marks in a row needed to win |
| `NoMineMoves` | 6 | First N moves placed are guaranteed mine-free |
| `MinePower` | 1 | Radius (in cells) affected when a mine explodes |
| `MineProbability` | 0.1 | Probability a newly-generated field is a mine |

These should remain configurable in the port (not hardcoded), since `Rules` is a first-class parameter object in the current design — but the values above are what's actually live in production today.

## Grid representation

- The grid is **fully pre-allocated at game creation** — every one of the `rows × columns` coordinates has a `Field` entry from move 0 (confirmed by reading `GameControl.Initialize`, which loops the whole board; the legacy test suite explicitly asserts every coordinate is readable immediately after init). It is *not* sparse in memory: "generated" is a per-field flag on an always-present entry, not something that controls whether the entry exists. (An earlier draft of this doc got this backwards — sparse serialization of a dense in-memory grid was mistaken for a sparse grid.)
- Tie detection scans the entire board on every single move regardless, so there is no memory-saving reason to model the grid sparsely in-memory in the port either — pre-allocate the same way. Sparseness is worth applying only at the storage/serialization boundary (write only non-default fields to `game_state`), not in-memory.
- Each field (`Field`) tracks:
  - `player`: `"O"`, `"X"`, or null (unoccupied)
  - `isMine`: whether this field is a mine
  - `generated`: whether this field has been visited/allocated yet
  - `hasAllNeighboursGenerated`: whether all 8 neighbors have been generated (set once this field is played on)
  - `surroundedByNotExplodedMines`: count of adjacent, still-live mines (decremented as neighboring mines explode)

## Turn & move flow

Players are `O` and `X`; `O` always moves first. A move (`coordinate`, `player`) is validated in this order — reproduce this exact order and these exact failure modes in the port:

1. Game is not already over.
2. It's this player's turn.
3. Coordinate is within grid bounds.
4. Target field is not already occupied.
5. If the field hasn't been generated yet, generate it and its 8 neighbors now (lazy generation — mines are decided per-field, on first visit, not upfront for the whole board).
6. Mine placement: an ungenerated field becomes a mine with probability `MineProbability`, **unless** fewer than `NoMineMoves` moves have been played so far (first N moves are always safe) — **but this safety window only applies to the field the player directly clicked.** The 8 neighbor fields generated as a side effect of that click do *not* check `NoMineMoves` at all — they can become mines from move 1 onward. This is easy to miss when reading the code casually and is directly observable: with `MineProbability = 1`, a field played within the first `NoMineMoves` moves is itself never a mine, but is very likely to be completely surrounded by mines (its `surroundedByNotExplodedMines` count maxes out at 8).
7. If the target field is a mine: **explode** it (see below). The mine erases *the triggering player's own* nearby marks within `MinePower`; the opponent's marks are left untouched. The mine's own field is still marked with the triggering player, same as a normal move (this matches the legacy C# behavior — see `GameMakeMove.cs:69`, which sets `field.Player = player` unconditionally, after and regardless of the mine branch).
8. If the target field is not a mine: place the player's mark normally.
9. Check win (5-in-a-row) and tie conditions — **except a move that exploded a mine can never win**, even though its field ends up marked with the triggering player (step 7). This is a deliberate correction, not a straight port: the win check always credits the just-placed coordinate as the first mark of a potential run without checking whether that coordinate itself is a mine (it only checks `isMine` while walking *outward* to neighbors — see "Win detection" below) — so without this exception, detonating a mine could win a game outright, which contradicts `SurroundedByNotExplodedMines`'s own naming (implying "exploded" mines were meant to be tracked as a distinct, inert state) and has no C# test coverage either way. The mine's field still permanently blocks any *future* line through it, for both players, exactly like an undiscovered mine already does (see "A run stops at..." below).
10. Alternate `playerOnTurn` (unless the game just ended).

Randomness for mine placement is **not seeded** in the legacy implementation — genuinely random per game, not reproducible. Decide deliberately in the port whether to keep this (e.g. for parity testing during the port, inject a seedable PRNG so game sequences can be replayed and diffed against the C# output, even though production remains unseeded).

## Win detection (5-in-a-row)

- Checked from the just-placed coordinate outward, across **4 axes** (horizontal, vertical, and the two diagonals) — each axis walked in both directions from the placed cell and summed. The just-placed coordinate itself is always credited as the first mark of every axis, without re-checking whether *it* is a mine - callers must not invoke this check at all for a move that exploded a mine (see "Turn & move flow" step 9), since it would otherwise count as a win.
- A run stops at: grid boundary, an ungenerated field, a mine, or an opponent's mark.
- **Mines do not break a run** if they don't stop it per the above — i.e. a mine cell itself doesn't count as "the opponent's mark," so a line of the same player's marks with an intervening mine is not automatically broken by mine-ness alone (double check this exact interaction against the ported test cases — it's the single subtlest rule in the engine, historically implemented via a "walk outward, group opposite directions into 4 master axes" approach).
- A win triggers as soon as any axis reaches `SeriesLength` consecutive marks belonging to the player who just moved.

## Tie detection

- Full-grid scan: the game is a tie once every non-mine field is occupied (no legal moves remain) and no win has been triggered.

## Mine explosion

- When a mine is hit, every field within `MinePower` (Chebyshev/square radius, not just Manhattan-adjacent — i.e. the `(2×MinePower+1)²` square centered on the mine, excluding the mine cell itself) is affected:
  - Only marks belonging to the **triggering player** (the one who just stepped on the mine) are erased (reset to unoccupied).
  - The **opponent's** marks in that radius are left untouched.
  - `surroundedByNotExplodedMines` is decremented on all affected non-mine fields in the radius (this mine no longer counts as a live threat to them).
- The set of all coordinates changed by a move (including explosion side-effects) is returned/tracked so the client can highlight what changed — see `changes` in the serialization format below.

## Game state JSON shape (legacy format — do not carry forward verbatim)

The legacy `GameSerializer` produces a custom, non-standard shape: the grid is a flat array of alternating `"row,col"` coordinate strings and field objects (not a normal JSON object/map), because .NET's default JSON serialization doesn't handle `Dictionary<Coordinate, Field>` keys the way this format needed:

```json
{
  "gameState": {
    "grid": [
      "0,0", { "player": "O", "isMine": false, "generated": true, "hasAllNeighboursGenerated": true, "surroundedByNotExplodedMines": 1 },
      "1,2", { "player": "X", "isMine": true, "generated": false, "hasAllNeighboursGenerated": false, "surroundedByNotExplodedMines": 0 }
    ],
    "isGameOver": true,
    "winner": "O",
    "playerOnTurn": "X",
    "changes": ["0,0", "1,2"],
    "movesPlayed": 7
  },
  "rules": { "rows": 16, "columns": 16, "seriesLength": 5, "noMineMoves": 6, "minePower": 1, "mineProbability": 0.1 }
}
```

**This shape was not carried forward.** `packages/game-engine/src/serialization.ts` (issue #5) uses a plain JSON object instead of the alternating-array workaround — the in-memory grid is fully pre-allocated (see above), but the *serialized* `grid` is sparse: only fields that differ from the default (unvisited, unoccupied, non-mine) are written, keyed by `"row,col"` strings, and reconstructed into a full dense grid on deserialize:

```json
{
  "rules": { "rows": 16, "columns": 16, "seriesLength": 5, "noMineMoves": 6, "minePower": 1, "mineProbability": 0.1 },
  "gameState": {
    "grid": {
      "0,0": { "player": "O", "isMine": false, "generated": true, "hasAllNeighboursGenerated": true, "surroundedByNotExplodedMines": 1 },
      "1,2": { "player": "X", "isMine": true, "generated": true, "hasAllNeighboursGenerated": false, "surroundedByNotExplodedMines": 0 }
    },
    "isGameOver": true,
    "winner": "O",
    "playerOnTurn": "X",
    "changes": [{ "row": 0, "col": 0 }, { "row": 1, "col": 2 }],
    "movesPlayed": 7
  }
}
```

## What the tests encode (ported into packages/game-engine)

The legacy test suites were the executable spec for all of the above, ported case-for-case (same scenarios, same expected values) into `packages/game-engine`'s Vitest suite:

- `gameControl.test.ts` / `makeMove.test.ts` / `gameOverChecks.test.ts` / `mineExplosion.test.ts`: turn enforcement, out-of-bounds coordinates, occupied-field rejection, win detection (horizontal + diagonal), tie detection (including the mocked-mine tie scenarios), mine explosion (the "only the triggering player's own marks are erased" rule, verified against the exact C# `SurroundingMinesChanges`/`BombExploded*` expected values), the `NoMineMoves`-only-applies-to-the-clicked-field subtlety, and — a deliberate deviation from the C# original, see "Turn & move flow" step 9 above — that a move which explodes a mine is never itself credited as a win (`mineExplosion.test.ts`'s "does not credit the triggering player with a win..." case).
- `properties.test.ts`: `fast-check` property tests for the mine-explosion invariant (opponent marks never erased, every affected counter decremented by exactly 1) and the move/turn-alternation invariant, generated across random inputs rather than fixed examples.
- `serialization.test.ts`: round-trip correctness for the new sparse JSON shape (not the legacy alternating-array format).
- `scripts/parity-check.ts`: one-off script (not in CI) that replayed several deterministic scenarios (`mineProbability` 0 or 1, so outcomes don't depend on the actual random draw) through both the real C# engine and the TS port and diffed the results — used once during the port, all scenarios matched exactly. Predates the mine-win-credit fix above, so its mine-triggering scenarios are frozen against the (since-corrected) original C# behavior; see its header comment.
