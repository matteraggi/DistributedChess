import { Component, EventEmitter, Output, Input, inject } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
    selector: 'app-leave-game-modal',
    standalone: true,
    imports: [CommonModule],
    templateUrl: './leave-game-modal.html',
    styleUrls: ['./leave-game-modal.scss']
})
export class LeaveGameModal {
    @Input() isLastPlayer: boolean = false;

    @Output() confirmLeave = new EventEmitter<void>();
    @Output() confirmDelete = new EventEmitter<void>();
    @Output() cancel = new EventEmitter<void>();

    onCancel() {
        this.cancel.emit();
    }

    onLeave() {
        this.confirmLeave.emit();
    }

    onDelete() {
        this.confirmDelete.emit();
    }
}
