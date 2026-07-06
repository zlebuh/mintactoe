export class HttpError extends Error {
  readonly status: number;

  constructor(status: number, message: string) {
    super(message);
    this.name = new.target.name;
    this.status = status;
  }
}

export class UnauthenticatedError extends HttpError {
  constructor(message = "Missing or invalid credentials.") {
    super(401, message);
  }
}

export class InvalidRequestError extends HttpError {
  constructor(message: string) {
    super(400, message);
  }
}

export class GameNotFoundError extends HttpError {
  constructor(gameId: string) {
    super(404, `Game ${gameId} not found.`);
  }
}

export class NotAParticipantError extends HttpError {
  constructor() {
    super(403, "You are not a participant in this game.");
  }
}

export class CannotJoinOwnGameError extends HttpError {
  constructor() {
    super(409, "You cannot join a game you are hosting.");
  }
}

export class GameAlreadyFullError extends HttpError {
  constructor() {
    super(409, "This game already has two players.");
  }
}
