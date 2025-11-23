import { Component, effect, OnInit, signal } from '@angular/core';
import { WebsocketService } from '../../services/websocket.service';

@Component({
  selector: 'app-lobby',
  standalone: true,
  imports: [],
  templateUrl: './lobby.html',
  styleUrls: ['./lobby.sass']
})
export class LobbyPage implements OnInit {

  players = signal<any[]>([]);
  games = signal<any[]>([]);

  constructor(private ws: WebsocketService) { }

  async ngOnInit() {

    // 1. Registrare handler PRIMA della connessione
    this.ws.onMessage(msg => {
      console.log('LobbyPage received message:', msg);
      switch (msg.type) {

        case 51:
          this.players.set(msg.players || []);
          this.games.set(msg.games || []);
          break;

        case 11:
          this.players.update(players => [...players, {
            playerId: msg.playerId,
            playerName: msg.playerName
          }]);
          break;

        case 21:
          this.games.update(games => [...games, {
            gameId: msg.gameId,
            gameName: msg.gameName
          }]);
          break;

        case 12:
          this.players.update(players => players.filter(p => p.playerId !== msg.playerId));
          break;

        default:
          console.warn('Unhandled message type in LobbyPage:', msg.type);
          break;
      }
    });

    // 2. Collegarsi e joinare la lobby
    await this.ws.joinLobby("Player_" + Math.floor(Math.random() * 10000));
  }
}
