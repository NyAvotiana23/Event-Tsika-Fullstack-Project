# Concept

Sources : `evenement.txt` et la présentation `E-ventsika.pptx` (dossier du projet de cours, hors dépôt). Le dépôt initial ne contenait que deux squelettes (Angular et ASP.NET) sans aucune fonctionnalité ; le concept ci-dessous est donc celui des documents, pas celui d'un code existant.

## Vision

E-Ventsika est une plateforme web qui simplifie la découverte, la réservation et la gestion d'événements à Madagascar. Objectifs : moderniser la gestion événementielle, valoriser la culture locale, faciliter l'accès aux événements, favoriser l'engagement communautaire.

## Public cible

- Participants : tous genres, de 13 ans à senior, classe moyenne ou plus, sachant lire, partout à Madagascar, avec un appareil connecté.
- Organisateurs : amateurs ou professionnels, associations, entreprises.

## Modèle économique

Commission (~5 %) sur les billets payants ; les événements gratuits n'en génèrent pas. Publicité et abonnements premium sont cités comme sources de revenus futures.

## Fonctionnalités

| Fonctionnalité du concept | État |
|---|---|
| Découvrir des événements par catégorie | Fait (catégories, texte, ville) |
| Réserver des places, gratuites ou payantes | Fait |
| Paiement en ligne sécurisé | **Simulé** : pas de contrat marchand Mobile Money (ADR-004) |
| Créer / publier des événements | Fait |
| Suivre les réservations | Fait (liste des participants, statistiques) |
| Statistiques de fréquentation | Fait (remplissage, chiffre d'affaires, commission) ; la satisfaction dépend des avis (roadmap) |
| Notifications sur les événements populaires ou d'intérêt | Non fait : roadmap |
| Partager sur les réseaux sociaux | Non fait : roadmap |
| Communiquer avec les participants | Non fait : roadmap |

## Choix de conception à préserver

- Commission calculée par réservation et figée au moment de l'achat (le taux peut évoluer sans réécrire l'historique).
- Un événement gratuit n'a ni paiement ni commission.
- Menaces identifiées dans le SWOT, traitées dans le code : survente (verrou de capacité), annulation (libération des places, remboursement via la passerelle), revente de billets (code unique par réservation). Non traitées : fraude au paiement, scalping avancé.
- Interface en français (malgache et anglais en roadmap).
- Accessible sur mobile : interface responsive, légère (chargement paresseux des pages).

## Hors périmètre (roadmap)

Application mobile, avis et notes, abonnements premium, partenariats locaux, événements en direct, gamification, multilingue : voir [ROADMAP.md](ROADMAP.md).
