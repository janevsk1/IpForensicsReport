import { Routes } from '@angular/router';
import { authGuard } from './core/auth/services/auth.guard';

export const routes: Routes = [
  {
    path: '',
    pathMatch: 'full',
    redirectTo: 'login'
  },
  {
    path: 'login',
    loadComponent: () =>
      import(
        './features/login/login.component'
      ).then(component => component.LoginComponent)
  },
  {
    path: 'register',
    loadComponent: () =>
      import(
        './features/register/register.component'
      ).then(component => component.RegisterComponent)
  },
  {
    path: 'reports/:id',
    canActivate: [authGuard],
    loadComponent: () =>
      import(
        './features/reports/report-details/report-details.component'
      ).then(
        component => component.ReportDetailsComponent
      )
  },
  {
    path: 'reports',
    canActivate: [authGuard],
    loadComponent: () =>
      import(
        './features/reports/reports.component'
      ).then(component => component.ReportsComponent)
  },
  {
    path: '**',
    redirectTo: 'login'
  }
];