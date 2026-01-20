import { Component, OnInit, signal } from '@angular/core';
import { Router } from '@angular/router';
import { SignalRService } from '../../services/SignalRService .service';
import { GameMode, GameRoomDTO, PlayerDTO } from '../../models/dtos';
import { CreateGameModal } from '../../components/create-game-modal/create-game-modal';
import { JoinGameModal } from '../../components/join-game-modal/join-game-modal';

@Component({
  selector: 'app-lobby',
  standalone: true,
  imports: [CreateGameModal, JoinGameModal],
  templateUrl: './lobby.html',
  styleUrls: ['./lobby.scss']
})
export class LobbyPage implements OnInit {

  players = signal<PlayerDTO[]>([]);
  games = signal<GameRoomDTO[]>([]);
  isCreateGameModalOpen = signal(false);
  isJoinGameModalOpen = signal(false);

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
      this.games.update(g => [
        ...g,
        {
          gameId: msg.gameId,
          gameName: msg.gameName,
          capacity: msg.capacity,
          players: [{
            playerId: msg.creatorId,
            playerName: msg.creatorName
          }]
        }
      ]);
    });

    this.ws.gameRemoved$.subscribe(msg => {
      this.games.update(g => g.filter(x => x.gameId !== msg.gameId));
    });

    this.ws.playerJoinedGame$.subscribe(msg => {
      if (msg.playerId === this.ws.getOrCreatePlayerId()) {
        this.router.navigate(['/game', msg.gameId], {
          state: { maxPlayers: msg.capacity }
        });
        return;
      }
      this.games.update(currentGames => {
        return currentGames.map(g => {
          if (g.gameId === msg.gameId) {
            const currentPlayers = g.players || [];
            if (!currentPlayers.some(p => p.playerId === msg.playerId)) {
              const updatedPlayers = [...currentPlayers, {
                playerId: msg.playerId,
                playerName: msg.playerName
              }];
              return { ...g, players: updatedPlayers };
            }
          }
          return g;
        });
      });
    });

    this.ws.playerLeftGame$.subscribe(msg => {
      // Aggiorna il contatore decrementandolo
      this.games.update(currentGames => {
        return currentGames.map(g => {
          if (g.gameId === msg.gameId) {
            // Rimuovi il giocatore dalla lista locale
            const updatedPlayers = (g.players || []).filter(p => p.playerId !== msg.playerId);
            return { ...g, players: updatedPlayers };
          }
          return g;
        });
      });
    });


    const randomName = "Player_" + Math.floor(Math.random() * 10000);
    await this.ws.joinLobby(randomName);
  }

  openCreateGameModal() {
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
}