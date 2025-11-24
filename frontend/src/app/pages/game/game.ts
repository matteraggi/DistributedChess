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
export class GamePage implements OnInit {

  gameId!: string;
  players = signal<Player[]>([]);

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private ws: WebsocketService
  ) { }

  async ngOnInit() {
    this.gameId = this.route.snapshot.paramMap.get('id')!;

    this.ws.send({
      type: 22,         // JoinGame
      gameId: this.gameId
    });


    // 1. Registrare handler PRIMA della richiesta di join
    this.ws.onMessage(msg => {
      console.log('GamePage received message:', msg);

      switch (msg.type) {

        case 52: // GameStateMessage
          const uniquePlayers = msg.players.filter(
            (p: any, index: number, self: any[]) =>
              index === self.findIndex(t => t.playerId === p.playerId)
          );
          this.players.set(uniquePlayers);
          break;

        case 23: // PlayerJoinedGameMessage
          this.players.update(players => {
            if (!players.some(p => p.playerId === msg.playerId)) {
              return [...players, { playerId: msg.playerId, playerName: msg.playerName }];
            }
            return players;
          });
          break;

        case 24: // PlayerLeftGameMessage
          this.players.update(players => players.filter(p => p.playerId !== msg.playerId));
          break;

        default:
          break;
      }
    });

  }
  trackByPlayerId(player: any) {
    return player.playerId;
  }

}
