import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpErrorResponse, HttpParams } from '@angular/common/http';
import {
  EventDto,
  EventFilters,
  EventInput,
  EventStatsDto,
  ParticipantDto,
  ReservationDto,
} from './models';

/** Message lisible à partir d'une erreur de l'API (ProblemDetails ou erreurs de validation). */
export function errorMessage(error: unknown): string {
  if (!(error instanceof HttpErrorResponse)) return 'Une erreur inattendue est survenue.';
  if (error.status === 0) return 'Le serveur est injoignable. Vérifiez votre connexion Internet.';
  const body = error.error as { title?: string; errors?: Record<string, string[]> } | null;
  const firstValidation = body?.errors ? Object.values(body.errors)[0]?.[0] : undefined;
  return firstValidation ?? body?.title ?? `Erreur ${error.status}`;
}

@Injectable({ providedIn: 'root' })
export class ApiService {
  private readonly http = inject(HttpClient);

  listEvents(filters: EventFilters = {}) {
    let params = new HttpParams();
    if (filters.q) params = params.set('q', filters.q);
    if (filters.category) params = params.set('category', filters.category);
    if (filters.city) params = params.set('city', filters.city);
    return this.http.get<EventDto[]>('/api/events', { params });
  }

  getEvent(id: string) {
    return this.http.get<EventDto>(`/api/events/${id}`);
  }

  createEvent(input: EventInput) {
    return this.http.post<EventDto>('/api/events', input);
  }

  updateEvent(id: string, input: EventInput) {
    return this.http.put<EventDto>(`/api/events/${id}`, input);
  }

  deleteEvent(id: string) {
    return this.http.delete<void>(`/api/events/${id}`);
  }

  reserve(eventId: string, quantity: number) {
    return this.http.post<ReservationDto>(`/api/events/${eventId}/reservations`, { quantity });
  }

  myReservations() {
    return this.http.get<ReservationDto[]>('/api/reservations/mine');
  }

  cancelReservation(id: string) {
    return this.http.post<ReservationDto>(`/api/reservations/${id}/cancel`, null);
  }

  organizerEvents() {
    return this.http.get<EventStatsDto[]>('/api/organizer/events');
  }

  participants(eventId: string) {
    return this.http.get<ParticipantDto[]>(`/api/organizer/events/${eventId}/participants`);
  }
}
