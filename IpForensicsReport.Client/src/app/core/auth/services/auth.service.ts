import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable, tap } from 'rxjs';

import { LoginRequest } from '../models/login-request.model';
import { LoginResponse } from '../models/login-response.model';
import { RegisterRequest } from '../models/register-request.model';
import { RegisterResponse } from '../models/register-response.model';
import { AuthSessionService } from './auth-session.service';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private readonly http = inject(HttpClient);

  private readonly authSessionService =
    inject(AuthSessionService);

  private readonly accountApiUrl = '/api/account';

  login(
    request: LoginRequest
  ): Observable<LoginResponse> {
    return this.http
      .post<LoginResponse>(
        `${this.accountApiUrl}/login`,
        request
      )
      .pipe(
        tap(response => {
          debugger;
          this.authSessionService.setSession({
            accessToken: response.accessToken,
            expiresAtUtc: response.expiresAtUtc,
            user: response.user
          });
        })
      );
  }

  register(
    request: RegisterRequest
  ): Observable<RegisterResponse> {
    return this.http.post<RegisterResponse>(
      `${this.accountApiUrl}/register`,
      request
    );
  }

  logout(): void {
    this.authSessionService.clear();
  }
}