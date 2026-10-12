import { Routes } from '@angular/router';
import { authGuard, organizerGuard } from './core/auth.http';

export const routes: Routes = [
  {
    path: '',
    title: 'E-Ventsika',
    loadComponent: () => import('./pages/home').then((m) => m.Home),
  },
  {
    path: 'evenements/:id',
    title: 'Événement',
    loadComponent: () => import('./pages/event-detail').then((m) => m.EventDetail),
  },
  {
    path: 'connexion',
    title: 'Connexion',
    data: { mode: 'login' },
    loadComponent: () => import('./pages/auth-form').then((m) => m.AuthForm),
  },
  {
    path: 'inscription',
    title: 'Inscription',
    data: { mode: 'register' },
    loadComponent: () => import('./pages/auth-form').then((m) => m.AuthForm),
  },
  {
    path: 'mes-billets',
    title: 'Mes billets',
    canActivate: [authGuard],
    loadComponent: () => import('./pages/my-tickets').then((m) => m.MyTickets),
  },
  {
    path: 'organisateur',
    title: 'Espace organisateur',
    canActivate: [organizerGuard],
    loadComponent: () => import('./pages/organizer-dashboard').then((m) => m.OrganizerDashboard),
  },
  {
    path: 'organisateur/evenements/nouveau',
    title: 'Nouvel événement',
    canActivate: [organizerGuard],
    loadComponent: () => import('./pages/event-form').then((m) => m.EventForm),
  },
  {
    path: 'organisateur/evenements/:id',
    title: "Modifier l'événement",
    canActivate: [organizerGuard],
    loadComponent: () => import('./pages/event-form').then((m) => m.EventForm),
  },
  { path: '**', redirectTo: '' },
];
