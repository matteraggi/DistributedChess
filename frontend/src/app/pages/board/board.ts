import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { CommonModule } from '@angular/common';
import { Chess } from 'chess.js'; // Importiamo la logica
import { SignalRService } from '../../services/SignalRService .service';

@Component({
  selector: 'app-board',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './board.html',
  styleUrl: './board.sass'
})
export class Board implements OnInit {

  gameId: string = '';
  chess = new Chess(); // istanza di chess.js
  board: any[][] = []; // Matrice 8x8 per la grafica

  selectedSquare: string | null = null;

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

  constructor(private route: ActivatedRoute, private ws: SignalRService) { }

  async ngOnInit() {
    this.gameId = this.route.snapshot.paramMap.get('id')!;
    this.updateBoard();

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

    // per refresh della pagina
    await this.ws.startConnection();
    // potrei chiedere lo stato attuale della partita qui
  }

  updateBoard() {
    this.board = this.chess.board();
  }

  // Gestione click utente
  onSquareClick(rowIndex: number, colIndex: number) {
    const files = ['a', 'b', 'c', 'd', 'e', 'f', 'g', 'h']; //colonne
    const ranks = ['8', '7', '6', '5', '4', '3', '2', '1']; // righe

    const square = (files[colIndex] + ranks[rowIndex]) as any;
    const piece = this.board[rowIndex][colIndex];

    if (this.selectedSquare) {
      this.tryMove(this.selectedSquare, square);
      return;
    }

    if (piece) {
      // da aggiungere controllo: "è il mio turno? è il mio colore?"
      this.selectedSquare = square;
    }
  }

  async tryMove(from: string, to: string) {
    try {
      // Validazione Locale con chess.js
      // Se la mossa è illegale, questa riga lancia un'eccezione o ritorna null
      // ma andrebbe disattivato direttamente il click sulle caselle non valide
      const move = this.chess.move({ from, to, promotion: 'q' }); // Auto-promozione a regina per semplicità

      if (move) {
        this.updateBoard();
        this.selectedSquare = null;

        await this.ws.makeMove(this.gameId, from, to, 'q');
      }
    } catch (e) {
      // mossa non valida da backend
      this.selectedSquare = null;

      // Se clicco su un altro mio pezzo, selezionalo invece di fallire
      // (Logica semplificata per ora: resetta selezione)
    }
  }

  isSquareBlack(rowIndex: number, colIndex: number): boolean {
    return (rowIndex + colIndex) % 2 === 1;
  }
}