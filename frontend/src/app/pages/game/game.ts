// src/app/pages/game/game.ts
import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';

@Component({
  selector: 'app-game',
  standalone: true,
  templateUrl: './game.html',
  styleUrls: ['./game.sass']
})
export class GamePage implements OnInit {
  gameId!: string;

  constructor(private route: ActivatedRoute) { }

  ngOnInit() {
    // recupera l'id del gioco dalla route
    this.gameId = this.route.snapshot.paramMap.get('id')!;
    console.log('Game ID:', this.gameId);

    // Qui puoi inizializzare il WebSocket per questa partita
    // oppure recuperare lo stato del gioco dal backend
  }
}
