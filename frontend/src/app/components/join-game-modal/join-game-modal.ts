import { Component, EventEmitter, Output, Input, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { SignalRService } from '../../services/SignalRService .service';
import { GameRoomDTO } from '../../models/dtos';

@Component({
  selector: 'app-join-game-modal',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './join-game-modal.html',
  styleUrls: ['./join-game-modal.scss']
})
export class JoinGameModal {
  @Input() games: GameRoomDTO[] = [];
  @Output() close = new EventEmitter<void>();
  ws = inject(SignalRService);

  async joinGame(gameId: string, gameName: string) {
    await this.ws.joinGame(gameId, gameName);
  }

  hasOpenSlot(game?: GameRoomDTO | null): boolean {
    if (!game) return false;
    const hasPlayers = Array.isArray(game.players);
    const hasCapacity = typeof game.capacity === 'number';
    if (game.players === undefined || game.capacity === undefined) return false;
    return hasPlayers && hasCapacity && game.players.length < game.capacity;
  }

  noGames(): boolean {
    if (this.games.length === 0) return true;
    return !this.games.some(g => this.hasOpenSlot(g));
  }

  onOverlayClick() {
    this.close.emit();
  }
}
