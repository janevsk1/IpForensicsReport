import { Component } from '@angular/core';
import { Router } from '@angular/router';

import { AuthService } from '../../core/auth/services/auth.service';

@Component({
  selector: 'app-reports',
  standalone: true,
  template: `
    <main>
      <h1>IP Forensics Reports</h1>

      <p>
        Login succeeded and the protected reports
        functionality will be implemented here.
      </p>

      <button
        type="button"
        (click)="logout()"
      >
        Sign out
      </button>
    </main>
  `
})
export class ReportsComponent {
  constructor(
    private readonly authService: AuthService,
    private readonly router: Router
  ) {
  }

  logout(): void {
    this.authService.logout();
    void this.router.navigate(['/login']);
  }
}