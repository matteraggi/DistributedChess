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

// Rispecchia PlayerJoinedLobbyMessage.cs
export interface PlayerJoinedLobbyMessage {
    playerId: string;
    playerName: string;
}

// Rispecchia GameCreatedMessage.cs
export interface GameCreatedMessage {
    gameId: string;
    gameName: string;
}

// Rispecchia DeletedGameMessage.cs
export interface DeletedGameMessage {
    gameId: string;
}

// Rispecchia PlayerLeftLobbyMessage.cs
export interface PlayerLeftLobbyMessage {
    playerId: string;
    username: string;
}

// Rispecchia PlayerJoinedGameMessage.cs
export interface PlayerJoinedGameMessage {
    gameId: string;
    playerId: string;
    playerName: string;
}

// Rispecchia RequestGameStateMessage (Output)
export interface RequestGameStateMessage {
    gameId: string;
    playerId: string;
}

// Rispecchia GameStateMessage (Input)
export interface GameStateMessage {
    gameId: string;
    players: PlayerDTO[];
}

// Rispecchia PlayerLeftGameMessage
export interface PlayerLeftGameMessage {
    gameId: string;
    playerId: string;
    playerName: string;
}

// Rispecchia ReadyGameMessage (Output)
export interface ReadyGameMessage {
    gameId: string;
    playerId: string;
    isReady: boolean;
}

// Rispecchia PlayerReadyStatusMessage (Input)
export interface PlayerReadyStatusMessage {
    gameId: string;
    playersReady: PlayerDTO[];
}

// Rispecchia GameStartMessage
export interface GameStartMessage {
    gameId: string;
}

// Rispecchia LeaveGameMessage (Output)
export interface LeaveGameMessage {
    gameId: string;
    playerId: string;
}