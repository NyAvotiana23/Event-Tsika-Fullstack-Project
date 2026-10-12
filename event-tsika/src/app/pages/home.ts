import { Component, inject, signal } from '@angular/core';
import { DatePipe, DecimalPipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { ApiService, errorMessage } from '../core/api.service';
import { CATEGORIES, CATEGORY_LABELS, EventDto, EventFilters } from '../core/models';

@Component({
  selector: 'app-home',
  imports: [FormsModule, RouterLink, DatePipe, DecimalPipe],
  template: `
    <section class="hero">
      <h1>Découvrez les événements près de chez vous</h1>
      <p>Concerts, conférences, sport, culture et causes caritatives partout à Madagascar.</p>
    </section>

    <form class="filters" (ngSubmit)="search()">
      <input
        name="q"
        [(ngModel)]="filters.q"
        placeholder="Rechercher un événement"
        aria-label="Recherche"
      />
      <select name="category" [(ngModel)]="filters.category" aria-label="Catégorie">
        <option value="">Toutes les catégories</option>
        @for (c of categories; track c) {
          <option [value]="c">{{ labels[c] }}</option>
        }
      </select>
      <input name="city" [(ngModel)]="filters.city" placeholder="Ville" aria-label="Ville" />
      <button type="submit">Filtrer</button>
    </form>

    @if (error()) {
      <p class="error" role="alert">{{ error() }}</p>
    }
    @if (loading()) {
      <p>Chargement…</p>
    } @else if (events().length === 0 && !error()) {
      <p class="muted">Aucun événement à venir ne correspond à votre recherche.</p>
    }

    <div class="grid">
      @for (e of events(); track e.id) {
        <a class="card" [routerLink]="['/evenements', e.id]">
          <span class="badge">{{ labels[e.category] }}</span>
          <h2>{{ e.title }}</h2>
          <p class="muted">{{ e.startsAtUtc | date: 'dd/MM/yyyy HH:mm' }} · {{ e.city }}</p>
          <p class="price">{{ e.priceMga === 0 ? 'Gratuit' : (e.priceMga | number) + ' Ar' }}</p>
          <p class="muted">{{ e.seatsLeft > 0 ? e.seatsLeft + ' places restantes' : 'Complet' }}</p>
        </a>
      }
    </div>
  `,
})
export class Home {
  private readonly api = inject(ApiService);

  protected readonly categories = CATEGORIES;
  protected readonly labels = CATEGORY_LABELS;
  protected filters: EventFilters = { q: '', category: '', city: '' };
  protected readonly events = signal<EventDto[]>([]);
  protected readonly loading = signal(true);
  protected readonly error = signal('');

  constructor() {
    this.search();
  }

  protected search(): void {
    this.loading.set(true);
    this.error.set('');
    this.api.listEvents(this.filters).subscribe({
      next: (events) => {
        this.events.set(events);
        this.loading.set(false);
      },
      error: (e) => {
        this.error.set(errorMessage(e));
        this.loading.set(false);
      },
    });
  }
}
