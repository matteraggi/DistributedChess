import { Injectable, OnDestroy } from '@angular/core';
import { Subject } from 'rxjs';

@Injectable({
    providedIn: 'root'
})
export class WebsocketService implements OnDestroy {

    private connectionPromise?: Promise<void>;
    private socket?: WebSocket;
    private messageSubject = new Subject<any>();
    private messageHandlers: ((msg: any) => void)[] = [];

    messages$ = this.messageSubject.asObservable();

    onMessage(handler: (msg: any) => void) {
        this.messageHandlers.push(handler);
    }

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
                this.messageHandlers.forEach(h => h(msg));
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
        if (this.connectionPromise) {
            await this.connectionPromise;
        }
        if (!this.socket || this.socket.readyState !== WebSocket.OPEN) {
            console.warn('WebSocket not connected, cannot send');
            return;
        }
        this.socket.send(JSON.stringify(message));
    }

    ngOnDestroy(): void {
        this.socket?.close();
    }


    async joinLobby(playerName: string) {
        await this.connect();
        await this.send({
            type: 10,
            playerName
        });
    }

}
