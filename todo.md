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

- [x] gestione utente che abbandona una partita
L'ideale sarebbe:
- dare i permessi dell'utente che è uscito ad un altro giocatore
- rendere la partita, anche in corso, disponibile ad un utente dalla lobby
- in questo modo se l'utente è crashato può rientrare, se invece ha abbandonato può essere sostituito
- quando rientra vengono spartiti nuovamente i permessi e riottiene esattamente quelli che aveva

- [x] design su figma e implementare

bugfix:
- [x] quando premo tasto back devo abbandonare effettivamente la partita (ora se esco e voglio rientrare risulto già dentro e non posso)
- [x] rimuovere stato ready ogni volta che si esce dalla lobby di gioco
- [x] capienza giocatori aggiornata in lobby
- [x] non mostrare stanze piene
- [x] salvare proposte in qualche modo per riprenderle on reload (al momento scompaiono)
- [x] quando un giocatore abbandona la partita/chiude il browser, toglierlo dai giocatori
- [x] Bugfix timer (quando scade senza mosse, quando fai reload della pagina)

- [x] entrata e uscita dei giocatori dalla lobby non sempre funzionante -> dovrebbe essere sistemato 🆗
- [x] notifica quando ricevi nuovi pezzi che erano di un tuo compagno
- [x] Aggiornare foto relazione
- [ ] Fare test finale
    - [ ] Una mossa illegale viene fermata da frontend? dovrebbe esser così
    - [ ] Verificare che il Backend blocchi comunque una mossa illegale anche se il team (hackerato) la vota all'unanimità
    - [x] Controllo tasto back per abbandonare la partita ed eventualmente eliminarla se ultimo giocatore