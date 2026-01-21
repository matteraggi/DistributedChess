# Distributed Team Chess 
### ♟️Relazione Finale per il Corso di Sistemi Distribuiti 2025/26 - Autore: Matteo Raggi

**Distributed Team Chess** è una rivisitazione cooperativa del classico gioco degli scacchi, progettata per dimostrare i principi fondamentali dei Sistemi Distribuiti all'interno di un ambiente interattivo in tempo reale.

A differenza degli scacchi tradizionali, questo sistema introduce meccaniche di **Role Sharding** (ripartizione dei ruoli) e **Distributed Consensus** (consenso distribuito). 

I giocatori sono organizzati in squadre e ogni nodo (giocatore) detiene permessi di scrittura esclusivi su un sottoinsieme di risorse (pezzi degli scacchi specifici). 

Per eseguire una mossa, la squadra deve raggiungere un consenso tramite un protocollo di voto, trasformando la partita in una sfida di coordinamento distribuito.

## 🏗️ Architettura del Sistema

Il sistema adotta un modello architetturale **Broker-Based Hybrid Peer-to-Peer**.

L'architettura è progettata per garantire **Strong Consistency** (coerenza forte) e **Partition Tolerance** (tolleranza alle partizioni), classificandosi come sistema CP nel teorema CAP.

**Componenti Principali**

- **Client (Frontend)**: Single Page Application (SPA) in Angular che funge da nodo "peer" per la visualizzazione dello stato e la validazione locale (P2P filtering).
- **Application Servers (Backend)**: Container stateless .NET 8 che orchestrano la comunicazione via SignalR. Agiscono come bus di coordinamento e non mantengono stato locale.
- **State Store / Message Broker (Redis)**: Un'istanza Redis che funge sia da layer di persistenza (Single Source of Truth) sia da Backplane per la comunicazione Pub/Sub tra i nodi backend.

## ✨ Funzionalità Distribuite Chiave

Il progetto implementa diverse feature critiche per i sistemi distribuiti:

- **Role Sharding (Resource Sharing)**: Il controllo della scacchiera è partizionato.
Ad esempio, il Giocatore A controlla solo i Pedoni, mentre il Giocatore B controlla le Torri.
- **Consenso Distribuito**: Una mossa proposta è valida solo se approvata dalla maggioranza del team attraverso un meccanismo di voto.
- **Fault Tolerance & Automatic Failover**: Il sistema rileva automaticamente i guasti dei nodi (disconnessioni). Se un giocatore si disconnette, i suoi "shard" (permessi sui pezzi) vengono automaticamente ridistribuiti ai compagni di squadra sopravvissuti per garantire la liveness del sistema.
- **Deadlock Resolution**: Un servizio di timeout impedisce lo stallo del gioco. Se un turno scade senza consenso, il sistema forza una risoluzione automatica.

## 🛠️ Tech Stack

**Frontend**: Angular
**Backend**: .NET (C#)
**Database**: Redis
**DevOps**: Docker
**Protocolli**: WebSocket

## 🚀 Guida all'Installazione e Avvio

Il sistema è progettato per essere agnostico rispetto alla piattaforma e completamente riproducibile tramite containerizzazione.

**Prerequisiti**
- Git
- Docker Desktop

**Istruzioni (Local Deployment)**

1. **Clona il repository**: 
Bashgit clone https://github.com/tuo-username/distributed-team-chess.git
cd distributed-team-chess

2. **Build e Avvio dei Container**: Esegui il comando di orchestrazione. Il flag --build assicura la compilazione del codice C# e TypeScript nelle immagini.
Bashdocker compose up -d --build

3. **Accedi all'Applicazione**: Una volta avviati i container, i servizi saranno disponibili ai seguenti indirizzi:
- **🎮 Frontend (Client)**: http://localhost:4200 26
- **🔌 Backend API**: http://localhost:5000
- **🗄️ Redis**: Porta 6379 (background)

## 📖 Come Giocare

1. **Creazione della Partita (Lobby)**
- Apri il browser su http://localhost:4200.
- Clicca su "Create Game".
- Seleziona la dimensione del team (es. "2" per una partita 2vs2 in modalità Team Consensus).
- Attendi nella Waiting Room che altri giocatori si uniscano e che tutti siano "Ready"30.

2. **Gameplay (Sharded Board)**
- Visualizzazione: I pezzi che non puoi controllare appariranno semi-trasparenti (opachi).
- Proposta: Se muovi un pezzo che possiedi, la mossa non viene eseguita subito ma diventa una "Move Proposal".

3. **Votazione**
- I compagni di squadra vedranno la proposta nella barra laterale.
- Devono cliccare "Vote" per approvare.
- Al raggiungimento del quorum, la mossa viene "Committata" e aggiornata su tutti gli schermi.

## 🧪 Testing e Validazione

Il progetto è stato validato attraverso scenari End-to-End (E2E) per testare la natura distribuita del sistema:
- Scenario Consenso: Verifica che le mosse illegali vengano bloccate localmente e che quelle legali richiedano il voto.
- Scenario Failover: Simulazione di crash del nodo (chiusura browser) per verificare il trasferimento automatico dei permessi ai compagni.
- Scenario Liveness: Verifica che il ProposalTimeout Service risolva i deadlock se un team non vota entro il tempo limite (120s).

## 📄 Licenza e Riferimenti
Questo progetto è stato sviluppato come parte del corso di Sistemi Distribuiti presso l'Università di Bologna.
Per i dettagli completi sull'implementazione, fare riferimento alla relazione tecnica allegata Final Report for the Distributed Systems Course 2025/2637.
