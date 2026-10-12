import { Component, inject, signal } from '@angular/core';
import { DatePipe, DecimalPipe, PercentPipe } from '@angular/common';
import { RouterLink } from '@angular/router';
import { ApiService, errorMessage } from '../core/api.service';
import { EventStatsDto, ParticipantDto } from '../core/models';

@Component({
  selector: 'app-organizer-dashboard',
  imports: [RouterLink, DatePipe, DecimalPipe, PercentPipe],
  template: `
    <div class="row">
      <h1>Espace organisateur</h1>
      <a class="button" routerLink="/organisateur/evenements/nouveau">Nouvel événement</a>
    </div>
    @if (error()) {
      <p class="error" role="alert">{{ error() }}</p>
    }
    @if (stats().length === 0 && !error()) {
      <p class="muted">Vous n'avez pas encore créé d'événement.</p>
    }
    @for (s of stats(); track s.eventId) {
      <article class="panel">
        <div class="row">
          <h2>{{ s.title }}</h2>
          <span>
            <a class="button secondary" [routerLink]="['/organisateur/evenements', s.eventId]"
              >Modifier</a
            >
            <button type="button" class="secondary" (click)="toggleParticipants(s.eventId)">
              Participants
            </button>
            <button type="button" class="danger" (click)="remove(s)">Supprimer</button>
          </span>
        </div>
        <dl class="stats">
          <div>
            <dt>Places vendues</dt>
            <dd>{{ s.seatsSold }} / {{ s.capacity }}</dd>
          </div>
          <div>
            <dt>Remplissage</dt>
            <dd>{{ s.fillRate | percent }}</dd>
          </div>
          <div>
            <dt>Chiffre d'affaires</dt>
            <dd>{{ s.revenueMga | number }} Ar</dd>
          </div>
          <div>
            <dt>Commission plateforme</dt>
            <dd>{{ s.commissionMga | number }} Ar</dd>
          </div>
          <div>
            <dt>Net organisateur</dt>
            <dd>{{ s.netMga | number }} Ar</dd>
          </div>
        </dl>
        @if (openId() === s.eventId) {
          <table>
            <thead>
              <tr>
                <th>Nom</th>
                <th>Places</th>
                <th>Code</th>
                <th>Statut</th>
                <th>Date</th>
              </tr>
            </thead>
            <tbody>
              @for (p of participants(); track p.ticketCode) {
                <tr>
                  <td>{{ p.displayName }}</td>
                  <td>{{ p.quantity }}</td>
                  <td>{{ p.ticketCode }}</td>
                  <td>{{ p.status === 'Confirmed' ? 'Confirmée' : 'Annulée' }}</td>
                  <td>{{ p.createdAtUtc | date: 'dd/MM/yyyy HH:mm' }}</td>
                </tr>
              } @empty {
                <tr>
                  <td colspan="5" class="muted">Aucune réservation.</td>
                </tr>
              }
            </tbody>
          </table>
        }
      </article>
    }
  `,
})
export class OrganizerDashboard {
  private readonly api = inject(ApiService);
  protected readonly stats = signal<EventStatsDto[]>([]);
  protected readonly participants = signal<ParticipantDto[]>([]);
  protected readonly openId = signal<string | null>(null);
  protected readonly error = signal('');

  constructor() {
    this.load();
  }

  protected toggleParticipants(eventId: string): void {
    if (this.openId() === eventId) {
      this.openId.set(null);
      return;
    }
    this.api.participants(eventId).subscribe({
      next: (p) => {
        this.participants.set(p);
        this.openId.set(eventId);
      },
      error: (e) => this.error.set(errorMessage(e)),
    });
  }

  protected remove(s: EventStatsDto): void {
    if (!confirm(`Supprimer « ${s.title} » ?`)) return;
    this.error.set('');
    this.api.deleteEvent(s.eventId).subscribe({
      next: () => this.load(),
      error: (e) => this.error.set(errorMessage(e)),
    });
  }

  private load(): void {
    this.api.organizerEvents().subscribe({
      next: (s) => this.stats.set(s),
      error: (e) => this.error.set(errorMessage(e)),
    });
  }
}
