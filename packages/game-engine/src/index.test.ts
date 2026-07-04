import { describe, expect, it } from "vitest";
import { GAME_ENGINE_VERSION } from "./index.js";

describe("game-engine package scaffold", () => {
  it("exports a version placeholder until the engine port lands", () => {
    expect(GAME_ENGINE_VERSION).toBe("0.0.0");
  });
});
