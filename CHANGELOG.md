# Changelog

Format [Keep a Changelog](https://keepachangelog.com/fr/1.1.0/).

## [Non publié]

### Ajouté
- API : comptes (JWT, rôles Participant/Organisateur), catalogue d'événements filtrable, réservations avec commission, annulation, espace organisateur (statistiques, participants), paiement simulé.
- Application Angular : catalogue, détail et réservation, connexion/inscription, mes billets, espace organisateur.
- Tests (API : 18, front : 7), ESLint, Prettier, CI GitHub Actions.
- Docker : images back et front, `docker-compose.yml`.
- Documentation : README, concept, architecture, décisions, audit, roadmap, rapport final.

### Modifié
- Angular épinglé en 21.2.25/21.2.26 (corrige les vulnérabilités `npm audit`).

### Supprimé
- Template `WeatherForecast` et page placeholder Angular.
- Artefacts de build retirés du suivi git.
