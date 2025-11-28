import { Injectable, OnDestroy } from '@angular/core';
import { Subject, tap } from 'rxjs';
import { filter } from 'rxjs/operators';
import { environment } from '../../environments/environment';


@Injectable({ providedIn: 'root' })
export class WebsocketService implements OnDestroy {

    private connectionPromise?: Promise<void>;
    private socket?: WebSocket;

    private messageSubject = new Subject<any>();
    messages$ = this.messageSubject.asObservable();
    private lastGameState: Map<string, any> = new Map();

    constructor() { }

    connect(url: string = environment.wsUrl): Promise<void> {
        if (this.connectionPromise) return this.connectionPromise;

        this.connectionPromise = new Promise((resolve, reject) => {
            this.socket = new WebSocket(url);

            this.socket.onopen = () => {
                console.log('WebSocket connected');
                resolve();
            };

            this.socket.onmessage = (event) => {
                const msg = JSON.parse(event.data);
                console.log('WebSocket message received', msg);
                this.messageSubject.next(msg);
            };

            this.socket.onclose = () => {
                console.log('WebSocket closed');
                this.socket = undefined;
                this.connectionPromise = undefined;
            };

            this.socket.onerror = (err) => {
                console.error('WebSocket error', err);
                reject(err);
                this.connectionPromise = undefined;
            };
        });
        return this.connectionPromise;
    }

    async send(message: any): Promise<void> {
        console.log('Sending message', message);

        if (this.connectionPromise) await this.connectionPromise;

        if (!this.socket || this.socket.readyState !== WebSocket.OPEN) {
            console.warn('WebSocket not connected, cannot send');
            return;
        }
        this.socket.send(JSON.stringify(message));
    }

    ngOnDestroy(): void {
        this.socket?.close();
    }

    onType(type: number) {
        return this.messages$.pipe(
            filter(m => m.type === type),
            tap(msg => {
                if (type === 52) {
                    this.lastGameState.set(msg.gameId, msg);
                }
            })
        );
    }

    getCachedGameState(gameId: string) {
        return this.lastGameState.get(gameId);
    }

    getOrCreatePlayerId(): string {
        let id = localStorage.getItem("playerId");
        if (!id) {
            id = crypto.randomUUID();
            localStorage.setItem("playerId", id);
        }
        return id;
    }

    async joinLobby(playerName?: string) {
        await this.connect();

        let playerId = localStorage.getItem("playerId");
        let name = localStorage.getItem("playerName");

        if (!playerId) {
            playerId = crypto.randomUUID();
            localStorage.setItem("playerId", playerId);
        }

        if (!name && playerName) {
            name = playerName;
            localStorage.setItem("playerName", name);
        }

        await this.send({
            type: 10,          // JoinLobby
            playerId,
            playerName: name
        });
    }

}
