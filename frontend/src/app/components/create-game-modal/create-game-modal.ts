import { Component, EventEmitter, Output, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { SignalRService } from '../../services/SignalRService .service';
import { GameMode } from '../../models/dtos';

@Component({
  selector: 'app-create-game-modal',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './create-game-modal.html',
  styleUrls: ['./create-game-modal.scss']
})
export class CreateGameModal {
  @Output() close = new EventEmitter<void>();
  ws = inject(SignalRService);

  newGameName = "Partita_" + Math.floor(Math.random() * 1000);
  teamSize = 1;

  async confirmCreate() {
    let selectedMode = GameMode.Classic1v1;

    if (this.teamSize < 1) this.teamSize = 1;
    if (this.teamSize > 1) {
      selectedMode = GameMode.TeamConsensus;
    }

    await this.ws.createGame(this.newGameName, selectedMode, this.teamSize);
    this.close.emit();
  }

  onOverlayClick() {
    this.close.emit();
  }
}
