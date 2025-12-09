Roadmap - struttura hybrid P2P

TODO:
Fase 1: Aggiornamento Modelli Dati (Shared)
Obiettivo: Preparare le strutture dati per supportare squadre, ruoli specifici e il ciclo di vita del voto.

Ristrutturazione GameRoom:
- [x] Aggiungere GameMode (enum: Classic1v1, TeamConsensus)
- [x] Aggiungere mappa PiecePermissions (es. PlayerA -> ['P', 'K'] per Pedoni/Re)
- [x] Aggiungere oggetto ActiveProposals (null se idle, popolato se c'è una votazione in corso)

Nuovi DTO (Messaggi):
- [x] CreateGameMessage: Aggiungere opzioni per modalità e numero giocatori
- [x] ProposeMoveMessage: Simile a MakeMove ma inteso come richiesta
- [x] VoteMessage: { GameId, ProposalId, Vote (bool) }
- [x] ActiveProposalsUpdateMessage: Il server notifica ai peer di validare una proposta
- [x] ProposalResultMessage: Esito del voto (Approvato/Respinto).

Fase 2: Backend Logic & Sharding
Obiettivo: Il Backend smette di essere l'unico decisore e diventa un gestore di stato e "notaio" per i voti (durante la partita).
Invece durante la lobby fa ovviamente ancora tutto lui.

Aggiornamento CreateGame:
- [x] Implementare logica di assegnazione ruoli (Sharding)
Esempio: Se 2v2, Giocatore 1 prende Pedoni/Regina, Giocatore 2 prende gli altri pezzi
- [x] Aggiungere implementazione per n giocatori
- [x] Implementazione Ciclo di Consenso (GameHub.Voting.cs):
Step 1: Propose: Riceve proposta -> Verifica possesso pezzi (Sharding) -> Salva su Redis -> Broadcast agli altri
Step 2: Collect Votes: Riceve voti -> Aggiorna conteggio su Redis -> Verifica quorum (Maggioranza)
Step 3: Commit/Reject: Se passa -> Esegue mossa (aggiorna FEN) -> Notifica tutti. Se fallisce -> Pulisce stato

Fault Tolerance (Resilienza):
- [x] Implementare un Timeout Votazione: Se il quorum non viene raggiunto entro X secondi (es. nodo disconnesso), la proposta scade o viene rigettata automaticamente

Fase 3: Frontend con logica
Obiettivo: Il Client diventa un nodo attivo che partecipa alla validazione, non solo un visualizzatore

Aggiornamento BoardComponent:
- [x] Visualizzazione Ruolo: Mostrare all'utente quali pezzi può muovere (es. evidenziare i propri, opacizzare gli altri)
- [x] Blocco Input: Impedire il drag & drop dei pezzi non assegnati al proprio ruolo (Sharding lato client)

Interfaccia di Voto:
- [x] Bottoni Voto, visibili solo ai compagni di squadra
- [x] Creare UI "Proposta in arrivo": Quando un compagno propone, mostrare una freccia sulla scacchiera (visualizzazione mossa)

Fase 4: FAULT TOLERANCE
- [ ] Obiettivo: Verificare che il sistema si comporti come un sistema distribuito.

Scenario A: Il "Crash" durante il Voto (AFK)
Situazione: In una partita 2vs2, il Giocatore A propone una mossa. Il Giocatore B (suo compagno) deve votare per raggiungere la maggioranza, ma chiude il browser.
Comportamento Atteso:
Il Timer (60s) continua a scendere.
Allo scadere, il Backend (ProposalTimeoutService) si accorge che non c'è maggioranza.
Il Backend forza una decisione (esegue la mossa più votata o random, oppure passa il turno).
La partita si sblocca per gli altri 3 giocatori.
Cosa Testare:
Apri 2 browser (stesso team).
Proponi con A.
Chiudi B.
Aspetta 60 secondi.
Verifica: La barra del tempo arriva a 0 e la mossa viene eseguita? Se sì, Test Superato.

Scenario B: Il Refresh della Pagina (State Recovery)
Situazione: Un giocatore preme F5 per sbaglio.
Comportamento Atteso:
SignalR si disconnette e riconnette (Nuovo ConnectionId).
Il Frontend chiama RequestGameState.
Il Backend lo re-iscrive ai gruppi (GameId e GameId_Color).
Il Backend invia lo stato completo, incluse le proposte attive.
Cosa Testare:
Fai una proposta. Non votarla.
Premi F5.
Verifica: Quando la pagina ricarica, vedi ancora la proposta nella sidebar? Vedi i pezzi colorati/bloccati giusti?
Nota: Se non vedi la proposta, controlla che RequestGameState nel backend popoli correttamente la lista ActiveProposals.

2. Tolleranza ai Guasti del BACKEND (Infrastruttura)
Qui dimostri la potenza di Docker e Redis.
Scenario C: Crash di un'istanza Backend
Situazione: Hai 2 istanze backend (Porta 5000 e 5001). Il Giocatore A è su 5000, B su 5001. Il container sulla 5000 muore.
Comportamento Atteso (Senza Load Balancer):
Il Giocatore A vede "Disconnected". SignalR prova a riconnettersi (default: 0s, 2s, 10s, 30s).
Se riavvii il container entro 30s, il Giocatore A si riconnette.
Grazie a Redis, lo stato della partita non è perso! Appena si riconnette, chiede lo stato e torna in gioco.
Comportamento Atteso (Con Nginx - Opzionale):
Se avessi Nginx davanti, il Giocatore A verrebbe spostato automaticamente sulla 5001 senza accorgersene.
Cosa dire al Prof: "La persistenza su Redis garantisce la Statelessness dei nodi di calcolo. Qualsiasi backend può servire qualsiasi utente in qualsiasi momento. Se un nodo cade, il client si riconnette a un altro nodo (o lo stesso riavviato) e recupera lo stato intatto."


3. Errori di Concorrenza (Race Conditions)
Cosa succede se due eventi accadono nello stesso millisecondo?
Scenario D: Doppia Proposta Simultanea
Situazione: Giocatore A e Giocatore B (stesso team) propongono due mosse diverse nello stesso istante.
Rischio: Il backend va in tilt o sovrascrive i dati.
Tua Protezione: Hai usato ActiveProposals come lista.
Il backend riceve A -> Aggiunge alla lista -> Update.
Il backend riceve B -> Aggiunge alla lista -> Update.
Il Frontend le mostra entrambe.
Test Superato.
Scenario E: Voto su Proposta Scaduta
Situazione: Il timer scade (Timeout Service cancella la proposta) MA un millisecondo prima un utente clicca "Vota". Il messaggio VoteMove arriva al backend dopo che la proposta è stata cancellata.
Tua Protezione: Nel metodo VoteMove hai:
code
C#
var targetProposal = room.ActiveProposals.FirstOrDefault(...);
if (targetProposal == null) return;
Il backend ignora silenziosamente il voto "fantasma". Test Superato.


4. Il "Single Point of Failure": REDIS
Devi essere onesto su questo.
Domanda del Prof: "Cosa succede se muore Redis?"
Risposta: "Il sistema si ferma (Availability loss). Ho scelto un'architettura CP (Consistency/Partition Tolerance). Senza Redis non posso garantire che i client vedano lo stesso stato, quindi preferisco fermare il gioco piuttosto che permettere mosse divergenti (Split-Brain). In produzione, userei Redis Sentinel o Redis Cluster per l'alta affidabilità."


Test F5 con Proposta:
Crea partita 2vs2.
P1 propone.
P2 (compagno) vede la proposta.
P2 preme F5.
Verifica: P2 deve rivedere la proposta e il tasto "Vota" attivo.
Test Reconnect:
Connetti P1 al backend Docker (5000).
Fai una mossa.
docker stop chessbackend.
P1 vede "Reconnecting...".
docker start chessbackend.
Verifica: P1 si riconnette e la scacchiera è aggiornata.
Test Mossa Illegale (Hacking):
Nel codice Frontend (board.ts), modifica temporaneamente tryMove per inviare una mossa folle (es. Torre che muove in diagonale).
Clicca per muovere.
Verifica:
Il tuo frontend magari la manda (se hai tolto i controlli).
Il frontend del compagno la riceve ma NON mostra il tasto Vota (perché il suo chess.js la blocca nel filtro P2P).
Il Backend non riceve voti sufficienti.
La mossa muore.

Test Sicurezza (Defense in Depth):
- [ ] Verificare che il Backend blocchi comunque una mossa illegale anche se il team (hackerato) la vota all'unanimità.

- [ ] design su figma e implementare

bugfix:
- [ ] rimuovere stato ready ogni volta che si esce dalla lobby di gioco
- [ ] Bugfix timer (quando scade senza mosse, quando fai reload della pagina)

extra: 
- [ ] chat di gioco