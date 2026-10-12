import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { HttpErrorResponse, provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { ApiService, errorMessage } from './api.service';
import { AuthService } from './auth.service';
import { toLocalInput } from '../pages/event-form';

describe('errorMessage', () => {
  it('prefers the first validation error', () => {
    const e = new HttpErrorResponse({
      status: 400,
      error: { title: 'x', errors: { Email: ['Email invalide'] } },
    });
    expect(errorMessage(e)).toBe('Email invalide');
  });

  it('falls back to the problem title', () => {
    expect(errorMessage(new HttpErrorResponse({ status: 409, error: { title: 'Complet' } }))).toBe(
      'Complet',
    );
  });

  it('reports an unreachable server', () => {
    expect(errorMessage(new HttpErrorResponse({ status: 0 }))).toContain('injoignable');
  });
});

describe('AuthService', () => {
  let http: HttpTestingController;

  beforeEach(() => {
    localStorage.clear();
    TestBed.configureTestingModule({
      providers: [provideRouter([]), provideHttpClient(), provideHttpClientTesting()],
    });
    http = TestBed.inject(HttpTestingController);
  });

  it('stores the session after login and clears it on logout', () => {
    const auth = TestBed.inject(AuthService);
    auth.login({ email: 'a@b.mg', password: 'motdepasse' }).subscribe();
    http
      .expectOne('/api/auth/login')
      .flush({ token: 't', userId: 'u', displayName: 'A', role: 'Organizer' });

    expect(auth.isOrganizer()).toBe(true);
    expect(localStorage.getItem('event-tsika.session')).toContain('"token":"t"');

    auth.logout();
    expect(auth.isLoggedIn()).toBe(false);
    expect(localStorage.getItem('event-tsika.session')).toBeNull();
  });
});

describe('ApiService', () => {
  it('sends only the non-empty filters', () => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()],
    });
    const http = TestBed.inject(HttpTestingController);
    TestBed.inject(ApiService)
      .listEvents({ q: 'jazz', category: '', city: 'Toamasina' })
      .subscribe();

    const req = http.expectOne((r) => r.url === '/api/events');
    expect(req.request.params.get('q')).toBe('jazz');
    expect(req.request.params.has('category')).toBe(false);
    expect(req.request.params.get('city')).toBe('Toamasina');
  });
});

describe('toLocalInput', () => {
  it('formats an ISO date for datetime-local inputs', () => {
    expect(toLocalInput(new Date(2030, 0, 5, 9, 7).toISOString())).toBe('2030-01-05T09:07');
  });
});
