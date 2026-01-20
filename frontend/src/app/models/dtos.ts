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

export interface CreateGameMessage {
    gameName: string;
    playerId: string;
    mode: GameMode;
    teamSize: number;
}

export interface GameCreatedMessage {
    gameId: string;
    gameName: string;
    capacity: number;
    creatorId: string;
    creatorName: string;
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
    capacity?: number;
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
    lastMoveAt: string;
    mode: GameMode;
    piecePermission: { [key: string]: string[] }; // Mappa ID -> Array di char ['P', 'K']
    activeProposals: MoveProposal[];
    capacity: number;
    gameName: string;
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

export enum GameMode {
    Classic1v1 = 0,
    TeamConsensus = 1
}

export interface MoveProposal {
    proposalId: string;
    proposerId: string;
    from: string;
    to: string;
    promotion: string;
    votes: string[];
    createdAt: string;
}

export interface ActiveProposalsUpdateMessage {
    gameId: string;
    proposals: MoveProposal[];
}

export interface ProposalResultMessage {
    gameId: string;
    proposalId: string;
    isAccepted: boolean;
    reason: string;
}
export interface JoinGameMessage {
    gameId: string;
    playerId: string;
}

export interface ProposeMoveMessage {
    gameId: string;
    playerId: string;
    from: string;
    to: string;
    promotion: string;
}

export interface VoteMessage {
    gameId: string;
    proposalId: string;
    isApproved: boolean;
}