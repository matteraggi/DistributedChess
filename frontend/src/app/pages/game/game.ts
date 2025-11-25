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

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private ws: WebsocketService
  ) { }
  async ngOnInit() {
    window.addEventListener('beforeunload', this.onWindowUnload);

    this.gameId = this.route.snapshot.paramMap.get('id')!;

    this.ws.send({ type: 22, gameId: this.gameId });

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

}
