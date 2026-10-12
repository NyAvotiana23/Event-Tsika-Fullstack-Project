# Rapport final

Date : 2026-10-07. Branche : `chore/finalisation` (aucun push, aucune fusion dans `main`).

## Ce qui a été fait

- Audit ([AUDIT.md](AUDIT.md)) : le dépôt n'était qu'un double squelette ; le concept a été reconstitué depuis `evenement.txt` et `E-ventsika.pptx` ([CONCEPT.md](CONCEPT.md)).
- Hygiène git : artefacts `bin/obj/.vs` retirés de l'index, `.gitignore`.
- API ASP.NET Core : comptes JWT, catalogue, réservations avec commission 5 %, annulation et remboursement simulé, espace organisateur, migrations EF Core.
- Application Angular : catalogue filtrable, détail et réservation, connexion/inscription, mes billets, espace organisateur (CRUD, participants, statistiques).
- Qualité : 18 tests API (unitaires + intégration), 7 tests front, ESLint, Prettier, CI GitHub Actions, `npm audit` à 0 vulnérabilité, aucun paquet NuGet vulnérable.
- Docker : `docker-compose.yml`, images back (aspnet non-root) et front (nginx).
- Documentation : README, CONCEPT, ARCHITECTURE, DECISIONS, AUDIT, ROADMAP, CONTRIBUTING, CHANGELOG, `.env.example`.

## Vérifications réalisées

| Vérification | Résultat |
|---|---|
| `dotnet test` | 18/18 |
| `npm test` | 7/7 (le premier lancement après installation expire parfois sous Windows ; le relancer passe) |
| `npm run lint`, `format:check`, `ng build` | OK |
| API lancée en réel : inscription, création d'événement, filtre | OK |
| `docker compose up --build` : site (8081), route profonde SPA, inscription via nginx, persistance après redémarrage | OK |

Non vérifié : la CI GitHub Actions (jamais exécutée), l'interface dans un navigateur (pas de test visuel ni de bout en bout), le README suivi depuis une machine vierge.

## Ce qui reste

- Paiement Mobile Money réel (la passerelle est simulée), notifications, partage social, messages aux participants, avis : voir [ROADMAP.md](ROADMAP.md).
- Aucune licence choisie.
- `event-tsika/docs/project-structure.txt` est obsolète ; je ne l'ai pas supprimé sans votre accord.

## Décisions et hypothèses

Toutes les décisions sont dans [DECISIONS.md](DECISIONS.md). Choix pris à votre place (vous aviez demandé de suivre mes recommandations) : périmètre MVP, **SQLite** plutôt que PostgreSQL (non testable ici sans service externe), JWT maison, paiement simulé, interface en français, SPA à rendu client.

## Points d'attention

- Le SDK .NET installé sur cette machine (10.0.100-rc.1) ne fonctionnait pas ; un SDK 10.0.401 a été installé dans `~/.dotnet10` pour le développement (hors dépôt).
- La survente est évitée par un verrou en mémoire : une seule instance d'API (ADR-005).
- Le jeton est dans `localStorage` et il n'y a pas de limitation de débit sur la connexion : à traiter avant la production.
- Angular est épinglé en 21.2.25/26 ; à surveiller lors des mises à jour.

## Prochaines étapes recommandées

1. Relire et fusionner `chore/finalisation` dans `main` (pull request).
2. Choisir une licence.
3. Intégrer un opérateur Mobile Money et passer à PostgreSQL.
4. Ajouter le QR code sur les billets et les notifications.
5. Tester le README sur une machine vierge et lancer la CI.
