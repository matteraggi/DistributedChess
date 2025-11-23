import { Injectable, signal } from '@angular/core';
import { WebsocketService } from './websocket.service';
import { Game, Player } from '../models/game';

@Injectable({ providedIn: 'root' })
export class LobbyStateService {

  players = signal<Player[]>([]);
  games = signal<Game[]>([]);

  constructor(private ws: WebsocketService) {
    ws.onMessage((msg) => this.handle(msg));
  }

  async joinLobby(playerName: string) {
    // Ensure the WebSocket connection is established before sending the join message
    try {
      await this.ws.connect();
    } catch (err) {
      console.error('Failed to connect WebSocket before joining lobby', err);
      return;
    }

    await this.ws.send({
      type: 10,
      playerName
    });
  }

  private handle(msg: any) {

    switch (msg.type) {

      // PlayerJoinedLobby
      case 11:
        this.players.set([...this.players(), {
          playerId: msg.playerId,
          playerName: msg.playerName
        }]);
        break;

      // GameCreated
      case 21:
        this.games.set([...this.games(), {
          gameId: msg.gameId,
          gameName: msg.gameName
        }]);
        break;

      default:
        break;
    }
  }
}
