# Flusso Completo delle Simulazioni — Quizora

Questo documento descrive in modo approfondito come funziona una sessione di simulazione (o esame) in Quizora, dal momento in cui l'utente preme "Inizia" fino alla visualizzazione dei risultati finali. Copre il comportamento del backend (modulo `ExamEngine`), l'integrazione con Redis per la gestione dei timer, e il comportamento del frontend (React + TanStack Query + Zustand).

---

## 1. Tipi di sessione

Prima di tutto è importante capire che in Quizora esistono tre tipi distinti di sessione, ognuno con un comportamento diverso:

**Official** è la modalità esame reale. Ha un timer, non mostra feedback immediato sulle risposte, e i risultati concorrono alla classifica globale del quiz. L'utente può avere al massimo una sessione Official attiva contemporaneamente per ogni quiz — se tenta di avviarne una seconda, il backend restituisce l'ID di quella già in corso.

**Simulation** è la modalità esercitazione con timer. Il comportamento è identico all'Official durante la sessione (nessun feedback, timer attivo), ma i risultati vengono tracciati separatamente e non influenzano la classifica ufficiale. Non c'è il controllo sulla sessione duplicata.

**Study** è la modalità apprendimento interattivo. Non ha timer, e dopo ogni risposta il backend rivela immediatamente se era corretta, mostra l'opzione giusta e la spiegazione associata alla domanda. È pensata per studiare il materiale senza pressione.

---

## 2. Struttura dati della sessione

Sul backend, il cuore del modulo è l'entità di dominio `ExamSession`. Questa entità incapsula tutto lo stato di una sessione: il riferimento al quiz, l'utente che la sta svolgendo, il tipo, lo stato corrente (`InProgress`, `Completed`, `Abandoned`, `TimedOut`), i timestamp di inizio e scadenza, e la collezione di risposte date (`SessionAnswer`).

Particolarmente importante è il `ConfigSnapshot`: al momento dell'avvio, la configurazione del quiz (punteggi per risposta corretta/errata/saltata, limite di tempo, punteggio di superamento) viene copiata immutabilmente nella sessione. Questo significa che anche se il creatore del quiz modifica la configurazione in seguito, le sessioni già avviate non ne risentono.

Il backend tiene separati il punteggio grezzo (`Score`) e il punteggio normalizzato (`NormalizedScore`), quest'ultimo espresso in percentuale da 0 a 100. Il calcolo avviene alla chiusura della sessione, sia essa volontaria, scaduta o abbandonata.

---

## 3. Avvio della simulazione

### Lato frontend

L'utente arriva sulla `QuizDetailPage`, che mostra i dettagli del quiz. Quando preme il pulsante per iniziare, appare un modale dove sceglie il tipo di sessione. A quel punto il frontend chiama la mutation `useStartExam()`, che effettua:

```
POST /api/v1/exams/start
Body: { quizId, sessionType }
Header: Idempotency-Key: <uuid generato lato client>
```

L'`Idempotency-Key` è un UUID generato dal frontend per ogni avvio. Serve a gestire i casi in cui la richiesta va persa e il client la ritenta: il backend riconosce la chiave e restituisce la risposta già prodotta invece di creare una sessione duplicata.

Alla risposta, il frontend:
1. Chiama `initSession()` sullo store Zustand, inizializzando l'indice della domanda corrente e azzerando le risposte locali.
2. Naviga verso `/exam/{sessionId}`.

### Lato backend

Il `StartExamCommandHandler` esegue i seguenti passi:

1. Valida il tipo di sessione ricevuto.
2. Se il tipo è **Official**, cerca una sessione attiva già esistente per quella combinazione di quiz+utente. Se la trova, restituisce l'ID esistente invece di crearne una nuova.
3. Recupera il quiz tramite `IQuizReader`, un'interfaccia cross-modulo che delega al modulo `QuizManagement`. Questo service usa Dapper per eseguire una query SQL che porta le domande, le opzioni e tutta la configurazione del quiz.
4. Costruisce il `ConfigSnapshot` copiando la configurazione corrente.
5. Crea la `ExamSession` tramite il factory method `ExamSession.Start()`, che imposta `StartedAt = UtcNow` e, per le sessioni con timer, calcola `ExpiresAt = UtcNow + TimeLimitSeconds`.
6. Salva la sessione nel database tramite EF Core (`ExamEngineDbContext`, schema `exam`).
7. Se la sessione **non** è di tipo Study, schedula il timer su Redis chiamando `IExamTimerService.ScheduleAsync()`.
8. Restituisce l'ID della sessione.

---

## 4. Il timer e Redis

Questo è uno dei meccanismi più interessanti dell'architettura. Il backend non usa job schedulati per ogni singola sessione, né mantiene connessioni WebSocket aperte. Usa invece un **sorted set Redis** come struttura dati temporale.

### Come funziona il sorted set

Quando una sessione con timer viene creata, `ExamTimerService` esegue:

```csharp
await db.SortedSetAddAsync("exam:timers", sessionId.ToString(), unixTimestampMilliseconds);
```

Il sorted set di Redis ordina automaticamente i suoi membri per score. In questo caso lo score è il timestamp Unix (in millisecondi) di scadenza della sessione. Questo significa che le sessioni che scadono prima stanno in cima alla lista.

Quando una sessione viene completata o abbandonata manualmente, il timer viene rimosso dal sorted set:

```csharp
await db.SortedSetRemoveAsync("exam:timers", sessionId.ToString());
```

### Il worker di polling

Un `BackgroundService` ASP.NET Core chiamato `ExamTimerWorker` gira in background e interroga Redis ogni 5 secondi:

```csharp
var expired = await db.SortedSetRangeByScoreAsync("exam:timers", 0, nowUnixMs, take: 100);
```

Questo comando recupera tutti i membri del sorted set il cui score è minore o uguale al timestamp corrente, cioè tutte le sessioni già scadute. Per ognuna di esse, il worker:

1. Invia un `AutoSubmitExamCommand` tramite MediatR.
2. L'handler recupera la sessione dal database.
3. Chiama `session.AutoSubmit()`, che porta la sessione allo stato `TimedOut` e calcola il punteggio finale.
4. Salva le modifiche.
5. Se tutto va a buon fine, rimuove l'entry dal sorted set.

Il worker gestisce gli errori in modo robusto: se l'elaborazione di una sessione fallisce, viene loggato l'errore ma il ciclo continua con le altre. L'entry rimane nel sorted set e verrà ritentata al prossimo ciclo.

### Perché questo approccio

Rispetto a un approccio con un job per sessione (es. Hangfire con un delay specifico), il sorted set Redis è estremamente scalabile: anche con migliaia di sessioni attive contemporaneamente, la query è `O(log N + M)` dove M è il numero di sessioni scadute nel batch. Non c'è overhead per session, non ci sono job individuali da gestire, e la struttura è resiliente ai riavvii del server (Redis persiste i dati).

Lo svantaggio è la granularità: le sessioni vengono elaborate con un ritardo massimo di 5 secondi rispetto alla scadenza effettiva. Per un'applicazione di quiz questo è accettabile.

### Il timer lato frontend

Il frontend ha il suo componente `ExamTimer` che mostra il conto alla rovescia. Questo timer funziona interamente in JavaScript, senza WebSocket. Al caricamento della pagina, il frontend riceve `remainingSeconds` dalla risposta di `GET /state`, calcola il timestamp di scadenza assoluto (`expireAt = Date.now() + remainingSeconds * 1000`) e poi fa scorrere il contatore localmente.

Per mantenere la sincronizzazione con il server, `ExamPage` effettua un refetch di `/state` ogni 30 secondi per le modalità Official e Simulation. Questo serve a correggere eventuali derive dell'orologio client e a rilevare cambiamenti di stato avvenuti server-side (come un `TimedOut` dovuto al worker).

Quando il timer del frontend raggiunge zero, chiama `handleTimerExpire()`, che naviga direttamente alla pagina dei risultati. Se invece il server ha già processato la scadenza prima che scadesse il timer client, la risposta al prossimo refetch di `/state` mostrerà `status: 'TimedOut'`, e `ExamPage` gestisce questo caso reindirizzando ai risultati.

---

## 5. Il flusso delle risposte

### Polling dello stato

Appena il frontend arriva su `ExamPage`, effettua:

```
GET /api/v1/exams/{sessionId}/state
```

Il backend esegue questa query tramite Dapper (non EF Core), con tre join: la sessione, le risposte già date, e le domande del quiz con le relative opzioni. Restituisce un `ExamStateDto` che contiene:

- Metadati della sessione (tipo, stato, `remainingSeconds`)
- La lista completa delle domande con le opzioni (solo testo e ID, non l'opzione corretta)
- Per ogni domanda, l'ID dell'opzione già selezionata (se presente)

Questa query non è cachata su Redis: viene eseguita ogni volta perché lo stato può cambiare tra una risposta e l'altra.

### Rispondere a una domanda

Quando l'utente seleziona un'opzione, il frontend registra il tempo trascorso sulla domanda corrente (tracciato con un `ref` aggiornato al cambio di domanda) e chiama:

```
POST /api/v1/exams/{sessionId}/answer
Body: { questionId, selectedOptionId, timeSpentSeconds }
```

Sul backend, `AnswerQuestionCommandHandler`:

1. Recupera la sessione dal DB (con le risposte già incluse via EF Core `Include`).
2. Controlla se la sessione è già scaduta: se `ExpiresAt < UtcNow`, chiama `session.AutoSubmit()` immediatamente e salva, quindi restituisce un errore al client. Questo è un controllo di sicurezza per il caso in cui il worker non abbia ancora elaborato la scadenza.
3. Recupera il quiz tramite `IQuizReader` per trovare la domanda e determinare l'opzione corretta.
4. Chiama `session.RecordAnswer()` sull'entità di dominio, che:
   - Verifica che la sessione sia `InProgress`
   - Verifica che non ci sia già una risposta per quella domanda (no duplicati)
   - Crea un `SessionAnswer` con `IsCorrect`, `PointsAwarded` calcolati in base al `ConfigSnapshot`
   - Aggiunge la risposta alla collezione interna
5. Salva le modifiche.
6. Costruisce `AnswerFeedbackDto`:
   - Per le sessioni **Official/Simulation**: restituisce solo `IsCorrect` e `PointsAwarded`
   - Per le sessioni **Study**: aggiunge `CorrectOptionId` e `CorrectExplanation`

Alla ricezione della risposta, il frontend:
- Aggiorna lo store Zustand con la risposta locale (per il `QuestionNavigator`)
- Per le sessioni Study, mostra il feedback in-page (verde/rosso + spiegazione)
- Invalida la query dello stato tramite `queryClient.invalidateQueries(examKeys.state(sessionId))`, forzando un refetch per aggiornare il contatore delle risposte date

### Navigazione tra domande

Il frontend mantiene localmente nel `examStore` l'indice della domanda corrente. La navigazione avviene senza chiamate al server: il `QuestionNavigator` mostra una griglia con lo stato visivo di ogni domanda (risposta/non risposta/corrente), e il pulsante Avanti/Indietro aggiorna solo l'indice locale.

---

## 6. Consegna dell'esame

### Consegna manuale

Quando l'utente preme "Consegna", il frontend chiama:

```
POST /api/v1/exams/{sessionId}/submit
Header: Idempotency-Key: <uuid>
```

Il `SubmitExamCommandHandler`:

1. Recupera la sessione.
2. Verifica che l'utente corrisponda al proprietario della sessione.
3. Se la sessione è già `Completed` (es. doppia submission), restituisce subito `200 OK` — l'operazione è idempotente.
4. Chiama `session.Complete()`, che:
   - Calcola il punteggio finale (somma dei `PointsAwarded`)
   - Calcola il `NormalizedScore` come `(score / (totalQuestions * pointsCorrect)) * 100`
   - Conta corrette/errate/saltate
   - Imposta `Status = Completed` e `CompletedAt = UtcNow`
   - Solleva l'evento di dominio `ExamCompletedEvent`
5. Estrae gli eventi di dominio dall'entità, li azzera, salva il DB.
6. Cancella il timer su Redis (`SortedSetRemoveAsync`).
7. Pubblica ogni evento di dominio tramite `IPublisher` di MediatR.

### Propagazione degli eventi di dominio

`ExamCompletedEvent` è ascoltato da due handler in moduli diversi:

**`ExamCompletedAnalyticsHandler`** (modulo Analytics): aggiorna le statistiche dell'utente (punteggio medio, accuratezza, tempo totale), le statistiche per categoria, l'attività giornaliera, e la posizione nella classifica del quiz su Redis.

**`ExamCompletedEventHandler`** (modulo AITutor): attiva automaticamente la generazione di un piano di studio personalizzato per l'utente basato sui risultati appena ottenuti, tramite il Claude API.

Questa separazione è possibile grazie al pattern degli eventi di dominio: `ExamSession` non conosce nulla di Analytics né di AITutor. Semplicemente dichiara cosa è successo, e i moduli interessati si iscrivono.

### Consegna per scadenza

Quando il timer scade e il worker Redis elabora la sessione (o quando il controllo in `AnswerQuestionCommandHandler` rileva la scadenza), viene chiamato `session.AutoSubmit()` invece di `session.Complete()`. La differenza è che lo stato diventa `TimedOut` invece di `Completed`, e viene sollevato `ExamTimedOutEvent` invece di `ExamCompletedEvent`. Questo permette di distinguere le due casistiche nelle statistiche e nella UI.

---

## 7. Abbandono

Se l'utente abbandona deliberatamente:

```
POST /api/v1/exams/{sessionId}/abandon
```

Il backend chiama `session.Abandon()`, che porta la sessione ad `Abandoned` senza calcolare punteggi e senza sollevare eventi. Il timer viene rimosso da Redis. Non vengono aggiornate statistiche né generati piani di studio.

---

## 8. Visualizzazione dei risultati

### La query dei risultati

```
GET /api/v1/exams/{sessionId}/results
```

Questa è la query più complessa del modulo. Il `GetExamResultsQueryHandler` usa Dapper con `QueryMultiple` per eseguire tre result set in una singola roundtrip al database:

1. **Dettagli sessione**: punteggio, normalizzato, soglia di superamento, contatori, data di completamento.
2. **Dettaglio risposte**: per ogni domanda, testo, spiegazione, opzione selezionata, correttezza, punteggio ottenuto, e l'opzione corretta (recuperata con una subquery).
3. **Classifica**: le prime 10 posizioni per quel quiz, calcolate con la funzione SQL `RANK() OVER (ORDER BY NormalizedScore DESC, CompletedAt ASC)`, filtrata solo sulle sessioni di tipo `Official` con stato `Completed` o `TimedOut`.

Il risultato determina anche se l'utente ha superato l'esame: se il quiz ha una soglia di superamento configurata (`PassingScore`), il backend confronta `NormalizedScore >= PassingScore`.

### La ResultsPage

Il frontend mostra:
- Il punteggio con colore verde (≥60%) o rosso
- La posizione in classifica e il numero totale di partecipanti
- Un grafico a torta con la distribuzione corrette/errate/saltate
- La lista completa delle domande con la risposta data, la risposta corretta e la spiegazione

---

## 9. Storico delle simulazioni

Per consultare le simulazioni passate su un determinato quiz:

```
GET /api/v1/exams/simulations/{quizId}
```

Questa query Dapper filtra per `QuizId = @QuizId`, `UserId = @UserId` e `Type = 'Simulation'`. Il frontend mostra una pagina (`SimulationsPage`) con un grafico a linee dell'andamento del punteggio nel tempo e la lista dei tentativi.

Lo storico generale di tutti gli esami (tutti i tipi) è disponibile su:

```
GET /api/v1/exams/history
```

---

## 10. Schema riassuntivo delle chiamate API

| Metodo | Endpoint | Chi chiama | Quando |
|--------|----------|------------|--------|
| `POST` | `/api/v1/exams/start` | Frontend | L'utente avvia una sessione |
| `GET` | `/api/v1/exams/{id}/state` | Frontend | All'ingresso in `ExamPage`, ogni 30s, dopo ogni risposta |
| `POST` | `/api/v1/exams/{id}/answer` | Frontend | L'utente seleziona un'opzione |
| `POST` | `/api/v1/exams/{id}/submit` | Frontend | L'utente preme "Consegna" |
| `POST` | `/api/v1/exams/{id}/abandon` | Frontend | L'utente abbandona |
| `GET` | `/api/v1/exams/{id}/results` | Frontend | Dopo il submit, su `ResultsPage` |
| `GET` | `/api/v1/exams/history` | Frontend | Dashboard / storico |
| `GET` | `/api/v1/exams/simulations/{quizId}` | Frontend | Pagina storico simulazioni di un quiz |

---

## 11. Ruolo di Redis nel sistema

Redis svolge due ruoli distinti in questo modulo:

**Gestione dei timer**: il sorted set `exam:timers` contiene gli ID delle sessioni attive con timer, ordinati per timestamp di scadenza. È la struttura che permette al `ExamTimerWorker` di trovare efficientemente solo le sessioni scadute senza interrogare il database.

**Classifica**: il modulo Analytics aggiorna la classifica del quiz su Redis ogni volta che una sessione viene completata (tramite `ExamCompletedEvent`). Questo permette alla query dei risultati di recuperare le posizioni in tempo reale senza ricalcolarle dal database ogni volta (sebbene la query attuale usi anche SQL RANK per la top 10, i dati possono essere integrati con la cache Redis per uso futuro in endpoint dedicati alla classifica).

---

## 12. Considerazioni architetturali

**Nessun WebSocket**: la scelta di non usare SignalR semplifica notevolmente il deployment e la scalabilità. Il frontend polling ogni 30 secondi è sufficiente per mantenere lo stato sincronizzato; la granularità di 5 secondi del worker Redis è accettabile per la scadenza dei timer.

**Separazione query/command**: tutte le operazioni di lettura (stato esame, risultati, storico) usano Dapper con SQL diretto, che è più performante di EF Core per query di proiezione complesse. Le operazioni di scrittura usano EF Core con il domain model completo, garantendo che la logica di business nell'entità venga sempre eseguita.

**Idempotenza**: le operazioni di avvio e consegna supportano `Idempotency-Key` per gestire i retry del client senza effetti collaterali. Questo è particolarmente importante per `SubmitExam`: se la risposta va persa in rete e il client riprova, la seconda richiesta trova la sessione già `Completed` e restituisce successo immediatamente.

**ConfigSnapshot**: la copia immutabile della configurazione al momento dell'avvio protegge le sessioni da modifiche retroattive. Un utente che completa una sessione avviata prima di una modifica del quiz vedrà sempre il punteggio calcolato con le regole vigenti al momento dell'avvio.
