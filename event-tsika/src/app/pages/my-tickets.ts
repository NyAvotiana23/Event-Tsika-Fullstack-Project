import { Component, inject, signal } from '@angular/core';
import { DatePipe, DecimalPipe } from '@angular/common';
import { RouterLink } from '@angular/router';
import { ApiService, errorMessage } from '../core/api.service';
import { ReservationDto } from '../core/models';

@Component({
  selector: 'app-my-tickets',
  imports: [DatePipe, DecimalPipe, RouterLink],
  template: `
    <h1>Mes billets</h1>
    @if (error()) {
      <p class="error" role="alert">{{ error() }}</p>
    }
    @if (tickets().length === 0 && !error()) {
      <p class="muted">Aucune réservation. <a routerLink="/">Découvrir les événements</a></p>
    }
    @for (t of tickets(); track t.id) {
      <article class="panel row" [class.cancelled]="t.status === 'Cancelled'">
        <div>
          <h2>
            <a [routerLink]="['/evenements', t.eventId]">{{ t.eventTitle }}</a>
          </h2>
          <p class="muted">
            {{ t.eventStartsAtUtc | date: 'dd/MM/yyyy HH:mm' }} · {{ t.quantity }} place(s) ·
            {{ t.totalMga | number }} Ar
          </p>
          <p>
            Code : <strong>{{ t.ticketCode }}</strong> —
            {{ t.status === 'Confirmed' ? 'Confirmée' : 'Annulée' }}
          </p>
        </div>
        @if (t.status === 'Confirmed') {
          <button type="button" class="secondary" (click)="cancel(t)">Annuler</button>
        }
      </article>
    }
  `,
})
export class MyTickets {
  private readonly api = inject(ApiService);
  protected readonly tickets = signal<ReservationDto[]>([]);
  protected readonly error = signal('');

  constructor() {
    this.load();
  }

  protected cancel(ticket: ReservationDto): void {
    if (!confirm(`Annuler la réservation « ${ticket.eventTitle} » ?`)) return;
    this.error.set('');
    this.api.cancelReservation(ticket.id).subscribe({
      next: () => this.load(),
      error: (e) => this.error.set(errorMessage(e)),
    });
  }

  private load(): void {
    this.api.myReservations().subscribe({
      next: (t) => this.tickets.set(t),
      error: (e) => this.error.set(errorMessage(e)),
    });
  }
}
