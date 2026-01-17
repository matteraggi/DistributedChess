import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-game-result-notification',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './game-result-notification.html',
  styleUrls: ['./game-result-notification.scss']
})
export class GameResultNotification {
  @Input() message: string = '';
  @Input() reason: string = '';
  @Input() countdown: number = 0;
  @Input() isVictory: boolean = false;

  @Output() close = new EventEmitter<void>();
}
