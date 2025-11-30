import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Chess } from 'chess.js'; // Importiamo la logica
import { SignalRService } from '../../services/SignalRService .service';
import { ActivatedRoute, Router } from '@angular/router';


@Component({
  selector: 'app-board',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './board.html',
  styleUrls: ['./board.sass']
})
export class Board implements OnInit {

  gameId: string = '';
  chess = new Chess(); // istanza di chess.js
  board: any[][] = []; // Matrice 8x8 per la grafica
  myColor: 'w' | 'b' | 'spectator' = 'spectator';
  selectedSquare: string | null = null;
  isFlipped = false;
  private hasLeft = false;

  pieceImages: { [key: string]: string } = {
    'p': 'https://upload.wikimedia.org/wikipedia/commons/c/c7/Chess_pdt45.svg', // Nero
    'r': 'https://upload.wikimedia.org/wikipedia/commons/f/ff/Chess_rdt45.svg',
    'n': 'https://upload.wikimedia.org/wikipedia/commons/e/ef/Chess_ndt45.svg',
    'b': 'https://upload.wikimedia.org/wikipedia/commons/9/98/Chess_bdt45.svg',
    'q': 'https://upload.wikimedia.org/wikipedia/commons/4/47/Chess_qdt45.svg',
    'k': 'https://upload.wikimedia.org/wikipedia/commons/f/f0/Chess_kdt45.svg',

    'P': 'https://upload.wikimedia.org/wikipedia/commons/4/45/Chess_plt45.svg', // Bianco
    'R': 'https://upload.wikimedia.org/wikipedia/commons/7/72/Chess_rlt45.svg',
    'N': 'https://upload.wikimedia.org/wikipedia/commons/7/70/Chess_nlt45.svg',
    'B': 'https://upload.wikimedia.org/wikipedia/commons/b/b1/Chess_blt45.svg',
    'Q': 'https://upload.wikimedia.org/wikipedia/commons/1/15/Chess_qlt45.svg',
    'K': 'https://upload.wikimedia.org/wikipedia/commons/4/42/Chess_klt45.svg',
  };

  constructor(private route: ActivatedRoute, private ws: SignalRService, private router: Router) { }

  async ngOnInit() {
    this.gameId = this.route.snapshot.paramMap.get('id')!;
    await this.ws.startConnection();

    this.ws.moveMade$.subscribe(msg => {
      if (msg.gameId !== this.gameId) return;

      // Aggiorna la logica interna con la mossa arrivata e la FEN
      try {
        this.chess.load(msg.fen);
        this.updateBoard();
        this.selectedSquare = null;
      } catch (e) {
        console.error("Errore sincronizzazione FEN", e);
      }
    });

    this.ws.gameState$.subscribe(msg => {
      if (msg.gameId !== this.gameId) return;

      this.chess.load(msg.fen);
      this.updateBoard();

      // Determina colore dalla mappa
      this.determineMyColor(msg.teams);
      this.isFlipped = this.myColor === 'b';
      this.updateBoard();
    });

    this.ws.gameOver$.subscribe(msg => {
      if (msg.gameId !== this.gameId) return;

      let message = "";
      if (msg.winnerPlayerId === this.ws.getOrCreatePlayerId()) {
        message = "HAI VINTO! 🏆";
      } else {
        message = "HAI PERSO... 💀";
      }

      alert(`${message} (Motivo: ${msg.reason})`);

      this.router.navigate(['/lobby']);
    });

    await this.ws.requestGameState(this.gameId);
  }

  updateBoard() {
    let rawBoard = this.chess.board();

    if (this.isFlipped) {
      this.board = rawBoard.slice().reverse().map(row => row.slice().reverse());
    } else {
      this.board = rawBoard;
    }
  }

  getSquareNotation(r: number, c: number): string {
    const files = ['a', 'b', 'c', 'd', 'e', 'f', 'g', 'h'];
    const ranks = ['8', '7', '6', '5', '4', '3', '2', '1'];

    if (this.isFlipped) {
      // Se girata: riga 0 visiva è Rank 1 ('1'), colonna 0 visiva è File h ('h')
      // Quindi dobbiamo invertire gli indici
      return (files[7 - c] + ranks[7 - r]);
    } else {
      return (files[c] + ranks[r]);
    }
  }


  // Gestione click utente
  onSquareClick(rowIndex: number, colIndex: number) {
    // 1. Ottieni la coordinata reale considerando la rotazione
    const square = this.getSquareNotation(rowIndex, colIndex) as any;

    // 2. Ottieni il pezzo logico dalla libreria chess
    const piece = this.chess.get(square);

    if (piece) {
      // Blocco 1: È il mio pezzo?
      if (this.myColor !== 'spectator' && piece.color !== this.myColor) {
        if (!this.selectedSquare) return;
      }

      if (!this.selectedSquare && this.chess.turn() !== this.myColor) {
        console.warn("Non è il tuo turno!");
        return;
      }

      if (piece.color === this.myColor) {
        this.selectedSquare = square;
        return;
      }
    }

    if (this.selectedSquare) {
      this.tryMove(this.selectedSquare, square);
    }
  }


  async tryMove(from: string, to: string) {
    // 1. Tentativo Locale (Optimistic UI)
    try {
      const move = this.chess.move({ from, to, promotion: 'q' });

      if (move) {
        this.updateBoard();
        this.selectedSquare = null;

        // 2. Invio al Server
        await this.ws.makeMove(this.gameId, from, to, 'q');
      }
    } catch (e) {
      console.error("Errore mossa:", e);

      // rollback (controllare se funziona)
      if (this.chess.history().length > 0) {
        const lastMove = this.chess.history({ verbose: true }).pop();
        if (lastMove && lastMove.from === from && lastMove.to === to) {
          this.chess.undo();
          this.updateBoard();
          alert("Mossa rifiutata dal server (Desync). La scacchiera è stata ripristinata.");
        }
      }
      this.selectedSquare = null;
    }
  }

  private determineMyColor(teams: { [key: string]: string }) {
    const myId = this.ws.getOrCreatePlayerId();

    console.group("🔍 DEBUG COLORE");
    console.log("Mio ID Locale:", myId);
    console.log("Lista Squadre dal Server:", teams);

    // Controllo difensivo se teams è null/undefined
    if (!teams) {
      console.error("ERRORE: Il server non ha mandato la mappa 'teams'!");
      this.myColor = 'spectator';
      console.groupEnd();
      return;
    }

    // Cerchiamo l'ID nella mappa
    if (teams[myId]) {
      this.myColor = teams[myId] as 'w' | 'b';
      console.log("✅ TROVATO! Il mio colore è:", this.myColor);
    } else {
      this.myColor = 'spectator';
      console.warn("❌ NON TROVATO. Sono Spettatore. (Il mio ID non è nella lista)");

      // Debug avanzato: controlliamo se c'è un match parziale (es. case sensitivity)
      const keys = Object.keys(teams);
      const match = keys.find(k => k.toLowerCase() === myId.toLowerCase());
      if (match) {
        console.error(`⚠️ ATTENZIONE: C'è un ID simile ma diverso! Server: '${match}' vs Locale: '${myId}'`);
      }
    }
    console.groupEnd();

    // Aggiorna indicatori
    this.isFlipped = this.myColor === 'b';
  }

  isSquareBlack(rowIndex: number, colIndex: number): boolean {
    return (rowIndex + colIndex) % 2 === 1;
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
}