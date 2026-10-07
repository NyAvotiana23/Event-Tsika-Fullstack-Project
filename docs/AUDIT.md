# Audit de l'état initial

Date : 2026-10-07. Commit audité : `ccd4648` (« Initial commit »).

## Constat principal

Le dépôt contenait deux projets générés (Angular CLI et template ASP.NET « WeatherForecast ») sans aucune fonctionnalité métier. Le concept n'existait que dans `evenement.txt` et `E-ventsika.pptx`. Le travail est donc un développement du MVP et non une finition.

## Cartographie initiale

| Élément | État |
|---|---|
| `event-tsika` (Angular 21, SSR, vitest) | Template `ng new` : page placeholder, `routes = []`, test attendant « Hello, event-tsika » |
| `event-tsika-back` (ASP.NET Core net10.0) | `WeatherForecastController`, aucun modèle, aucune BDD, aucune authentification |
| `docs/project-structure.txt` (front) | Architecture cible jamais réalisée |
| Git | 1 commit ; 46 fichiers `bin/`, `obj/`, `.vs/` versionnés ; pas de `.gitignore` |

## Problèmes et priorité

| Priorité | Problème | Traitement |
|---|---|---|
| Bloquant | Aucune fonctionnalité | Implémenté (voir CONCEPT) |
| Bloquant | `dotnet build` impossible : SDK `10.0.100-rc.1` sans runtime 10 | SDK 10.0.401 installé en local (`~/.dotnet10`), `global.json` ajouté |
| Bloquant | Aucune décision de stack (BDD, auth, paiement) | ADR-002 à 004 |
| Important | Artefacts de build versionnés, pas de `.gitignore` | Retirés de l'index, `.gitignore` ajouté |
| Important | Test frontend faux (template) | Remplacé |
| Important | Aucun lint, CI, `.env.example`, README utile | Ajoutés |
| Important | `npm audit` : 36 vulnérabilités (3 critiques) | 0 après mise à jour d'Angular (ADR-008) |
| Important | `Microsoft.OpenApi` 2.0.0 vulnérable (transitif) | Corrigé par mise à jour de `Microsoft.AspNetCore.OpenApi` |
| Confort | `.vscode/mcp.json`, `.csproj.user` | `.user` désindexé ; `mcp.json` conservé |
| Confort | `event-tsika/docs/project-structure.txt` obsolète | Conservé (suppression soumise à accord) ; remplacé par `docs/ARCHITECTURE.md` |

## Sécurité (état final)

- Aucun secret dans le code ni dans l'historique : la clé JWT vient de l'environnement ; l'API refuse de démarrer sans elle.
- Validation des entrées par DataAnnotations ; requêtes EF Core paramétrées ; échappement Angular par défaut.
- Contrôle de propriété sur toutes les routes organisateur et réservation (tests dédiés).
- Points ouverts : pas de limitation de débit sur `/login`, pas de rafraîchissement de jeton, le jeton est stocké dans `localStorage` (exposé en cas de XSS). Voir la roadmap.

## Couverture de tests

Avant : 1 test (échouant). Après : 18 tests API (unitaires + intégration) et 7 tests front. Couverture non mesurée en pourcentage.
