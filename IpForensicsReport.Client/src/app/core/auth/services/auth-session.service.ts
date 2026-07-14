import { computed, Injectable, signal } from '@angular/core';
import { AuthSession } from '../models/auth-session.model';

const AUTH_STORAGE_KEY = 'ip-forensics-auth-session';

@Injectable({
  providedIn: 'root'
})
export class AuthSessionService {
  private readonly sessionState = signal<AuthSession | null>(
    this.readStoredSession()
  );

  readonly session = this.sessionState.asReadonly();

  readonly currentUser = computed(() => this.sessionState()?.user ?? null);

  readonly isAuthenticated = computed(() => {
    const session = this.sessionState();

    return session !== null && !this.isExpired(session);
  });

  get accessToken(): string | null {
    const session = this.sessionState();

    if (!session || this.isExpired(session)) {
      return null;
    }

    return session.accessToken;
  }

  setSession(session: AuthSession): void {
    this.sessionState.set(session);

    if (typeof window !== 'undefined') {
      sessionStorage.setItem(AUTH_STORAGE_KEY, JSON.stringify(session));
    }
  }

  clear(): void {
    this.sessionState.set(null);

    if (typeof window !== 'undefined') {
      sessionStorage.removeItem(AUTH_STORAGE_KEY);
    }
  }

  private readStoredSession(): AuthSession | null {
    if (typeof window === 'undefined') {
      return null;
    }

    const storedValue = sessionStorage.getItem(AUTH_STORAGE_KEY);

    if (!storedValue) {
      return null;
    }

    try {
      const session = JSON.parse(storedValue) as AuthSession;

      if (
        !session.accessToken ||
        !session.expiresAtUtc ||
        !session.user ||
        this.isExpired(session)
      ) {
        sessionStorage.removeItem(AUTH_STORAGE_KEY);
        return null;
      }

      return session;
    } catch {
      sessionStorage.removeItem(AUTH_STORAGE_KEY);
      return null;
    }
  }

  private isExpired(session: AuthSession): boolean {
    const expiresAt = Date.parse(session.expiresAtUtc);

    return Number.isNaN(expiresAt) || expiresAt <= Date.now();
  }
}