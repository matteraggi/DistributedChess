// player.ts
export interface Player {
    playerId: string;
    playerName: string;
}

// game.ts
export interface Game {
    gameId: string;
    gameName: string;
    initialBoard?: BoardPiece[]; // opzionale se vuoi mappare anche lo stato iniziale
}

export interface BoardPiece {
    rank: number;
    file: number;
    pieceType: string;
    pieceColor: string;
}
