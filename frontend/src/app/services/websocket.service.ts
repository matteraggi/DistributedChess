import { Injectable, OnDestroy } from '@angular/core';
import { Subject, Observable } from 'rxjs';
import { filter } from 'rxjs/operators';

@Injectable({ providedIn: 'root' })
export class WebsocketService implements OnDestroy {

    private connectionPromise?: Promise<void>;
    private socket?: WebSocket;

    private messageSubject = new Subject<any>();
    messages$ = this.messageSubject.asObservable();

    constructor() { }

    connect(url: string = 'ws://localhost:5164/ws'): Promise<void> {
        if (this.connectionPromise) return this.connectionPromise;

        this.connectionPromise = new Promise((resolve, reject) => {
            this.socket = new WebSocket(url);

            this.socket.onopen = () => {
                console.log('WebSocket connected');
                resolve();
            };

            this.socket.onmessage = (event) => {
                const msg = JSON.parse(event.data);
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

    onType(type: number): Observable<any> {
        return this.messages$.pipe(filter(msg => msg.type === type));
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
