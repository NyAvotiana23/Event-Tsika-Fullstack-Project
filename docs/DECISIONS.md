# Décisions techniques

Format court : contexte, choix, raison, alternatives écartées. Date : 2026-10-07 pour toutes.

## ADR-001 : Périmètre MVP
- Contexte : le dépôt ne contenait que des squelettes.
- Choix : implémenter le cœur du concept (catalogue, comptes, réservation, billets, espace organisateur) ; le reste va dans la roadmap.
- Raison : livrer un produit complet et testé plutôt qu'une ébauche large.
- Écarté : notifications, partage social, avis, messagerie organisateur.

## ADR-002 : SQLite + EF Core avec migrations
- Choix : SQLite (fichier, volume Docker) et migrations appliquées au démarrage.
- Raison : zéro installation, reproductible, tests d'intégration réalistes. Les accès passent par EF Core : un passage à PostgreSQL ne change que le fournisseur et les migrations.
- Écarté : PostgreSQL d'emblée (serveur à installer, non testable ici sans service externe).

## ADR-003 : Authentification JWT maison, rôles Participant / Organisateur
- Choix : JWT (HS256) signé avec `Jwt:Key` obligatoire, hachage PBKDF2-SHA256 (210 000 itérations) de la bibliothèque standard.
- Raison : minimal, aucune dépendance de plus que `JwtBearer`. Le rôle est choisi à l'inscription (l'organisateur n'a pas de validation).
- Écarté : ASP.NET Identity (lourd pour deux rôles) ; rôle Admin (hors concept).
- Conséquence : pas de rafraîchissement de jeton ; durée de vie 120 min, l'intercepteur déconnecte sur 401.

## ADR-004 : Paiement simulé derrière `IPaymentGateway`
- Choix : une interface et une implémentation qui accepte tout. Le remboursement à l'annulation passe par la même interface.
- Raison : MVola, Orange Money et Airtel Money exigent un contrat marchand ; impossible à intégrer ni tester ici.
- Conséquence : l'interface est le seul code à remplacer. L'interface web indique que le paiement est simulé.

## ADR-005 : Verrou de capacité en mémoire
- Choix : `SemaphoreSlim` statique autour de « compter les places puis insérer ».
- Raison : correct pour une instance avec SQLite, simple.
- Limite : plusieurs instances d'API nécessitent une transaction sérialisable ou une contrainte en base (marqué `ponytail:` dans le code).

## ADR-006 : SPA à rendu client malgré le SSR du squelette
- Choix : `RenderMode.Client` pour toutes les routes ; en production, nginx sert les fichiers statiques.
- Raison : les pages dépendent de la session et de l'API ; le prérendu (par défaut du squelette) ne produit rien d'utile et empêche les routes dynamiques. Le serveur SSR (`server.ts`) reste dans le dépôt mais n'est pas déployé.
- Écarté : retirer entièrement SSR (modification structurelle du squelette, réversible plus tard).

## ADR-007 : Outils de qualité
- Choix : ESLint (`angular-eslint`) et Prettier pour le front, vitest ; analyseurs .NET par défaut pour le back.
- Raison : conventions appliquées automatiquement, vérifiées en CI.

## ADR-008 : Versions verrouillées
- Choix : Angular épinglé à 21.2.25/21.2.26 (branche LTS 21) pour corriger les vulnérabilités `npm audit` sans changer de version majeure ; SDK .NET fixé par `global.json` ; outil `dotnet-ef` dans `dotnet-tools.json`.

## ADR-009 : Docker
- Choix : deux images (API sur `aspnet:10.0` non-root, front sur nginx) et `docker-compose.yml` ; base dans un volume.
- Raison : lancement en deux commandes, même origine pour front et API.
