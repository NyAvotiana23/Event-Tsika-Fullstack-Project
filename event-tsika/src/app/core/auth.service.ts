import { Injectable, PLATFORM_ID, computed, inject, signal } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { tap } from 'rxjs';
import { Session } from './models';

const STORAGE_KEY = 'event-tsika.session';

interface Credentials {
  email: string;
  password: string;
}

export interface Registration extends Credentials {
  displayName: string;
  isOrganizer: boolean;
}

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly router = inject(Router);
  private readonly browser = isPlatformBrowser(inject(PLATFORM_ID));

  readonly session = signal<Session | null>(this.restore());
  readonly isLoggedIn = computed(() => this.session() !== null);
  readonly isOrganizer = computed(() => this.session()?.role === 'Organizer');

  login(credentials: Credentials) {
    return this.http.post<Session>('/api/auth/login', credentials).pipe(tap((s) => this.store(s)));
  }

  register(registration: Registration) {
    return this.http
      .post<Session>('/api/auth/register', registration)
      .pipe(tap((s) => this.store(s)));
  }

  logout(): void {
    this.session.set(null);
    if (this.browser) localStorage.removeItem(STORAGE_KEY);
    void this.router.navigateByUrl('/');
  }

  private store(session: Session): void {
    this.session.set(session);
    if (this.browser) localStorage.setItem(STORAGE_KEY, JSON.stringify(session));
  }

  private restore(): Session | null {
    if (!this.browser) return null;
    try {
      return JSON.parse(localStorage.getItem(STORAGE_KEY) ?? 'null') as Session | null;
    } catch {
      return null;
    }
  }
}
