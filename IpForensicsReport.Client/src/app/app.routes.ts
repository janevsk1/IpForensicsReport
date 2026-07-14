import { Routes } from '@angular/router';

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
    path: 'reports',
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