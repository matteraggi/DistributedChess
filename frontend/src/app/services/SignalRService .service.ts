import { Injectable, OnDestroy } from '@angular/core';
import { HubConnection, HubConnectionBuilder, LogLevel } from '@microsoft/signalr';
import { Subject } from 'rxjs';
import { environment } from '../../environments/environment';
import { DeletedGameMessage, GameCreatedMessage, GameStartMessage, GameStateMessage, LobbyStateMessage, PlayerJoinedGameMessage, PlayerJoinedLobbyMessage, PlayerLeftGameMessage, PlayerLeftLobbyMessage, PlayerReadyStatusMessage } from '../models/dtos';

@Injectable({ providedIn: 'root' })
export class SignalRService implements OnDestroy {

    public hubConnection: HubConnection | undefined;

    // sostituiscono i tipi
    public lobbyState$ = new Subject<LobbyStateMessage>();      // type 51
    public playerJoined$ = new Subject<PlayerJoinedLobbyMessage>();    // type 11
    public playerLeft$ = new Subject<PlayerLeftLobbyMessage>();      // type 12
    public gameCreated$ = new Subject<GameCreatedMessage>();     // type 21
    public gameRemoved$ = new Subject<DeletedGameMessage>();     // type 26
    public playerJoinedGame$ = new Subject<PlayerJoinedGameMessage>(); // type 23
    public gameState$ = new Subject<GameStateMessage>();
    public playerLeftGame$ = new Subject<PlayerLeftGameMessage>();
    public playerReadyStatus$ = new Subject<PlayerReadyStatusMessage>();
    public gameStart$ = new Subject<GameStartMessage>();

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

    public async createGame(gameName: string) {
        if (!this.hubConnection) return;

        await this.hubConnection.invoke('CreateGame', {
            gameName: gameName,
            playerId: this.getOrCreatePlayerId()
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