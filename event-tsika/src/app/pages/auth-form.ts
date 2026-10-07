import { Component, computed, inject, input, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { errorMessage } from '../core/api.service';
import { AuthService } from '../core/auth.service';

/** Formulaire partagé connexion / inscription ; le mode vient de `data.mode` de la route. */
@Component({
  selector: 'app-auth-form',
  imports: [FormsModule, RouterLink],
  template: `
    <section class="panel narrow">
      <h1>{{ isRegister() ? 'Créer un compte' : 'Connexion' }}</h1>
      <form (ngSubmit)="submit()">
        @if (isRegister()) {
          <label
            >Nom affiché
            <input name="displayName" [(ngModel)]="displayName" required maxlength="100"
          /></label>
        }
        <label
          >Email <input type="email" name="email" [(ngModel)]="email" required autocomplete="email"
        /></label>
        <label>
          Mot de passe
          <input
            type="password"
            name="password"
            [(ngModel)]="password"
            required
            minlength="8"
            [autocomplete]="isRegister() ? 'new-password' : 'current-password'"
          />
        </label>
        @if (isRegister()) {
          <label class="check">
            <input type="checkbox" name="isOrganizer" [(ngModel)]="isOrganizer" />
            Je suis organisateur d'événements
          </label>
        }
        @if (error()) {
          <p class="error" role="alert">{{ error() }}</p>
        }
        <button type="submit" [disabled]="busy()">
          {{ isRegister() ? "S'inscrire" : 'Se connecter' }}
        </button>
      </form>
      <p class="muted">
        @if (isRegister()) {
          Déjà inscrit ? <a routerLink="/connexion">Connexion</a>
        } @else {
          Pas de compte ? <a routerLink="/inscription">Inscription</a>
        }
      </p>
    </section>
  `,
})
export class AuthForm {
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);

  readonly mode = input<'login' | 'register'>('login');
  protected readonly isRegister = computed(() => this.mode() === 'register');

  protected displayName = '';
  protected email = '';
  protected password = '';
  protected isOrganizer = false;
  protected readonly error = signal('');
  protected readonly busy = signal(false);

  protected submit(): void {
    this.busy.set(true);
    this.error.set('');
    const request = this.isRegister()
      ? this.auth.register({
          displayName: this.displayName,
          email: this.email,
          password: this.password,
          isOrganizer: this.isOrganizer,
        })
      : this.auth.login({ email: this.email, password: this.password });
    request.subscribe({
      next: (session) =>
        void this.router.navigateByUrl(session.role === 'Organizer' ? '/organisateur' : '/'),
      error: (e) => {
        this.error.set(errorMessage(e));
        this.busy.set(false);
      },
    });
  }
}
