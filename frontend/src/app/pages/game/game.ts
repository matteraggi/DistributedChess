import { Component, OnInit, signal, computed } from '@angular/core';
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
  capacity = signal<number>(2);

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
    const nav = this.router.currentNavigation();
    const state = nav?.extras.state as { capacity: number };

    if (state && state.capacity) {
      this.capacity.set(state.capacity);
    }
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
      this.capacity.set(msg.capacity);
      const initialReadySet = new Set<string>();
      msg.players.forEach(p => {
        if (p.isReady) {
          initialReadySet.add(p.playerId);
        }
      });
      this.readyPlayers.set(initialReadySet);
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

  readyPercentage = computed(() => {
    const total = this.players().length;
    if (total === 0) return 0;
    const ready = this.readyPlayers().size;
    return (ready / total) * 100;
  });

  /* 
  Per evitare che ricaricando la pagina il giocatore non trovi più il gioco essendo stato eliminato
  
    ngOnDestroy() {
      this.sendLeaveOnce();
      window.removeEventListener('beforeunload', this.onWindowUnload);
    }
  
    onWindowUnload = () => {
      this.sendLeaveOnce();
    };
  */
}