import { Component, OnInit, signal } from '@angular/core';
import { Router } from '@angular/router';
import { SignalRService } from '../../services/SignalRService .service';
import { GameRoomDTO, PlayerDTO } from '../../models/dtos';

@Component({
  selector: 'app-lobby',
  standalone: true,
  imports: [],
  templateUrl: './lobby.html',
  styleUrls: ['./lobby.sass']
})
export class LobbyPage implements OnInit {

  players = signal<PlayerDTO[]>([]);
  games = signal<GameRoomDTO[]>([]);

  constructor(private ws: SignalRService, private router: Router) { }

  async ngOnInit() {

    this.ws.lobbyState$.subscribe(msg => {
      this.players.set(msg.players || []);
      this.games.set(msg.games || []);
    });

    this.ws.playerJoined$.subscribe(msg => {
      this.players.update(p => {
        if (!p.some(x => x.playerId === msg.playerId)) {
          return [...p, { playerId: msg.playerId, playerName: msg.playerName }];
        }
        return p;
      });
    });

    this.ws.playerLeft$.subscribe(msg => {
      this.players.update(p => p.filter(x => x.playerId !== msg.playerId));
    });

    this.ws.gameCreated$.subscribe(msg => {
      this.games.update(g => [...g, { gameId: msg.gameId, gameName: msg.gameName }]);
    });

    this.ws.gameRemoved$.subscribe(msg => {
      this.games.update(g => g.filter(x => x.gameId !== msg.gameId));
    });

    this.ws.playerJoinedGame$.subscribe(msg => {
      if (msg.playerId === this.ws.getOrCreatePlayerId()) {
        this.router.navigate(['/game', msg.gameId]);
      }
    });

    const randomName = "Player_" + Math.floor(Math.random() * 10000);
    await this.ws.joinLobby(randomName);
  }

  async createGame() {
    const gameName = "Partita_" + Math.floor(Math.random() * 10000);
    await this.ws.createGame(gameName);
  }

  async joinGame(gameId: string) {
    await this.ws.joinGame(gameId);
  }
}