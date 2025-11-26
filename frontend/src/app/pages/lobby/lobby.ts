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

    // LobbyStateMessage
    this.ws.onType(51).subscribe(msg => {
      this.players.set(msg.players || []);
      this.games.set(msg.games || []);
    });

    // PlayerJoinedLobbyMessage
    this.ws.onType(11).subscribe(msg => {
      this.players.update(p => {
        // se non esiste già il player con questo id, aggiungilo
        if (!p.some(x => x.playerId === msg.playerId)) {
          return [...p, { playerId: msg.playerId, playerName: msg.playerName }];
        }
        return p; // altrimenti ritorna l'array invariato
      });
    });

    // GameCreatedMessage
    this.ws.onType(21).subscribe(msg => {
      this.games.update(g => [...g, { gameId: msg.gameId, gameName: msg.gameName }]);
    });

    // PlayerLeftLobbyMessage
    this.ws.onType(12).subscribe(msg => {
      this.players.update(p => p.filter(x => x.playerId !== msg.playerId));
    });

    // PlayerJoinedGame → naviga a game
    this.ws.onType(23).subscribe(msg => {
      this.router.navigate(['/game', msg.gameId]);
    });

    // GameRemovedMessage
    this.ws.onType(26).subscribe(msg => {
      this.games.update(g => g.filter(x => x.gameId !== msg.gameId));
    });

    await this.ws.joinLobby("Player_" + Math.floor(Math.random() * 10000));
  }


  async createGame() {
    await this.ws.send({
      type: 20,
      gameName: "Partita_" + Math.floor(Math.random() * 10000),
      playerId: this.ws.getOrCreatePlayerId()
    });
  }

  async joinGame(gameId: string) {
    await this.ws.send({
      type: 22,     // MessageType.JoinGame
      gameId: gameId,
      playerId: this.ws.getOrCreatePlayerId()
    });
  }

}
