// Rispecchia LobbyStateMessage.cs
export interface LobbyStateMessage {
    players: PlayerDTO[];
    games: GameRoomDTO[];
}

export interface PlayerDTO {
    playerId: string;
    playerName: string;
    isReady?: boolean;
}

export interface GameRoomDTO {
    gameId: string;
    gameName: string;
    players?: PlayerDTO[];
    capacity?: number;
}

export interface PlayerJoinedLobbyMessage {
    playerId: string;
    playerName: string;
}

export interface GameCreatedMessage {
    gameId: string;
    gameName: string;
}

export interface DeletedGameMessage {
    gameId: string;
}

export interface PlayerLeftLobbyMessage {
    playerId: string;
    username: string;
}

export interface PlayerJoinedGameMessage {
    gameId: string;
    playerId: string;
    playerName: string;
}

export interface RequestGameStateMessage {
    gameId: string;
    playerId: string;
}

export interface GameStateMessage {
    gameId: string;
    players: PlayerDTO[];
    fen: string;
    teams: { [key: string]: string };
}

export interface PlayerLeftGameMessage {
    gameId: string;
    playerId: string;
    playerName: string;
}

export interface ReadyGameMessage {
    gameId: string;
    playerId: string;
    isReady: boolean;
}

export interface PlayerReadyStatusMessage {
    gameId: string;
    playersReady: PlayerDTO[];
}

export interface GameStartMessage {
    gameId: string;
    fen: string;
    teams: { [key: string]: string };
}

export interface LeaveGameMessage {
    gameId: string;
    playerId: string;
}

export interface MakeMoveMessage {
    gameId: string;
    playerId: string;
    from: string;
    to: string;
    promotion?: string; // es: "q" per regina
}

export interface MoveMadeMessage {
    gameId: string;
    playerId: string;
    from: string;
    to: string;
    fen: string; // stato completo della scacchiera dopo la mossa
}

export interface GameOverMessage {
    gameId: string;
    winnerPlayerId: string | null;
    reason: string; // es: "checkmate", "stalemate", "resignation", etc.
}