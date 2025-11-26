// src/app/pages/game/game.ts
import { Component, OnInit, signal } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { WebsocketService } from '../../services/websocket.service';
import { Player } from '../../models/player';

@Component({
  selector: 'app-game',
  standalone: true,
  templateUrl: './game.html',
  styleUrls: ['./game.sass']
})
export class Game implements OnInit {

  gameId!: string;
  players = signal<Player[]>([]);
  readyPlayers = signal<Set<string>>(new Set());
  amIReady = signal<boolean>(false);

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private ws: WebsocketService
  ) { }
  async ngOnInit() {
    window.addEventListener('beforeunload', this.onWindowUnload);

    this.gameId = this.route.snapshot.paramMap.get('id')!;

    await this.ws.send({
      type: 53,
      gameId: this.gameId,
      playerId: this.ws.getOrCreatePlayerId()
    });

    // GameStateMessage
    this.ws.onType(52).subscribe(msg => {
      const uniquePlayers = msg.players.filter(
        (p: any, index: number, self: any[]) =>
          index === self.findIndex(t => t.playerId === p.playerId)
      );
      this.players.set(uniquePlayers);
    });

    // PlayerJoinedGameMessage
    this.ws.onType(23).subscribe(msg => {
      this.players.update(players => {
        if (!players.some(p => p.playerId === msg.playerId)) {
          return [...players, { playerId: msg.playerId, playerName: msg.playerName }];
        }
        return players;
      });
    });

    // PlayerLeftGameMessage
    this.ws.onType(24).subscribe(msg => {
      this.players.update(players => players.filter(p => p.playerId !== msg.playerId));
    });

    // 31 → PlayerReadyStatusMessage
    this.ws.onType(31).subscribe((msg: { gameId: string, readyPlayers: string[] }) => {
      if (msg.gameId !== this.gameId) return; // sicurezza: solo per questa partita
      // aggiorna la signal dei giocatori ready
      this.readyPlayers.set(new Set(msg.readyPlayers));
    });

    // 32 → GameStartMessage
    this.ws.onType(32).subscribe((msg: { gameId: string }) => {
      if (msg.gameId !== this.gameId) return;
      this.router.navigate(['/board', this.gameId]);
    });

  }

  trackByPlayerId(player: any) {
    return player.playerId;
  }

  ngOnDestroy() {
    this.ws.send({ type: 25, gameId: this.gameId });
    window.removeEventListener('beforeunload', this.onWindowUnload);
  }

  onWindowUnload = () => {
    this.ws.send({ type: 25, gameId: this.gameId });
  };

  goBack() {
    this.ws.send({ type: 25, gameId: this.gameId });
    this.router.navigate(['/lobby']);
  }

  toggleReady() {

    this.ws.send({ type: 30, gameId: this.gameId, playerId: this.ws.getOrCreatePlayerId(), ready: !this.amIReady() });
    this.amIReady.set(!this.amIReady());
  }
}