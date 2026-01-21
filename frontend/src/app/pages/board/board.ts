import { Component, OnDestroy, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Chess } from 'chess.js';
import { SignalRService } from '../../services/SignalRService .service';
import { ActivatedRoute, Router } from '@angular/router';
import { MoveProposal, GameMode } from '../../models/dtos';
import { LeaveGameModal } from '../../components/leave-game-modal/leave-game-modal';
import { GameResultNotification } from '../../components/game-result-notification/game-result-notification';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-board',
  standalone: true,
  imports: [CommonModule, LeaveGameModal, GameResultNotification, FormsModule],
  templateUrl: './board.html',
  styleUrls: ['./board.scss']
})
export class Board implements OnInit, OnDestroy {

  gameId: string = '';
  chess = new Chess();
  board: any[][] = [];
  myColor: 'w' | 'b' | 'spectator' = 'spectator';
  selectedSquare: string | null = null;
  isFlipped = false;
  private hasLeft = false;
  possibleMoves: string[] = [];
  activeProposals: MoveProposal[] = [];
  myPermissions: string[] = [];
  myTeamProposals: MoveProposal[] = [];
  teamsMap: { [key: string]: string } = {};
  lastPiecePermissionMap: { [key: string]: string[] } = {};
  lastMoveAt: number = 0;
  readonly TURN_DURATION = 120;
  now: number = Date.now();
  private timerInterval: any;
  toastMessage: string | null = null;
  private toastTimeout: any;
  illegalProposals = new Set<string>();
  hoveredProposalId: string | null = null;

  // New state
  isLeaveModalOpen = signal(false);
  isLastPlayer = signal(false);
  gameMode = signal<GameMode>(GameMode.Classic1v1);

  // Debug State
  debugMode = signal(false);
  debugFrom = '';
  debugTo = '';

  // Game Result State
  showGameResult = false;
  gameResultMessage = '';
  gameResultReason = '';
  redirectCountdown = 0;
  isVictory = false;

  pieceImages: { [key: string]: string } = {
    'p': 'https://upload.wikimedia.org/wikipedia/commons/c/c7/Chess_pdt45.svg',
    'r': 'https://upload.wikimedia.org/wikipedia/commons/f/ff/Chess_rdt45.svg',
    'n': 'https://upload.wikimedia.org/wikipedia/commons/e/ef/Chess_ndt45.svg',
    'b': 'https://upload.wikimedia.org/wikipedia/commons/9/98/Chess_bdt45.svg',
    'q': 'https://upload.wikimedia.org/wikipedia/commons/4/47/Chess_qdt45.svg',
    'k': 'https://upload.wikimedia.org/wikipedia/commons/f/f0/Chess_kdt45.svg',

    'P': 'https://upload.wikimedia.org/wikipedia/commons/4/45/Chess_plt45.svg',
    'R': 'https://upload.wikimedia.org/wikipedia/commons/7/72/Chess_rlt45.svg',
    'N': 'https://upload.wikimedia.org/wikipedia/commons/7/70/Chess_nlt45.svg',
    'B': 'https://upload.wikimedia.org/wikipedia/commons/b/b1/Chess_blt45.svg',
    'Q': 'https://upload.wikimedia.org/wikipedia/commons/1/15/Chess_qlt45.svg',
    'K': 'https://upload.wikimedia.org/wikipedia/commons/4/42/Chess_klt45.svg',
  };

  pieceSymbols: { [key: string]: string } = {
    'P': '♟',
    'N': '♞',
    'B': '♝',
    'R': '♜',
    'Q': '♛',
    'K': '♚'
  };
  constructor(private route: ActivatedRoute, public ws: SignalRService, private router: Router) { }

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
      if (msg.piecePermission) {
        this.checkPieceTransfers(msg.piecePermission, msg.teams);
        this.lastPiecePermissionMap = msg.piecePermission;

        if (msg.piecePermission[myId]) {
          this.myPermissions = msg.piecePermission[myId];
        } else {
          this.myPermissions = [];
        }
      } else {
        this.myPermissions = [];
      }
      this.isFlipped = this.myColor === 'b';
      if (msg.lastMoveAt) {
        this.lastMoveAt = new Date(msg.lastMoveAt).getTime();
        console.log("Timer sincronizzato:", this.lastMoveAt);
      }


      // Update Game Mode
      this.gameMode.set(msg.mode);
      this.players.set(msg.players || []);

      this.updateBoard();
    });

    this.ws.activeProposals$.subscribe(msg => {
      if (msg.gameId !== this.gameId) return;
      console.log("Proposte aggiornate:", msg.proposals);
      this.activeProposals = msg.proposals;
      this.filterProposals();
    });

    this.ws.moveMade$.subscribe(msg => {
      if (msg.gameId !== this.gameId) return;

      this.chess.load(msg.fen);
      this.updateBoard();

      this.selectedSquare = null;
      this.lastMoveAt = Date.now();
      this.possibleMoves = [];
    });

    this.ws.gameOver$.subscribe(msg => {
      if (msg.gameId !== this.gameId) return;

      this.isVictory = msg.winnerPlayerId === this.ws.getOrCreatePlayerId();

      if (this.isVictory) {
        this.gameResultMessage = "HAI VINTO!";
      } else {
        this.gameResultMessage = "HAI PERSO...";
      }

      this.gameResultReason = msg.reason;
      this.showGameResult = true;
      this.redirectCountdown = 5; // 5 seconds countdown

      const countdownInterval = setInterval(() => {
        this.redirectCountdown--;
        if (this.redirectCountdown <= 0) {
          clearInterval(countdownInterval);
          this.router.navigate(['/lobby']);
        }
      }, 1000);
    });

    this.ws.playerJoinedGame$.subscribe(msg => {
      if (msg.gameId !== this.gameId) return;
      console.log("Giocatore entrato, ricalcolo assetti...");
      this.ws.requestGameState(this.gameId);
    });

    this.ws.gameState$.subscribe(msg => {
      // ... (existing subscriptions)
    });

    await this.ws.requestGameState(this.gameId);
  }

  // Helper to store players for "isLastPlayer" check
  players = signal<any[]>([]);

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
      return (files[7 - c] + ranks[7 - r]);
    } else {
      return (files[c] + ranks[r]);
    }
  }

  getPermissionIcons(): string {
    if (!this.myPermissions || this.myPermissions.length === 0) {
      return '∞';
    }

    return this.myPermissions
      .map(p => this.pieceSymbols[p.toUpperCase()] || p)
      .join(' ');
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
    if (!this.lastMoveAt) return this.TURN_DURATION;
    const deadline = this.lastMoveAt + (this.TURN_DURATION * 1000);
    const diff = deadline - this.now;
    return Math.max(0, Math.ceil(diff / 1000));
  }

  filterProposals() {
    const myId = this.ws.getOrCreatePlayerId();
    const myTeamColor = this.teamsMap[myId];

    if (!myTeamColor) {
      this.myTeamProposals = [];
      return;
    }

    let candidates = this.activeProposals.filter(p => {
      const proposerColor = this.teamsMap[p.proposerId];
      return proposerColor === myTeamColor;
    });

    this.myTeamProposals = candidates.filter(prop => {
      if (prop.proposerId === myId) return true;
      if (prop.votes.includes(myId)) return true;

      const ghostEngine = new Chess();
      try {
        ghostEngine.load(this.chess.fen());

        const moveResult = ghostEngine.move({
          from: prop.from,
          to: prop.to,
          promotion: prop.promotion || undefined
        });

        return !!moveResult;
      } catch (e) {
        return false;
      }
    });
  }

  onProposalHover(proposalId: string) {
    this.hoveredProposalId = proposalId;
  }

  onProposalLeave() {
    this.hoveredProposalId = null;
  }

  isProposalHighlight(rowIndex: number, colIndex: number, type: 'from' | 'to'): boolean {
    if (!this.hoveredProposalId) return false;

    const prop = this.myTeamProposals.find(p => p.proposalId === this.hoveredProposalId);
    if (!prop) return false;

    const square = this.getSquareNotation(rowIndex, colIndex);

    if (type === 'from') return square === prop.from;
    if (type === 'to') return square === prop.to;

    return false;
  }

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
        await this.ws.proposeMove(this.gameId, from, to, 'q');

        console.log("Proposta inviata!");
        this.selectedSquare = null;
        this.possibleMoves = [];
      } else {
        alert("❌ MOSSA BLOCCATA DAL FRONTEND: Mossa illegale!");
        console.warn("Mossa illegale bloccata dal frontend:", from, to);
      }
    } catch (e) {
      console.error(e);
    }
  }

  async voteFor(proposalId: string) {
    await this.ws.voteMove(this.gameId, proposalId, true);
  }

  hasVotedFor(prop: MoveProposal): boolean {
    const myId = this.ws.getOrCreatePlayerId();
    return prop.votes.includes(myId);
  }

  isPossibleMove(rowIndex: number, colIndex: number): boolean {
    const square = this.getSquareNotation(rowIndex, colIndex);
    return this.possibleMoves.includes(square);
  }

  canControlPiece(piece: any): boolean {
    if (!piece) return false;

    if (this.myColor === 'spectator' || piece.color !== this.myColor) return false;

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

    if (!teams) {
      console.error("ERRORE: Il server non ha mandato la mappa 'teams'!");
      this.myColor = 'spectator';
      console.groupEnd();
      return;
    }

    if (teams[myId]) {
      this.myColor = teams[myId] as 'w' | 'b';
      console.log("✅ TROVATO! Il mio colore è:", this.myColor);
    } else {
      this.myColor = 'spectator';
      console.warn("❌ NON TROVATO. Sono Spettatore. (Il mio ID non è nella lista)");

      const keys = Object.keys(teams);
      const match = keys.find(k => k.toLowerCase() === myId.toLowerCase());
      if (match) {
        console.error(`⚠️ ATTENZIONE: C'è un ID simile ma diverso! Server: '${match}' vs Locale: '${myId}'`);
      }
    }
    console.groupEnd();

    this.isFlipped = this.myColor === 'b';
  }

  private checkPieceTransfers(newPermissions: { [key: string]: string[] }, newTeams: { [key: string]: string }) {
    const myId = this.ws.getOrCreatePlayerId();

    // Se è la prima volta che riceviamo i permessi (o reload), non notificare
    if (Object.keys(this.lastPiecePermissionMap).length === 0) return;

    const myNewPermissions = newPermissions[myId] || [];
    const myOldPermissions = this.lastPiecePermissionMap[myId] || [];

    // Trova i pezzi nuovi
    const addedPieces = myNewPermissions.filter(p => !myOldPermissions.includes(p));

    if (addedPieces.length === 0) return;

    addedPieces.forEach(pieceChar => {
      // Cerca chi aveva questo pezzo prima
      let previousOwnerId: string | null = null;

      for (const [playerId, perms] of Object.entries(this.lastPiecePermissionMap)) {
        if (playerId !== myId && perms.includes(pieceChar)) {
          previousOwnerId = playerId;
          break;
        }
      }

      if (previousOwnerId) {
        // Verifica se era un compagno di squadra (stesso colore)
        const myTeam = newTeams[myId];
        const otherTeam = newTeams[previousOwnerId];

        if (myTeam && otherTeam && myTeam === otherTeam) {
          const pieceSymbol = this.pieceSymbols[pieceChar] || pieceChar;
          // Cerca il nome del giocatore
          const player = (this.players() || []).find(p => p.playerId === previousOwnerId);
          const playerName = player ? player.playerName : "Teammate";

          this.showToast(`📩 Received ${pieceSymbol} from ${playerName}`);
        }
      }
    });
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
    // Check if Last Player
    if (this.players().length === 1) {
      this.isLastPlayer.set(true);
      this.isLeaveModalOpen.set(true);
    } else {
      // Just explicit confirm or just leave?
      // User asked for modal only if last player or always?
      // "mettere un modulo per quando si abbandona la partita e si è l'ultima persona dentro"
      // Implicitly, if not last player, standard behavior (maybe just leave or confirm simple).
      // Let's show simple confirm if not last player, as per my assumption in template.
      this.isLastPlayer.set(false);
      this.isLeaveModalOpen.set(true);
    }
  }

  closeLeaveModal() {
    this.isLeaveModalOpen.set(false);
  }

  confirmLeave() {
    this.sendLeaveOnce();
    this.isLeaveModalOpen.set(false);
    this.router.navigate(['/lobby']);
  }

  async confirmDelete() {
    if (this.hasLeft) return;
    this.hasLeft = true;
    try {
      await this.ws.deleteGame(this.gameId);
    } catch (e) {
      console.error("Errore durante deleteGame", e);
    }
    this.isLeaveModalOpen.set(false);
    this.router.navigate(['/lobby']);
  }

  toggleDebug() {
    this.debugMode.update(v => !v);
  }

  async forceIllegalMove() {
    if (!this.debugFrom || !this.debugTo) {
      alert("Inserisci From e To");
      return;
    }
    console.warn(`[DEBUG] Forcing RAW move: ${this.debugFrom} -> ${this.debugTo}`);
    // Bypass frontend checks directly
    await this.ws.proposeMove(this.gameId, this.debugFrom, this.debugTo, 'q');
  }

  async testFrontendMove() {
    if (!this.debugFrom || !this.debugTo) {
      alert("Inserisci From e To");
      return;
    }
    console.log(`[DEBUG] Testing Frontend move: ${this.debugFrom} -> ${this.debugTo}`);
    // Use the standard tryMove which contains validation
    await this.tryMove(this.debugFrom, this.debugTo);
  }
}