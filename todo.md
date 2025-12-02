- 4. Posso evidenziare le mosse possibili?
Sì, ed è facilissimo con chess.js.
La libreria ha una funzione this.chess.moves({ square: 'e2', verbose: true }) che ti restituisce la lista di tutte le caselle dove il pezzo in 'e2' può andare.
Potremo usare questa lista per colorare di verde le caselle valide nel tuo HTML.


Roadmap - struttura hybrid P2P

Fase 1: Aggiornamento Modelli Dati (Shared)
Obiettivo: Preparare le strutture dati per supportare squadre, ruoli specifici e il ciclo di vita del voto.

Ristrutturazione GameRoom:
- [ ] Aggiungere GameMode (enum: Classic1v1, TeamConsensus)
- [ ] Aggiungere mappa PiecePermissions (es. PlayerA -> ['P', 'K'] per Pedoni/Re)
- [ ] Aggiungere oggetto CurrentProposal (null se idle, popolato se c'è una votazione in corso)

Nuovi DTO (Messaggi):
- [ ] CreateGameMessage: Aggiungere opzioni per modalità e numero giocatori
- [ ] ProposeMoveMessage: Simile a MakeMove ma inteso come richiesta
- [ ] VoteMessage: { GameId, ProposalId, Vote (bool) }
- [ ] ProposalBroadcastMessage: Il server notifica ai peer di validare una proposta
- [ ] ProposalResultMessage: Esito del voto (Approvato/Respinto).

Fase 2: Backend Logic & Sharding
Obiettivo: Il Backend smette di essere l'unico decisore e diventa un gestore di stato e "notaio" per i voti (durante la partita).
Invece durante la lobby fa ovviamente ancora tutto lui.

Aggiornamento CreateGame:
- [ ] Implementare logica di assegnazione ruoli (Sharding)
Esempio: Se 2v2, Giocatore 1 prende Pedoni/Regina, Giocatore 2 prende gli altri pezzi
- [ ] Implementazione Ciclo di Consenso (GameHub.Voting.cs):
OPZ 1: più p2p ma meno consistenza:
Step 1 (propose): Client A invia proposta. Server fa broadcast a tutti.
Step 2 (vote): Client B invia voto. Server fa broadcast del voto a tutti ("B ha votato Sì").
Step 3 (count, Lato Client): Ogni Client (A, B, C, D) tiene il conto locale dei voti ricevuti:
    Quando il Client A vede che c'è la maggioranza -> Esegue la mossa localmente.
    Quando il Client B vede la maggioranza -> Esegue la mossa localmente.
    Save: Il server ascolta passivamente e quando vede la maggioranza salva su Redis solo per backup.
Problema: Se un pacchetto si perde, A muove il pezzo e B no. La partita si desincronizza.
Per risolvere questo servirebbe un protocollo complesso (Paxos/Raft).

OPZ 2: meno p2p ma consistenza forte
Step 1: Propose: Riceve proposta -> Verifica possesso pezzi (Sharding) -> Salva su Redis -> Broadcast agli altri
Step 2: Collect Votes: Riceve voti -> Aggiorna conteggio su Redis -> Verifica quorum (Maggioranza)
Step 3: Commit/Reject: Se passa -> Esegue mossa (aggiorna FEN) -> Notifica tutti. Se fallisce -> Pulisce stato

Fault Tolerance (Resilienza):
- [ ] Implementare un Timeout Votazione: Se il quorum non viene raggiunto entro X secondi (es. nodo disconnesso), la proposta scade o viene rigettata automaticamente

Fase 3: Frontend con logica
Obiettivo: Il Client diventa un nodo attivo che partecipa alla validazione, non solo un visualizzatore

Aggiornamento BoardComponent:
- [ ] Visualizzazione Ruolo: Mostrare all'utente quali pezzi può muovere (es. evidenziare i propri, opacizzare gli altri)
- [x] Blocco Input: Impedire il drag & drop dei pezzi non assegnati al proprio ruolo (Sharding lato client)

Interfaccia di Voto:
- [ ] Creare UI "Proposta in arrivo": Quando un compagno propone, mostrare una freccia sulla scacchiera (visualizzazione mossa)
- [ ] Bottoni Voto, visibili solo ai compagni di squadra

Gestione Stati:
- [ ] Gestire lo stato visivo "In attesa di approvazione..." per chi ha proposto

Fase 4: Testing Distribuito e Validazione (fault test)
Obiettivo: Verificare che il sistema si comporti come un sistema distribuito.

Test Consenso:
Scenario: 3 giocatori in team. 1 propone, 1 vota sì, 1 vota no. La mossa passa? (Logica maggioranza).

Test Fault Tolerance:
Scenario: 1 giocatore propone e chiude il browser. La partita si sblocca dopo il timeout?

Test Sicurezza (Defense in Depth):
Verificare che il Backend blocchi comunque una mossa illegale anche se il team (hackerato) la vota all'unanimità.

Fase 5: Documentazione Esame