import { RenderMode, ServerRoute } from '@angular/ssr';

// Les pages dépendent de l'API et de la session : rendu côté client uniquement (voir docs/DECISIONS.md, ADR-006).
export const serverRoutes: ServerRoute[] = [{ path: '**', renderMode: RenderMode.Client }];
