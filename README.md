# E-Ventsika

E-Ventsika est une plateforme web de gestion d'événements pour Madagascar : les visiteurs découvrent, réservent et paient des places ; les organisateurs publient leurs événements et suivent leurs réservations. La plateforme prélève une commission (5 % par défaut) sur les billets payants. Le projet comprend une API ASP.NET Core et une application Angular.

## Fonctionnalités

- Catalogue d'événements à venir, avec recherche texte, catégorie et ville (culture, sport, éducation, divertissement, professionnel, caritatif).
- Comptes avec rôles Participant / Organisateur (JWT).
- Réservation de 1 à 10 places, événements gratuits ou payants, billet avec code unique, annulation avant l'événement.
- Espace organisateur : création, modification et suppression d'événements, liste des participants, statistiques (places vendues, remplissage, chiffre d'affaires, commission, net).
- Paiement **simulé** (voir [limites](#limites-connues)).

Le concept complet est décrit dans [docs/CONCEPT.md](docs/CONCEPT.md).

## Prérequis

| Usage | Outils |
|---|---|
| Docker (le plus simple) | Docker avec Compose |
| Développement | .NET SDK 10.0.1xx (voir `event-tsika-back/global.json`), Node.js 24 + npm 10 |

## Lancement avec Docker

```bash
cp .env.example .env        # puis renseigner JWT_KEY (32 caractères minimum)
docker compose up --build
```

Ouvrir http://localhost:8080 (port modifiable avec `WEB_PORT`). La base SQLite est conservée dans le volume `db-data`.

## Lancement en développement

Terminal 1, API (http://localhost:5104) :

```bash
cd event-tsika-back/event-tsika-back
dotnet user-secrets set "Jwt:Key" "une-cle-secrete-d-au-moins-32-caracteres"
dotnet run --launch-profile http
```

Terminal 2, application Angular (http://localhost:4200, les appels `/api` sont proxifiés vers l'API) :

```bash
cd event-tsika
npm ci
npm start
```

La base `event-tsika.db` est créée et migrée automatiquement au démarrage de l'API.

## Premiers pas

1. S'inscrire en cochant « Je suis organisateur » puis créer un événement depuis l'espace organisateur.
2. Se déconnecter, créer un compte participant, réserver des places depuis la page de l'événement.
3. Revenir sur le compte organisateur pour voir statistiques et participants.

Documentation interactive de l'API : `http://localhost:5104/openapi/v1.json` (mode Développement).

## Configuration

Toutes les variables sont décrites dans [.env.example](.env.example). Seule `JWT_KEY` / `Jwt__Key` est obligatoire : l'API refuse de démarrer sans elle.

## Tests, lint, build

```bash
# API (xUnit, tests d'intégration sur SQLite temporaire)
cd event-tsika-back && dotnet test

# Front (vitest, ESLint, Prettier)
cd event-tsika && npm test && npm run lint && npm run format:check && npm run build
```

Sous Windows, le premier `npm test` après une installation peut dépasser le délai de démarrage de vitest ; relancer la commande suffit.

La CI GitHub Actions ([.github/workflows/ci.yml](.github/workflows/ci.yml)) exécute ces mêmes commandes.

## Structure du projet

```
event-tsika-back/             API ASP.NET Core (net10.0)
  event-tsika-back/           Controllers, Services, Data (EF Core), Domain, Dtos, Migrations
  event-tsika-back.Tests/     Tests unitaires et d'intégration
event-tsika/                  Application Angular 21
  src/app/core/               API, authentification, intercepteur, guards, modèles
  src/app/pages/              Pages (catalogue, détail, auth, billets, espace organisateur)
docs/                         Concept, architecture, décisions, audit, roadmap, rapport final
docker-compose.yml            Back + front (nginx)
```

Détails : [docs/ARCHITECTURE.md](docs/ARCHITECTURE.md). Contribuer : [CONTRIBUTING.md](CONTRIBUTING.md).

## Limites connues

- Le paiement Mobile Money (MVola, Orange Money, Airtel Money) n'est pas branché : `SimulatedPaymentGateway` accepte tout paiement. L'interface `IPaymentGateway` est le point d'intégration.
- Pas de notifications, de partage social ni d'avis : voir [docs/ROADMAP.md](docs/ROADMAP.md).
- Instance unique : la protection contre la survente est un verrou en mémoire (ADR-005).

## Licence

Aucune licence n'est définie à ce jour : tous droits réservés par les auteurs, à choisir avant toute diffusion.
