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

- The grid is **sparse**: only fields that have been "generated" (visited by move placement or neighbor-generation) exist; everything else is implicitly empty/ungenerated.
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
6. Mine placement: an ungenerated field becomes a mine with probability `MineProbability`, **unless** fewer than `NoMineMoves` moves have been played so far (first N moves are always safe).
7. If the target field is a mine: **explode** it (see below). The mine erases *the triggering player's own* nearby marks within `MinePower`; the opponent's marks are left untouched.
8. If the target field is not a mine: place the player's mark normally.
9. Check win (5-in-a-row) and tie conditions.
10. Alternate `playerOnTurn` (unless the game just ended).

Randomness for mine placement is **not seeded** in the legacy implementation — genuinely random per game, not reproducible. Decide deliberately in the port whether to keep this (e.g. for parity testing during the port, inject a seedable PRNG so game sequences can be replayed and diffed against the C# output, even though production remains unseeded).

## Win detection (5-in-a-row)

- Checked from the just-placed coordinate outward, across **4 axes** (horizontal, vertical, and the two diagonals) — each axis walked in both directions from the placed cell and summed.
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

**This shape does not need to be preserved.** Since `game_state` is moving to a real `jsonb` column with no legacy readers, the TypeScript port should use a straightforward JSON object (e.g. `{ [coordinateKey: string]: Field }`) instead of the alternating-array workaround. Keep `isGameOver`, `winner`, `playerOnTurn`, `changes`, and `movesPlayed` (or equivalents) — the frontend's realtime update handling depends on knowing what just changed, not just the full state.

## What the tests already encode (port these first)

The legacy test suites are the executable spec for all of the above — port them case-for-case into the new `packages/game-engine` Vitest suite before considering the port done:

- `GameEngineTests` / `GameControlTests` / `GameOverTests`: turn enforcement, out-of-bounds coordinates, occupied-field rejection, win detection in all 4 axes, tie detection, mine explosion (including the "only the triggering player's own marks are erased" rule), neighbor generation on first placement.
- `GameSerializationTests` / `CoordinateConverterTests` / `GridConverterTests`: round-trip serialization correctness (less relevant verbatim once the JSON shape changes, but the underlying state transitions they exercise are still valid fixtures).
