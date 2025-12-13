import { Component, OnInit, signal } from '@angular/core';
import { Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { SignalRService } from '../../services/SignalRService .service';
import { GameMode, GameRoomDTO, PlayerDTO } from '../../models/dtos';

@Component({
  selector: 'app-lobby',
  standalone: true,
  imports: [FormsModule],
  templateUrl: './lobby.html',
  styleUrls: ['./lobby.scss']
})
export class LobbyPage implements OnInit {

  players = signal<PlayerDTO[]>([]);
  games = signal<GameRoomDTO[]>([]);
  isCreateGameModalOpen = signal(false);
  isJoinGameModalOpen = signal(false);
  newGameName = "Partita_" + Math.floor(Math.random() * 1000);
  selectedMode: GameMode = GameMode.Classic1v1;
  teamSize = 1;

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
        this.router.navigate(['/game', msg.gameId], {
          state: { maxPlayers: msg.capacity }
        });
      }
    });

    const randomName = "Player_" + Math.floor(Math.random() * 10000);
    await this.ws.joinLobby(randomName);
  }

  openCreateGameModal() {
    this.newGameName = "Partita_" + Math.floor(Math.random() * 1000);
    this.isCreateGameModalOpen.set(true);
  }

  closeCreateGameModal() {
    this.isCreateGameModalOpen.set(false);
  }

  openJoinGameModal() {
    this.isJoinGameModalOpen.set(true);
  }

  closeJoinGameModal() {
    this.isJoinGameModalOpen.set(false);
  }

  async confirmCreate() {
    // Validazione base
    if (this.teamSize < 1) this.teamSize = 1;

    // Se team size > 1, forziamo la modalità TeamConsensus
    if (this.teamSize > 1) {
      this.selectedMode = GameMode.TeamConsensus;
    } else {
      this.selectedMode = GameMode.Classic1v1;
    }

    await this.ws.createGame(this.newGameName, this.selectedMode, this.teamSize);
    this.closeCreateGameModal();
  }

  async joinGame(gameId: string) {
    await this.ws.joinGame(gameId);
  }

  noGames(): boolean {
    if (this.games().length === 0) {
      return true;
    }
    for (const g of this.games()) {
      if (this.hasOpenSlot(g)) {
        return false;
      }
    }
    return true;
  }

  hasOpenSlot(game?: GameRoomDTO | null): boolean {
    if (!game) return false;

    const hasPlayers = Array.isArray(game.players);
    const hasCapacity = typeof game.capacity === 'number';
    if (game.players === undefined || game.capacity === undefined) return false;
    const hasRoom = hasPlayers && hasCapacity && game.players.length < game.capacity;

    return hasRoom;
  }
}