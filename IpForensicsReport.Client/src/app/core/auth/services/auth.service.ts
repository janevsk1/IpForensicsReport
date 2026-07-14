import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable, tap } from 'rxjs';
import { LoginRequest } from '../models/login-request.model';
import { LoginResponse } from '../models/login-response.model';
import { RegisterRequest } from '../models/register-request.model';
import { RegisterResponse } from '../models/register-response.model';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private readonly http = inject(HttpClient);

  private readonly accountApiUrl = '/api/account';

  login(request: LoginRequest): Observable<LoginResponse> {
    return this.http
      .post<LoginResponse>(
        `${this.accountApiUrl}/login`,
        request
      )
      .pipe(
        tap(response => {
          sessionStorage.setItem(
            'accessToken',
            response.accessToken
          );

          sessionStorage.setItem(
            'authenticatedUser',
            JSON.stringify(response.user)
          );

          sessionStorage.setItem(
            'tokenExpiresAtUtc',
            response.expiresAtUtc
          );
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
    sessionStorage.removeItem('accessToken');
    sessionStorage.removeItem('authenticatedUser');
    sessionStorage.removeItem('tokenExpiresAtUtc');
  }

  getAccessToken(): string | null {
    return sessionStorage.getItem('accessToken');
  }

  isAuthenticated(): boolean {
    const accessToken =
      sessionStorage.getItem('accessToken');

    const expiresAtUtc =
      sessionStorage.getItem('tokenExpiresAtUtc');

    if (!accessToken || !expiresAtUtc) {
      return false;
    }

    const expirationDate = new Date(expiresAtUtc);

    if (Number.isNaN(expirationDate.getTime())) {
      this.logout();
      return false;
    }

    if (expirationDate <= new Date()) {
      this.logout();
      return false;
    }

    return true;
  }
}