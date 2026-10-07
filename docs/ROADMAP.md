# Roadmap (hors périmètre du MVP)

Issues du concept (`evenement.txt`, SWOT) ou des limites actuelles. Rien ici n'est implémenté.

## Prioritaire pour une mise en production
- Intégration réelle d'un opérateur Mobile Money (MVola, Orange Money, Airtel Money) derrière `IPaymentGateway`, avec webhook de confirmation.
- Migration vers PostgreSQL et transaction sérialisable pour la capacité (ADR-005).
- Limitation de débit sur l'authentification, rafraîchissement de jeton, cookie `HttpOnly` à la place de `localStorage`.
- Vérification d'email et validation des organisateurs.
- Billet avec QR code et contrôle à l'entrée (anti-revente / scalping).
- HTTPS (reverse proxy) et sauvegarde de la base.

## Fonctionnalités du concept non faites
- Notifications selon les centres d'intérêt et événements populaires.
- Partage sur les réseaux sociaux.
- Messages de l'organisateur aux participants.
- Satisfaction des participants dans les statistiques.

## Améliorations proposées par le concept
Application mobile, système de notes et avis, abonnements premium, partenariats locaux (repas, hébergement, transport), événements en direct, gamification, support multilingue (malgache, français, anglais), publicité.

## Technique
- Tests de bout en bout (Playwright).
- Mesure de la couverture de tests.
- Hooks pre-commit (formatage, lint).
- Pagination côté interface du catalogue.
