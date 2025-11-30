import { Component, OnInit, OnDestroy, signal, effect } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { PlayerDTO } from '../../models/dtos';
import { SignalRService } from '../../services/SignalRService .service';

@Component({
  selector: 'app-game',
  standalone: true,
  templateUrl: './game.html',
  styleUrls: ['./game.sass']
})
export class Game implements OnInit {

  gameId!: string;

  players = signal<PlayerDTO[]>([]);
  readyPlayers = signal<Set<string>>(new Set());
  amIReady = signal<boolean>(false);

  private hasLeft = false;
  private myPlayerId: string;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private ws: SignalRService
  ) {
    this.myPlayerId = this.ws.getOrCreatePlayerId();
  }

  async ngOnInit() {
    //window.addEventListener('beforeunload', this.onWindowUnload);

    this.gameId = this.route.snapshot.paramMap.get('id')!;

    this.ws.gameState$.subscribe(msg => {
      const uniquePlayers = msg.players.filter(
        (p, index, self) =>
          index === self.findIndex(t => t.playerId === p.playerId)
      );
      this.players.set(uniquePlayers);

      // da espandere con lo stato IsReady.
    });

    this.ws.playerJoinedGame$.subscribe(msg => {
      if (msg.gameId !== this.gameId) return;

      this.players.update(players => {
        if (!players.some(p => p.playerId === msg.playerId)) {
          return [...players, { playerId: msg.playerId, playerName: msg.playerName }];
        }
        return players;
      });
    });

    this.ws.playerLeftGame$.subscribe(msg => {
      if (msg.gameId !== this.gameId) return;
      this.players.update(players => players.filter(p => p.playerId !== msg.playerId));

      this.readyPlayers.update(set => {
        const newSet = new Set(set);
        newSet.delete(msg.playerId);
        return newSet;
      });
    });

    this.ws.playerReadyStatus$.subscribe(msg => {
      if (msg.gameId !== this.gameId) return;

      const newReadySet = new Set<string>();

      msg.playersReady.forEach(p => {
        if (p.isReady) {
          newReadySet.add(p.playerId);
        }
      });

      this.readyPlayers.set(newReadySet);
    });

    this.ws.gameStart$.subscribe(msg => {
      if (msg.gameId !== this.gameId) return;
      this.router.navigate(['/board', this.gameId]);
    });


    await this.ws.startConnection();
    await this.ws.requestGameState(this.gameId);
  }

  async toggleReady() {
    const newState = !this.amIReady();
    this.amIReady.set(newState);

    await this.ws.readyGame(this.gameId, newState);
  }

  async sendLeaveOnce() {
    if (this.hasLeft) return;
    this.hasLeft = true;

    try {
      await this.ws.leaveGame(this.gameId);
    } catch (e) {
      console.error("Errore durante leaveGame", e);
    }
  }

  goBack() {
    this.sendLeaveOnce();
    this.router.navigate(['/lobby']);
  }
  /*
    ngOnDestroy() {
      this.sendLeaveOnce();
      window.removeEventListener('beforeunload', this.onWindowUnload);
    }
  
    onWindowUnload = () => {
      this.sendLeaveOnce();
    };
    */
}