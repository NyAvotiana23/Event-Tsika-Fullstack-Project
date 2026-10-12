import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { tap } from 'rxjs';
import { AuthService } from './auth.service';

/** Ajoute le jeton JWT aux appels API et déconnecte l'utilisateur si le jeton est refusé. */
export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const auth = inject(AuthService);
  const token = auth.session()?.token;
  const request = token ? req.clone({ setHeaders: { Authorization: `Bearer ${token}` } }) : req;
  return next(request).pipe(
    tap({
      error: (e) => {
        if (token && e?.status === 401) auth.logout();
      },
    }),
  );
};

export const authGuard: CanActivateFn = () => {
  const router = inject(Router);
  return inject(AuthService).isLoggedIn() || router.createUrlTree(['/connexion']);
};

export const organizerGuard: CanActivateFn = () => {
  const router = inject(Router);
  return inject(AuthService).isOrganizer() || router.createUrlTree(['/connexion']);
};
