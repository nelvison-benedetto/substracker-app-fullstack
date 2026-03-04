## SUBSNAP BACKEND – ARCHITECTURE & ENGINEERING OVERVIEW

Author: **Nelvison Benedetto**</br>
Project: SubSnap Backend</br>
Platform: ASP.NET Core (.NET 8)</br>
Database: PostgreSQL</br>
ORM: Entity Framework Core</br>
Architecture: Clean Architecture + Domain-Driven Design + CQRS</br>
Style: Domain-First + Vertical Slice Architecture</br>
Messaging Pattern: MediatR (internal only)</br>
Maturity Level: Production-grade backend foundation</br>

SubSnap is intentionally designed following modern enterprise backend practices inspired by systems used at companies such as Uber, Stripe, Shopify and large-scale ASP.NET Core platforms.

---
### .CORE layer
Contains pure business logic (no framework logic!), must be runnable without external references.

Contains:
DOMAIN [Entities, Aggregates, Value Objects, Events, Domain Rules, ...].

references: NONE!!

---

### .APPLICATION layer
Contains:
Behaviors(MediatR pipeline), DependencyInjection (only for .application level), Ports (interfaces), UseCases(slices e.g.Login, Logout,...)(each slice contains Orchestrator(the TRUE entry point), Handler, Command, Result, Policies, Loaders).

references: .Core

---

### .INFRASTRUCTURE layer
Technical implementations.
Contains:
EF Core repositories, JWT generation, Password hashing, Entities Configuration, ApplicationDbContext, UnitofWork, DataLoaders(Aggregates & Batch Loaders), DependencyInjection (only for .infrastructure level), Storage(for Hetzner Object media files), OutBox Processor.

references: .Application, .Core

---

### .API layer
HTTP API only.

Contains:
Requests & Responses (will match w Command & Result of the target UseCase), Mapping(x auto match request->command & result->response), ApiResult & ApiError (wrapper, for uniformity when return the response to the client), Controllers(don't know MediatR, they call the orchestrator of target usecase), Filters, Middleware(for global exception and correlationid for logging), Startup Extensions (authentication, authorization, correlationid, cors, healthchecks, swagger, validation), Validators(usa plugin Fluent Validator, for rules e.g. email must not be empty), Versioning, Program.cs.

references: .Application, .Infrastructure

---

### Techniques & Patterns

- **Domain-First**: the domain model (in .Core) represents the source of truth, cosi il databse è scollegato(facilmente sostituibile!) ed non è trattato come system fundation (approach Db-First).
- **Clean Architecture**: project suddiviso in layers .API .Application .Infrastructure .Core, e le loro references. 
- **Domain Driven Design (DDD)**: 
- **Boundaries tra Assemblies(projects)**: .applications/ports qui vengono definiti i contratti, la vero implementazione è dentro .infrastructure e.g.IUserRepository -> UserRepository, in servicecollectionextensions.cs setti che qundo chiami l'interfaccia automaticamente viene chiamata l'implementazione, obbligatorio perche un prj non deve conosce le implementazioni dell'altro prj, ma solo le interfaccie: i boundaries sono rispettati (e i tests sono clean).
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
- **Outbox Pattern(reliable events)**: per assicurarsi di e.g registrare utente + inviare email in una 1 sola transazione(COMMIT) (altrimenti magari la registrazione user ha successo ed è salvato su db, ma poi l'invio email fallisce). quindi Save Aggregate -> Save OutBox message -> COMMIT(salvi su db), dopodiche il background worker outboxprocessor.cs sempre attivo in modalità polling ogni 2sec legge ultime 20 righe della tabella db  outboxmessage (solo quelli non ancora done) e pubblica quegli eventi (e.g. invio email di benvenuto)!
- **Observality & Logging**: sempre chiaro logging di cosa è appena successo, ogni log fornito di chiara descrizione e di correlationId per poter facilmente tracciare cosa succede in real-time. see loggingbehavior.cs correlationidmiddleware.cs
- **Security**: jwt tokens, access tokens, refresh tokens, jwt claims
- **Media Upload**: su tab db usermedia salvo solo i paths, i media files che l'utente carica vengono salvati su Hetzner Object.

## Perchè questo progetto è scalabile

Questo progetto è in via di production-oriented backend architecture blueprint.

Perchè questa architettura è scalabile
 Explicit transactional boundaries
 Event-driven extensibility
 Stateless API design
 Independent read/write models
 Background processing support
 Infrastructure replaceability

Each UseCase approximates an internal microservice.
Production-grade backend foundation.


## SUBSNAP FRONTEND – ARCHITECTURE & ENGINEERING OVERVIEW

ReactJs, Typescript, TailwindCSS, React-Query, React-Redux, React-Router, DaisyUI.

## SUBSNAP DATABASE – ARCHITECTURE & ENGINEERING OVERVIEW

<span style="color:gray">Abilita estensione per UUID</span>
CREATE EXTENSION IF NOT EXISTS "pgcrypto";

<span style="color:gray">Tabella users</span>
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

<span style="color:gray">Tabella refreshtokens</span>
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

<span style="color:gray">Tabella subscriptions</span>
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

<span style="color:gray">Tabella subscriptionhistories</span>
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

<span style="color:gray">Tabella sharedlinks</span>
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

<span style="color:gray">Trigger updatedat per users</span>
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

<span style="color:gray">Tabella outboxmessages (x OUTBOX PATTERN (e.g.save user + send email(domain event) = 1 transazione</span>
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
