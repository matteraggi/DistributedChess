import { Component, OnDestroy, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Chess } from 'chess.js'; // Importiamo la logica
import { SignalRService } from '../../services/SignalRService .service';
import { ActivatedRoute, Router } from '@angular/router';
import { MoveProposal } from '../../models/dtos';


@Component({
  selector: 'app-board',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './board.html',
  styleUrls: ['./board.sass']
})
export class Board implements OnInit, OnDestroy {

  gameId: string = '';
  chess = new Chess(); // istanza di chess.js
  board: any[][] = []; // Matrice 8x8 per la grafica
  myColor: 'w' | 'b' | 'spectator' = 'spectator';
  selectedSquare: string | null = null;
  isFlipped = false;
  private hasLeft = false;
  possibleMoves: string[] = [];
  activeProposals: MoveProposal[] = [];
  myPermissions: string[] = [];
  myTeamProposals: MoveProposal[] = [];
  teamsMap: { [key: string]: string } = {};
  lastMoveAt: number = Date.now();
  readonly TURN_DURATION = 60;
  now: number = Date.now();
  private timerInterval: any;
  toastMessage: string | null = null;
  private toastTimeout: any;

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

  pieceNames: { [key: string]: string } = {
    'P': 'Pedoni',
    'N': 'Cavalli',
    'B': 'Alfieri',
    'R': 'Torri',
    'Q': 'Regina',
    'K': 'Re'
  };

  constructor(private route: ActivatedRoute, private ws: SignalRService, private router: Router) { }

  async ngOnInit() {
    this.gameId = this.route.snapshot.paramMap.get('id')!;
    await this.ws.startConnection();

    this.timerInterval = setInterval(() => {
      this.now = Date.now();
    }, 100);

    this.ws.moveMade$.subscribe(msg => {
      if (msg.gameId !== this.gameId) return;

      try {
        this.chess.load(msg.fen);
        this.updateBoard();
        this.selectedSquare = null;
        this.lastMoveAt = Date.now();
      } catch (e) {
        console.error("Errore sincronizzazione FEN", e);
      }
    });

    this.ws.gameState$.subscribe(msg => {
      if (msg.gameId !== this.gameId) return;

      this.chess.load(msg.fen);
      this.determineMyColor(msg.teams);
      this.activeProposals = msg.activeProposals || [];
      this.teamsMap = msg.teams;
      this.filterProposals();
      const myId = this.ws.getOrCreatePlayerId();
      if (msg.piecePermission && msg.piecePermission[myId]) {
        this.myPermissions = msg.piecePermission[myId];
      } else {
        this.myPermissions = [];
      }
      this.isFlipped = this.myColor === 'b';
      if (msg.lastMoveAt) {
        this.lastMoveAt = new Date(msg.lastMoveAt).getTime();
      }
      this.updateBoard();
    });

    this.ws.activeProposals$.subscribe(msg => {
      if (msg.gameId !== this.gameId) return;
      console.log("Proposte aggiornate:", msg.proposals);
      this.activeProposals = msg.proposals;
      this.filterProposals();
    });

    // 3. Gestione Mossa Eseguita
    this.ws.moveMade$.subscribe(msg => {
      if (msg.gameId !== this.gameId) return;

      this.chess.load(msg.fen);
      this.updateBoard();

      this.selectedSquare = null;
      this.lastMoveAt = Date.now();
      this.possibleMoves = [];
      // Nota: activeProposals verrà pulito dal messaggio activeProposals$ che arriverà subito dopo
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

  ngOnDestroy() {
    if (this.timerInterval) {
      clearInterval(this.timerInterval);
    }
  }

  showToast(message: string) {
    this.toastMessage = message;

    if (this.toastTimeout) clearTimeout(this.toastTimeout);

    this.toastTimeout = setTimeout(() => {
      this.toastMessage = null;
    }, 3000);
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

  getPermissionNames(): string {
    if (!this.myPermissions || this.myPermissions.length === 0) return 'Tutti';
    return this.myPermissions.map(p => this.pieceNames[p] || p).join(', ');
  }

  isMyPieceLocked(piece: any): boolean {
    if (!piece) return false;

    if (piece.color !== this.myColor) return false;

    if (this.myColor === 'spectator') return false;

    if (this.myPermissions.length > 0) {
      const typeUpper = piece.type.toUpperCase();
      return !this.myPermissions.includes(typeUpper);
    }

    return false;
  }

  getGlobalTimerPercentage(): number {
    const deadline = this.lastMoveAt + (this.TURN_DURATION * 1000);
    const diff = deadline - this.now;
    const percentage = (diff / (this.TURN_DURATION * 1000)) * 100;
    return Math.max(0, Math.min(100, percentage));
  }

  getGlobalTimerSeconds(): number {
    const deadline = this.lastMoveAt + (this.TURN_DURATION * 1000);
    const diff = deadline - this.now;
    return Math.max(0, Math.ceil(diff / 1000));
  }

  filterProposals() {
    const myId = this.ws.getOrCreatePlayerId();
    const myTeamColor = this.teamsMap[myId]; // 'w' o 'b'

    if (!myTeamColor) {
      this.myTeamProposals = [];
      return;
    }

    this.myTeamProposals = this.activeProposals.filter(p => {
      const proposerColor = this.teamsMap[p.proposerId];
      return proposerColor === myTeamColor;
    });
  }

  // Gestione click utente
  onSquareClick(rowIndex: number, colIndex: number) {
    const square = this.getSquareNotation(rowIndex, colIndex) as any;
    const piece = this.chess.get(square);

    if (piece && (!this.selectedSquare || piece.color === this.myColor)) {

      if (piece.color !== this.myColor) {
        return;
      }

      if (this.chess.turn() !== this.myColor) {
        console.log("Non è il tuo turno");
        return;
      }

      if (this.myPermissions.length > 0) {
        const pieceChar = piece.type.toUpperCase();
        if (!this.myPermissions.includes(pieceChar)) {
          this.showToast(`🚫 Non è un tuo pezzo!`);

          this.selectedSquare = null;
          this.possibleMoves = [];
          return;
        }
      }

      this.selectedSquare = square;
      const moves = this.chess.moves({ square: square, verbose: true });
      this.possibleMoves = moves.map((m: any) => m.to);
      return;
    }

    if (this.selectedSquare) {
      if (this.possibleMoves.includes(square)) {
        this.tryMove(this.selectedSquare, square);
      } else {
        this.selectedSquare = null;
        this.possibleMoves = [];
      }
    }
  }


  async tryMove(from: string, to: string) {
    try {
      const piece = this.chess.get(from as any);

      // Controllo Sharding lato client (UX)
      // Se myPermissions è popolato, devo controllare se il pezzo è nella lista
      if (this.myPermissions.length > 0 && piece) {
        const typeUpper = piece.type.toUpperCase();
        if (!this.myPermissions.includes(typeUpper)) {
          alert(`Non puoi muovere questo pezzo! I tuoi permessi: ${this.myPermissions.join(', ')}`);
          this.selectedSquare = null;
          this.possibleMoves = [];
          return;
        }
      }

      const moves = this.chess.moves({ verbose: true });
      const isLegal = moves.some((m: any) => m.from === from && m.to === to);

      if (isLegal) {
        // Invia PROPOSTA invece di mossa diretta
        await this.ws.proposeMove(this.gameId, from, to, 'q');

        // Feedback utente
        console.log("Proposta inviata!");
        this.selectedSquare = null;
        this.possibleMoves = [];
      }
    } catch (e) {
      console.error(e);
    }
  }

  // Azione Voto
  async voteFor(proposalId: string) {
    await this.ws.voteMove(this.gameId, proposalId, true);
  }

  // Helper per vedere se ho già votato
  hasVotedFor(prop: MoveProposal): boolean {
    const myId = this.ws.getOrCreatePlayerId();
    return prop.votes.includes(myId);
  }

  isPossibleMove(rowIndex: number, colIndex: number): boolean {
    const square = this.getSquareNotation(rowIndex, colIndex);
    return this.possibleMoves.includes(square);
  }

  // Ritorna TRUE se posso toccare questo pezzo
  canControlPiece(piece: any): boolean {
    if (!piece) return false;

    // 1. Controllo Colore
    if (this.myColor === 'spectator' || piece.color !== this.myColor) return false;

    // 2. Controllo Sharding (Permessi)
    if (this.myPermissions.length > 0) {
      const typeUpper = piece.type.toUpperCase();
      if (!this.myPermissions.includes(typeUpper)) return false;
    }

    return true;
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