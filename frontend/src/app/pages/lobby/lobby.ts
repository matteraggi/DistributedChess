import { Component, effect, OnInit, signal } from '@angular/core';
import { WebsocketService } from '../../services/websocket.service';
import { Router } from '@angular/router';

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

  constructor(private ws: WebsocketService, private router: Router) { }

  async ngOnInit() {

    // 1. Registrare handler PRIMA della connessione
    this.ws.onMessage(msg => {
      console.log('LobbyPage received message:', msg);
      switch (msg.type) {

        case 51: // LobbyStateMessage
          this.players.set(msg.players || []);
          this.games.set(msg.games || []);
          break;

        case 11: // PlayerJoinedLobbyMessage
          this.players.update(players => [...players, {
            playerId: msg.playerId,
            playerName: msg.playerName
          }]);
          break;

        case 21: // GameCreatedMessage
          this.games.update(games => [...games, {
            gameId: msg.gameId,
            gameName: msg.gameName
          }]);
          break;

        case 12: // PlayerLeftLobbyMessage
          this.players.update(players => players.filter(p => p.playerId !== msg.playerId));
          break;

        case 23: // GameRemovedMessage
          this.router.navigate(['/game', msg.gameId]);
          break;

        case 26: // GameRemovedMessage
          this.games.update(games =>
            games.filter(g => g.gameId !== msg.gameId)
          );
          break;

        default:
          break;
      }
    });

    // 2. Collegarsi e joinare la lobby
    await this.ws.joinLobby("Player_" + Math.floor(Math.random() * 10000));
  }

  async createGame() {
    await this.ws.send({
      type: 20,
      gameName: "Partita_" + Math.floor(Math.random() * 10000),
    });
  }

  async joinGame(gameId: string) {
    await this.ws.send({
      type: 22,     // MessageType.JoinGame
      gameId: gameId
    });
  }

}
