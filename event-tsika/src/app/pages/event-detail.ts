import { Component, effect, inject, input, signal } from '@angular/core';
import { DatePipe, DecimalPipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { ApiService, errorMessage } from '../core/api.service';
import { AuthService } from '../core/auth.service';
import { CATEGORY_LABELS, EventDto, ReservationDto } from '../core/models';

const MAX_TICKETS_PER_ORDER = 10;

@Component({
  selector: 'app-event-detail',
  imports: [FormsModule, RouterLink, DatePipe, DecimalPipe],
  template: `
    @if (error() && !event()) {
      <p class="error" role="alert">{{ error() }}</p>
    }
    @if (event(); as e) {
      <article class="panel">
        <span class="badge">{{ labels[e.category] }}</span>
        <h1>{{ e.title }}</h1>
        <p class="muted">
          {{ e.startsAtUtc | date: 'EEEE dd/MM/yyyy HH:mm' }} · {{ e.venue }}, {{ e.city }} · par
          {{ e.organizerName }}
        </p>
        <p class="description">{{ e.description }}</p>
        <p class="price">
          {{ e.priceMga === 0 ? 'Gratuit' : (e.priceMga | number) + ' Ar / place' }}
        </p>
        <p class="muted">{{ e.seatsLeft }} places restantes sur {{ e.capacity }}</p>

        @if (ticket(); as t) {
          <div class="success" role="status">
            Réservation confirmée ! Code de billet : <strong>{{ t.ticketCode }}</strong> ({{
              t.totalMga | number
            }}
            Ar).
            <a routerLink="/mes-billets">Voir mes billets</a>
          </div>
        } @else if (e.seatsLeft === 0) {
          <p class="error">Cet événement est complet.</p>
        } @else if (!auth.isLoggedIn()) {
          <p>
            <a routerLink="/connexion">Connectez-vous</a> ou
            <a routerLink="/inscription">créez un compte</a> pour réserver.
          </p>
        } @else {
          <form class="inline" (ngSubmit)="reserve()">
            <label>
              Nombre de places
              <input
                type="number"
                name="quantity"
                min="1"
                [max]="maxQuantity(e)"
                [(ngModel)]="quantity"
              />
            </label>
            <button type="submit" [disabled]="busy()">
              {{
                e.priceMga === 0 ? 'Réserver' : 'Payer ' + (e.priceMga * quantity | number) + ' Ar'
              }}
            </button>
          </form>
          @if (e.priceMga > 0) {
            <p class="muted">
              Paiement simulé : aucun débit réel n'est effectué (version de démonstration).
            </p>
          }
        }
        @if (error() && event()) {
          <p class="error" role="alert">{{ error() }}</p>
        }
      </article>
    }
  `,
})
export class EventDetail {
  private readonly api = inject(ApiService);
  protected readonly auth = inject(AuthService);

  readonly id = input.required<string>();

  protected readonly labels = CATEGORY_LABELS;
  protected readonly event = signal<EventDto | null>(null);
  protected readonly ticket = signal<ReservationDto | null>(null);
  protected readonly error = signal('');
  protected readonly busy = signal(false);
  protected quantity = 1;

  constructor() {
    effect(() => this.load(this.id()));
  }

  protected maxQuantity(e: EventDto): number {
    return Math.min(MAX_TICKETS_PER_ORDER, e.seatsLeft);
  }

  protected reserve(): void {
    this.busy.set(true);
    this.error.set('');
    this.api.reserve(this.id(), this.quantity).subscribe({
      next: (ticket) => {
        this.ticket.set(ticket);
        this.busy.set(false);
        this.load(this.id());
      },
      error: (e) => {
        this.error.set(errorMessage(e));
        this.busy.set(false);
      },
    });
  }

  private load(id: string): void {
    this.api.getEvent(id).subscribe({
      next: (e) => this.event.set(e),
      error: (e) => this.error.set(errorMessage(e)),
    });
  }
}
