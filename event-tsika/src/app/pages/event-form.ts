import { ChangeDetectorRef, Component, inject, input, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { ApiService, errorMessage } from '../core/api.service';
import { CATEGORIES, CATEGORY_LABELS, EventCategory } from '../core/models';

/** Valeur d'un `<input type="datetime-local">` (heure locale) à partir d'une date ISO UTC. */
export function toLocalInput(iso: string): string {
  const d = new Date(iso);
  const pad = (n: number) => String(n).padStart(2, '0');
  return `${d.getFullYear()}-${pad(d.getMonth() + 1)}-${pad(d.getDate())}T${pad(d.getHours())}:${pad(d.getMinutes())}`;
}

/** Création (pas de paramètre `id`) ou modification d'un événement. */
@Component({
  selector: 'app-event-form',
  imports: [FormsModule, RouterLink],
  template: `
    <section class="panel narrow">
      <h1>{{ id() ? "Modifier l'événement" : 'Nouvel événement' }}</h1>
      <form (ngSubmit)="save()">
        <label
          >Titre <input name="title" [(ngModel)]="form.title" required maxlength="150"
        /></label>
        <label
          >Description
          <textarea
            name="description"
            [(ngModel)]="form.description"
            required
            rows="5"
            maxlength="4000"
          ></textarea>
        </label>
        <label>
          Catégorie
          <select name="category" [(ngModel)]="form.category">
            @for (c of categories; track c) {
              <option [value]="c">{{ labels[c] }}</option>
            }
          </select>
        </label>
        <label>Ville <input name="city" [(ngModel)]="form.city" required maxlength="100" /></label>
        <label>Lieu <input name="venue" [(ngModel)]="form.venue" required maxlength="150" /></label>
        <label
          >Date et heure
          <input type="datetime-local" name="startsAt" [(ngModel)]="form.startsAtLocal" required
        /></label>
        <label
          >Capacité
          <input type="number" name="capacity" min="1" [(ngModel)]="form.capacity" required
        /></label>
        <label
          >Prix par place en Ar (0 = gratuit)
          <input type="number" name="price" min="0" [(ngModel)]="form.priceMga" required
        /></label>
        @if (error()) {
          <p class="error" role="alert">{{ error() }}</p>
        }
        <div class="row">
          <a class="button secondary" routerLink="/organisateur">Annuler</a>
          <button type="submit" [disabled]="busy()">Enregistrer</button>
        </div>
      </form>
    </section>
  `,
})
export class EventForm {
  private readonly api = inject(ApiService);
  private readonly router = inject(Router);
  private readonly cdr = inject(ChangeDetectorRef);

  /** Fourni par la route `organisateur/evenements/:id` ; absent à la création. */
  readonly id = input<string>();

  protected readonly categories = CATEGORIES;
  protected readonly labels = CATEGORY_LABELS;
  protected form = {
    title: '',
    description: '',
    category: 'Culture' as EventCategory,
    city: '',
    venue: '',
    startsAtLocal: '',
    capacity: 50,
    priceMga: 0,
  };
  protected readonly error = signal('');
  protected readonly busy = signal(false);

  constructor() {
    queueMicrotask(() => this.loadExisting());
  }

  protected save(): void {
    this.busy.set(true);
    this.error.set('');
    const { startsAtLocal, ...rest } = this.form;
    const input = { ...rest, startsAtUtc: new Date(startsAtLocal).toISOString() };
    const id = this.id();
    (id ? this.api.updateEvent(id, input) : this.api.createEvent(input)).subscribe({
      next: () => void this.router.navigateByUrl('/organisateur'),
      error: (e) => {
        this.error.set(errorMessage(e));
        this.busy.set(false);
      },
    });
  }

  private loadExisting(): void {
    const id = this.id();
    if (!id) return;
    this.api.getEvent(id).subscribe({
      next: (e) => {
        const { title, description, category, city, venue, capacity, priceMga } = e;
        this.form = {
          title,
          description,
          category,
          city,
          venue,
          capacity,
          priceMga,
          startsAtLocal: toLocalInput(e.startsAtUtc),
        };
        this.cdr.markForCheck();
      },
      error: (e) => this.error.set(errorMessage(e)),
    });
  }
}
