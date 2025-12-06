import { Injectable, OnDestroy } from '@angular/core';
import { HubConnection, HubConnectionBuilder, LogLevel } from '@microsoft/signalr';
import { Subject } from 'rxjs';
import { environment } from '../../environments/environment';
import { DeletedGameMessage, GameCreatedMessage, GameStartMessage, GameStateMessage, LobbyStateMessage, PlayerJoinedGameMessage, PlayerJoinedLobbyMessage, PlayerLeftGameMessage, PlayerLeftLobbyMessage, PlayerReadyStatusMessage, MakeMoveMessage, MoveMadeMessage, GameOverMessage, ActiveProposalsUpdateMessage, ProposeMoveMessage, VoteMessage, GameMode } from '../models/dtos';

@Injectable({ providedIn: 'root' })
export class SignalRService implements OnDestroy {

    public hubConnection: HubConnection | undefined;

    // sostituiscono i tipi
    public lobbyState$ = new Subject<LobbyStateMessage>();
    public playerJoined$ = new Subject<PlayerJoinedLobbyMessage>();
    public playerLeft$ = new Subject<PlayerLeftLobbyMessage>();
    public gameCreated$ = new Subject<GameCreatedMessage>();
    public gameRemoved$ = new Subject<DeletedGameMessage>();
    public playerJoinedGame$ = new Subject<PlayerJoinedGameMessage>();
    public gameState$ = new Subject<GameStateMessage>();
    public playerLeftGame$ = new Subject<PlayerLeftGameMessage>();
    public playerReadyStatus$ = new Subject<PlayerReadyStatusMessage>();
    public gameStart$ = new Subject<GameStartMessage>();
    public moveMade$ = new Subject<MoveMadeMessage>();
    public gameOver$ = new Subject<GameOverMessage>();
    public activeProposals$ = new Subject<ActiveProposalsUpdateMessage>();
    public proposalRejected$ = new Subject<any>();

    constructor() { }

    public async startConnection(): Promise<void> {
        if (this.hubConnection && this.hubConnection.state !== 'Disconnected') {
            return;
        }

        // SignalR usa HTTP per negoziare, quindi convertiamo ws:// in http:// se necessario
        // Il backend mappa l'hub su "/ws"
        const url = environment.wsUrl.replace('ws://', 'http://');

        this.hubConnection = new HubConnectionBuilder()
            .withUrl(url) // es: http://localhost:5001/ws
            .withAutomaticReconnect() // riprova se cade la linea
            .configureLogging(LogLevel.Information)
            .build();

        this.hubConnection.on('ReceiveLobbyState', (data: LobbyStateMessage) => {
            console.log('Lobby State received:', data);
            this.lobbyState$.next(data);
        });

        this.hubConnection.on('PlayerJoined', (data: PlayerJoinedLobbyMessage) => {
            console.log('Player Joined Lobby:', data);
            this.playerJoined$.next(data);
        });

        this.hubConnection.on('PlayerLeft', (data: PlayerLeftLobbyMessage) => {
            console.log('Player Left Lobby:', data);
            this.playerLeft$.next(data);
        });

        this.hubConnection.on('GameCreated', (data: GameCreatedMessage) => {
            console.log('Game Created:', data);
            this.gameCreated$.next(data);
        });

        this.hubConnection.on('DeletedGame', (data: DeletedGameMessage) => {
            console.log('Game Deleted:', data);
            this.gameRemoved$.next(data);
        });

        this.hubConnection.on('PlayerJoinedGame', (data: PlayerJoinedGameMessage) => {
            console.log('Player Joined Game:', data);
            this.playerJoinedGame$.next(data);
        });

        this.hubConnection.on('ReceiveGameState', (data: GameStateMessage) => {
            console.log('Game State received:', data);
            this.gameState$.next(data);
        });

        this.hubConnection.on('PlayerLeftGame', (data: PlayerLeftGameMessage) => {
            console.log('Player Left Game:', data);
            this.playerLeftGame$.next(data);
        });

        this.hubConnection.on('PlayerReadyStatus', (data: PlayerReadyStatusMessage) => {
            console.log('Player Ready Status:', data);
            this.playerReadyStatus$.next(data);
        });

        this.hubConnection.on('GameStart', (data: GameStartMessage) => {
            console.log('Game Start:', data);
            this.gameStart$.next(data);
        });

        this.hubConnection.on('MoveMade', (data: MoveMadeMessage) => {
            console.log('Move Made:', data);
            this.moveMade$.next(data);
        });

        this.hubConnection.on('GameOver', (data: GameOverMessage) => {
            this.gameOver$.next(data);
        });

        this.hubConnection.on('ActiveProposalsUpdate', (data: ActiveProposalsUpdateMessage) => {
            this.activeProposals$.next(data);
        });

        this.hubConnection.on('ProposalRejected', (data: any) => {
            this.proposalRejected$.next(data);
        });

        try {
            await this.hubConnection.start();
            console.log('SignalR Connected!');
        } catch (err) {
            console.error('Error while starting SignalR connection:', err);
        }
    }

    public async joinLobby(playerName?: string) {
        await this.startConnection();

        const playerId = this.getOrCreatePlayerId();
        let name = localStorage.getItem("playerName");

        if (!name && playerName) {
            name = playerName;
            localStorage.setItem("playerName", name);
        }

        if (this.hubConnection) {
            await this.hubConnection.invoke('JoinLobby', {
                playerId: playerId,
                playerName: name || 'Ospite'
            });
        }
    }

    public async createGame(gameName: string, mode: GameMode, teamSize: number) {
        if (!this.hubConnection) return;

        await this.hubConnection.invoke('CreateGame', {
            gameName: gameName,
            playerId: this.getOrCreatePlayerId(),
            mode: mode,
            teamSize: teamSize
        });
    }

    public async joinGame(gameId: string) {
        if (!this.hubConnection) return;

        await this.hubConnection.invoke('JoinGame', {
            gameId: gameId,
            playerId: this.getOrCreatePlayerId()
        });
    }

    public async requestGameState(gameId: string) {
        if (!this.hubConnection) return;
        await this.hubConnection.invoke('RequestGameState', {
            gameId: gameId,
            playerId: this.getOrCreatePlayerId()
        });
    }

    public async leaveGame(gameId: string) {
        if (!this.hubConnection) return;
        await this.hubConnection.invoke('LeaveGame', {
            gameId: gameId,
            playerId: this.getOrCreatePlayerId()
        });
    }

    public async readyGame(gameId: string, isReady: boolean) {
        if (!this.hubConnection) return;
        await this.hubConnection.invoke('ReadyGame', {
            gameId: gameId,
            playerId: this.getOrCreatePlayerId(),
            isReady: isReady
        });
    }

    public async makeMove(gameId: string, from: string, to: string, promotion: string = '') {
        if (!this.hubConnection) return;

        await this.hubConnection.invoke('MakeMove', {
            gameId,
            playerId: this.getOrCreatePlayerId(),
            from,
            to,
            promotion
        });
    }

    public async proposeMove(gameId: string, from: string, to: string, promotion: string = '') {
        if (!this.hubConnection) return;
        await this.hubConnection.invoke('ProposeMove', {
            gameId,
            playerId: this.getOrCreatePlayerId(),
            from,
            to,
            promotion
        });
    }

    public async voteMove(gameId: string, proposalId: string, isApproved: boolean = true) {
        if (!this.hubConnection) return;
        await this.hubConnection.invoke('VoteMove', {
            gameId,
            proposalId,
            isApproved // true = Voto per questa proposta
        });
    }

    getOrCreatePlayerId(): string {
        let id = localStorage.getItem("playerId");
        if (!id) {
            id = crypto.randomUUID();
            localStorage.setItem("playerId", id);
        }
        return id;
    }

    ngOnDestroy(): void {
        this.hubConnection?.stop();
    }
}