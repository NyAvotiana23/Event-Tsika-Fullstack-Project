# Contribuer

## Workflow git

- Ne jamais travailler sur `main` : créer une branche (`feat/...`, `fix/...`, `chore/...`).
- Un commit par intention, au format Conventional Commits (`feat:`, `fix:`, `refactor:`, `test:`, `docs:`, `chore:`).
- Ne pas mélanger refactorisation et changement de comportement dans un même commit.
- Proposer le changement par pull request ; la CI doit être verte.

## Avant de pousser

```bash
# API
cd event-tsika-back && dotnet test

# Front
cd event-tsika && npm test && npm run lint && npm run format:check && npm run build
```

`npm run format` applique Prettier. Les secrets ne se committent jamais : utiliser `.env` (ignoré) et `dotnet user-secrets`.

## Règles du projet

- Le concept (`docs/CONCEPT.md`) prime : une idée d'extension va dans `docs/ROADMAP.md`, pas dans le code.
- Toute décision technique structurante est ajoutée à `docs/DECISIONS.md`.
- Toute fonctionnalité arrive avec ses tests ; toute modification du modèle avec sa migration :

```bash
cd event-tsika-back/event-tsika-back
dotnet tool restore
dotnet tool run dotnet-ef migrations add NomDeLaMigration
```

- Mettre à jour `CHANGELOG.md` (section `Non publié`).
