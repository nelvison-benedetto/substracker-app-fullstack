## SUBSNAP BACKEND – ARCHITECTURE & ENGINEERING OVERVIEW

- Author: **Nelvison Benedetto**
- Project: SubSnap Backend
- Platform: ASP.NET Core (.NET 8)
- Database: PostgreSQL
- ORM: Entity Framework Core
- Architecture: Clean Architecture + Domain-Driven Design + CQRS
- Style: Domain-First + Vertical Slice Architecture
- Messaging Pattern: MediatR (internal only)
- Maturity Level: Production-grade backend foundation

SubSnap è progettato intenzionalmente seguendo pratiche architetturali adottate nei backend moderni ad alta scalabilità, ispirate a piattaforme utilizzate da aziende come Uber, Stripe, Shopify e da sistemi enterprise basati su ASP.NET Core.

L'obiettivo del progetto non è dimostrare semplicemente l'implementazione di funzionalità applicative, ma mostrare un approccio strutturale alla progettazione di backend scalabili, mantenibili ed evolvibili nel tempo.

SubSnap rappresenta quindi un blueprint architetturale orientato alla produzione, progettato per evidenziare pattern e principi utilizzati nei sistemi backend di larga scala.

```
         API
        /   \
Application  Infrastructure
       \    /
        Core
```

---
### .CORE layer
Questo layer contiene **pure business logic** e il **core domain model**.
È completamente indipendente da framework, infrastruttura o servizi esterni e deve poter essere eseguito in isolamento.

**Contains**:
DOMAIN [Entities, Aggregates, Value Objects, Events, Domain Rules, ...].

**References**: NONE

---

### .APPLICATION layer
Questo layer definisce gli **application workflows** e rappresenta il confine tra il dominio e il mondo esterno.
Non contiene logica infrastrutturale, ma definisce contratti (Ports) che dovranno essere implementati dal layer Infrastructure.

**Contains**:
Behaviors(MediatR pipeline), DependencyInjection (only for .application level), Ports (interfaces), UseCases(slices e.g.Login, Logout,...)(each slice contains Orchestrator(the TRUE entry point), Handler, Command, Result, Policies, Loaders).

**References**: .Core
usa il domain in .core e definisce le ports(interfaccie) che .infrastructure implementerà

---

### .INFRASTRUCTURE layer
Questo layer contiene tutte le integrazioni con sistemi esterni, come database, servizi di autenticazione, sistemi di storage e altri componenti infrastrutturali.
Implementa le interfacce (Ports) definite nell’Application layer ed è responsabile della persistenza dei dati, dei meccanismi di sicurezza e delle integrazioni esterne.

**Contains**:
EF Core repositories, JWT generation, Password hashing, Entities Configuration, ApplicationDbContext, UnitofWork, DataLoaders(Aggregates & Batch Loaders), DependencyInjection (only for .infrastructure level), Storage(for Hetzner Object media files), OutBox Processor.

**References**: .Application, .Core 
implementa i Ports definiti in .applicatione e usa le entities del .core

---

### .API layer
Questo layer espone il sistema tramite endpoint HTTP ed è responsabile della gestione delle richieste, della validazione, della serializzazione e della configurazione dell’API.
I controller non contengono logica di business e **non dipendono direttamente da MediatR**.
Invocano invece l’Orchestrator del relativo Use Case, mantenendo il layer HTTP sottile e disaccoppiato dalla pipeline applicativa.

**Contains**:
Requests & Responses (will match w Command & Result of the target UseCase), Mapping(x auto match request->command & result->response), ApiResult & ApiError (wrapper, for uniformity when return the response to the client), Controllers(don't know MediatR, they call the orchestrator of target usecase), Filters, Middleware(for global exception and correlationid for logging), Startup Extensions (authentication, authorization, correlationid, cors, healthchecks, swagger, validation), Validators(usa plugin Fluent Validator, for rules e.g. email must not be empty), Versioning, Program.cs.

**References**: .Application, .Infrastructure
chiama gli usescases(only orchestrators) del .application e usa dependencyinjection registrata in .infrastructure

---

### Techniques & Patterns

- **Domain-First**: the domain model (in .Core) represents the source of truth, cosi il databse è scollegato(facilmente sostituibile!) ed non è trattato come system fundation (approach Db-First).
- **Clean Architecture**: project suddiviso in layers .API .Application .Infrastructure .Core, e le loro references. 
- **Domain Driven Design (DDD)**: la logica di business è incapsulata direttamente nel modello di dominio, evitando così logica dispersa nei servizi applicativi.
- **Boundaries tra Assemblies(projects)**: .applications/ports qui vengono definiti i contratti, la vero implementazione è dentro .infrastructure e.g.IUserRepository -> UserRepository, la risoluzione delle dipendenze avviene in servicecollectionextensions.cs così quando chiami l'interfaccia automaticamente viene chiamata l'implementazione, obbligatorio perche un prj non deve conosce le implementazioni dell'altro prj, ma solo le interfaccie: i boundaries sono rispettati (e i tests sono clean).
- **Mapping request->command & result->response nel controller**: grazie a plugin AutoMapper converto i dati ricevuti dal client in type xxRequest e lo converto in type xxCommand, nel file mapping e.g.requesttocommandprofile.cs non devi esplicitare come avviene la conversione SE i types & names matchano perfettamente e SE non vengono usati Value Objects (e.g.UserId). 
- **Api Result & Api Error**: la risposta che riceverà il client sara SEMPRE con la STESSA struttura, per qualsiasi tipo di risposta (200/201/404/...).
- **MediatR & Pipeline Bahviors**: plugin mediatr lavora come dispatcher, dal controller chiami l'orchestrator dello target slice usecase, e l'orchestrator fa partire la pipeline di behaviors controller -> orchestrator-> validationbehavior.cs -> loggingbehavior.cs -> performancebehavior.cs -> transactionbehavior.cs -> exceptionbehavior.cs -> handler target, quindi sono a 'cipolla'(dal controller penetri fino in fondo e poi torni in superfice)!
- **Unit Of Work Pattern**: solo transactionbehavior.cs chiama l'uow, che centralizza il SaveChangesAsync() al ritorno verso superfice dei dati elaborati.
- **Vertical Slice Architecture**: ogni 'usecase' è uno slice isolato, ogni slice si comporta come un mini microservice e.g. 
UseCases/Auth/Login/
    LoginCommand
    LoginOrchestrator
    LoginHandler 
    LoginResult
    Policies/  //regole
    Loaders/
- **Validations**: validationbehvior.cs sia in entrata che in uscita della pipeline valida i dati (usa plugin FluentValidator), controlla solo i file xxxValidator.cs che hanno xxxCommand target utilizzato (e.g. RegisterUserValidator : AbstractValidator<RUCommand>)
- **Aggregate Root Pattern**: solo gli aggregate root (e.g. User.cs) hanno il proprio repository, per modificare i figli (e.g.Subscriptions.cs) devi passare dall'aggregate root, cosi la consistency è sempre garantita. 
- **Value Objects**: sostituisce i tipi primitivi where business meaning exists, e.g. UserId è uno struct.
- **Loaders**: per le READ queries quando sono necessari risultati multi-tabellari, uso i loaders per caricare e.g. user + subscriptions e restituisco in formato e.g. UserSubscriptionsAggregate.cs(composizione a runtime). quindi .Include() assolutamente DA NON FARE perche crea mega-joins.
- **CQRS (Command Query Separation)**: WRITE e READ queries hanno business separato, per le Read queries uso Cuncurrent Batched queries, cosi invece di N richieste -> N query, faccio N richieste -> le accumulo -> faccio 1 sola batch query! utilizzato anche da Uber.
- **Domain Events**: rappresenta un'evento che accade quando succede qualcosa e.g UserRegisteredEvent.cs usato da UserRegisteredHandler.cs che puo fare per esempio logica di invio di email di benvenuto. 
- **Outbox Pattern (reliable events)**: per assicurarsi di e.g registrare utente + inviare email in una 1 sola transazione(COMMIT) (altrimenti magari la registrazione user ha successo ed è salvato su db, ma poi l'invio email fallisce). quindi Save Aggregate -> Save OutBox message -> COMMIT(salvi su db), dopodiche il background worker outboxprocessor.cs sempre attivo in modalità polling ogni 2sec legge ultime 20 righe della tabella db  outboxmessage (solo quelli non ancora done) e pubblica quegli eventi (e.g. invio email di benvenuto)!
- **Observability & Logging**: sempre chiaro logging di cosa è appena successo, ogni log fornito di chiara descrizione e di correlationId per poter facilmente tracciare cosa succede in real-time. see loggingbehavior.cs correlationidmiddleware.cs
- **Security**: il sistema di auth si basa su JWT(Json Web Tokens) dunque utilizzo access tokens(breve durata), refresh tokens(lunga durata) e jwt claims.
- **Media Upload**: su tab db usermedia sul db salvo solo i metadati, i file vengono archiviati su Hetzner Object storage.

## Perché questa architettura è scalabile

L'architettura del progetto è costruita per supportare evoluzione, crescita del dominio e carichi applicativi crescenti senza compromettere la manutenibilità del sistema.

Le principali caratteristiche architetturali includono:

### Explicit Transactional Boundaries
I confini transazionali sono definiti esplicitamente all'interno dell'Application Layer, garantendo consistenza dei dati e isolamento delle operazioni critiche.

### Event-Driven Extensibility
La struttura applicativa è progettata per supportare facilmente un'evoluzione verso modelli event-driven, consentendo integrazioni asincrone e distribuite tra componenti del sistema.

### Stateless API Design
L'API è completamente stateless, facilitando la scalabilità orizzontale tramite bilanciamento del carico e deploy su infrastrutture distribuite.

### Independent Read/Write Models (CQRS mindset)
Le operazioni di lettura e scrittura sono separate concettualmente, permettendo ottimizzazioni indipendenti delle query e maggiore flessibilità nell'evoluzione dei modelli dati.

### Background Processing Ready
L'architettura è predisposta per l'introduzione di processi asincroni e job in background (es. worker services, code di messaggi, task distribuiti).

### Infrastructure Replaceability
Le dipendenze infrastrutturali sono completamente astratte, consentendo la sostituzione di componenti tecnici (database, sistemi di storage, provider di autenticazione) senza impattare la logica di dominio.


## SUBSNAP FRONTEND – ARCHITECTURE & ENGINEERING OVERVIEW

ReactJs, Typescript, TailwindCSS, React-Query, React-Redux, React-Router, DaisyUI.

## SUBSNAP DATABASE – ARCHITECTURE & ENGINEERING OVERVIEW

```
-- Abilita estensione per UUID
CREATE EXTENSION IF NOT EXISTS "pgcrypto";

-- Tabella users
CREATE TABLE users (
    id UUID NOT NULL DEFAULT gen_random_uuid(),
    email VARCHAR(255) NOT NULL,
    passwordhash VARCHAR(512) NOT NULL,
    isactive BOOLEAN NOT NULL DEFAULT TRUE,
    createdat TIMESTAMPTZ(3) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updatedat TIMESTAMPTZ(3) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    lastlogin TIMESTAMPTZ(3) NULL,

    CONSTRAINT pk_users PRIMARY KEY (id),
    CONSTRAINT uq_users_email UNIQUE (email)
);

-- Tabella refreshtokens
CREATE TABLE refreshtokens (
    id UUID NOT NULL DEFAULT gen_random_uuid(),
    userid UUID NOT NULL,
    token VARCHAR(512) NOT NULL,
    expiresat TIMESTAMPTZ(3) NOT NULL,
    isrevoked BOOLEAN NOT NULL DEFAULT FALSE,
    createdat TIMESTAMPTZ(3) NOT NULL DEFAULT CURRENT_TIMESTAMP,

    CONSTRAINT pk_refreshtokens PRIMARY KEY (id),
    CONSTRAINT fk_refreshtokens_users FOREIGN KEY (userid) REFERENCES users(id),
    CONSTRAINT uq_refreshtokens_token UNIQUE (token)
);

CREATE INDEX ix_refreshtokens_userid ON refreshtokens(userid);

-- Tabella subscriptions
CREATE TABLE subscriptions (
    id UUID NOT NULL DEFAULT gen_random_uuid(),
    userid UUID NOT NULL,
    name VARCHAR(100) NOT NULL,
    amount DECIMAL(10,2) NOT NULL,
    billingcycle VARCHAR(50) NOT NULL DEFAULT 'Mensile',
    startdate DATE NOT NULL,
    enddate DATE NULL,
    category VARCHAR(50) NULL,
    createdat TIMESTAMPTZ(3) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updatedat TIMESTAMPTZ(3) NOT NULL DEFAULT CURRENT_TIMESTAMP,

    CONSTRAINT pk_subscriptions PRIMARY KEY (id),
    CONSTRAINT fk_subscriptions_users FOREIGN KEY (userid) REFERENCES users(id)
);

CREATE INDEX ix_subscriptions_userid ON subscriptions(userid);

-- Tabella subscriptionhistories
CREATE TABLE subscriptionhistories (
    id UUID NOT NULL DEFAULT gen_random_uuid(),
    subscriptionid UUID NOT NULL,
    action VARCHAR(50) NOT NULL,
    oldvalue TEXT NULL,
    newvalue TEXT NULL,
    createdat TIMESTAMPTZ(3) NOT NULL DEFAULT CURRENT_TIMESTAMP,

    CONSTRAINT pk_subscriptionhistories PRIMARY KEY (id),
    CONSTRAINT fk_subscriptionhistories_subscriptions FOREIGN KEY (subscriptionid) REFERENCES subscriptions(id)
);

CREATE INDEX ix_subscriptionhistories_subscriptionid ON subscriptionhistories(subscriptionid);

-- Tabella sharedlinks
CREATE TABLE sharedlinks (
    id UUID NOT NULL DEFAULT gen_random_uuid(),
    userid UUID NOT NULL,
    link VARCHAR(255) NOT NULL,
    expiresat TIMESTAMPTZ(3) NULL,
    views INT NOT NULL DEFAULT 0,
    createdat TIMESTAMPTZ(3) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updatedat TIMESTAMPTZ(3) NOT NULL DEFAULT CURRENT_TIMESTAMP,

    CONSTRAINT pk_sharedlinks PRIMARY KEY (id),
    CONSTRAINT fk_sharedlinks_users FOREIGN KEY (userid) REFERENCES users(id)
);

CREATE INDEX ix_sharedlinks_userid ON sharedlinks(userid);

-- Trigger updatedat per users
CREATE OR REPLACE FUNCTION set_updatedat_users()
RETURNS TRIGGER AS $$
BEGIN
    NEW.updatedat = CURRENT_TIMESTAMP;
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER tr_users_update
BEFORE UPDATE ON users
FOR EACH ROW
EXECUTE FUNCTION set_updatedat_users();

<span style="color:gray">Trigger updatedat per subscriptions</span>
CREATE OR REPLACE FUNCTION set_updatedat_subscriptions()
RETURNS TRIGGER AS $$
BEGIN
    NEW.updatedat = CURRENT_TIMESTAMP;
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER tr_subscriptions_update
BEFORE UPDATE ON subscriptions
FOR EACH ROW
EXECUTE FUNCTION set_updatedat_subscriptions();

<span style="color:gray">Trigger updatedat per sharedlinks</span>
CREATE OR REPLACE FUNCTION set_updatedat_sharedlinks()
RETURNS TRIGGER AS $$
BEGIN
    NEW.updatedat = CURRENT_TIMESTAMP;
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER tr_sharedlinks_update
BEFORE UPDATE ON sharedlinks
FOR EACH ROW
EXECUTE FUNCTION set_updatedat_sharedlinks();

-- Tabella outboxmessages (x OUTBOX PATTERN (e.g.save user + send email(domain event) = 1 transazione
CREATE TABLE outboxmessages (
    id UUID NOT NULL,
    type TEXT NOT NULL,
    payload JSONB NOT NULL,
    occurredonutc TIMESTAMPTZ NOT NULL,
    processedonutc TIMESTAMPTZ NULL,

    CONSTRAINT pk_outboxmessages PRIMARY KEY (id)
);

CREATE INDEX ix_outboxmessages_processed
ON outboxmessages(processedonutc);

CREATE TABLE usermedia (
    id UUID PRIMARY KEY,
    userid UUID NOT NULL,
    objectkey TEXT NOT NULL,
    contenttype VARCHAR(100),
    size BIGINT,
    uploadedat TIMESTAMPTZ NOT NULL,

    CONSTRAINT fk_usermedia_user
        FOREIGN KEY (userid) REFERENCES users(id)
);

CREATE INDEX ix_usermedia_userid ON usermedia(userid);
```