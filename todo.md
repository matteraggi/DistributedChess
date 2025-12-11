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

- Tolleranza ai Guasti del BACKEND (Infrastruttura)
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

- [ ] gestione utente che abbandona una partita
L'ideale sarebbe:
- dare i permessi dell'utente che è uscito ad un altro giocatore
- rendere la partita, anche in corso, disponibile ad un utente dalla lobby
- in questo modo se l'utente è crashato può rientrare, se invece ha abbandonato può essere sostituito
- quando rientra vengono spartiti nuovamente i permessi e riottiene esattamente quelli che aveva

Test Sicurezza (Defense in Depth):
- [ ] Verificare che il Backend blocchi comunque una mossa illegale anche se il team (hackerato) la vota all'unanimità.

- [x] design su figma e implementare

bugfix:
- [ ] rimuovere stato ready ogni volta che si esce dalla lobby di gioco
- [ ] Bugfix timer (quando scade senza mosse, quando fai reload della pagina)

extra: 
- [ ] chat di gioco