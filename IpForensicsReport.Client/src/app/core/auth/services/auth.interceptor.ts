import {
  HttpErrorResponse,
  HttpInterceptorFn
} from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, throwError } from 'rxjs';
import { AuthSessionService } from './auth-session.service';

export const authInterceptor: HttpInterceptorFn = (request, next) => {
  const authSession = inject(AuthSessionService);
  const router = inject(Router);

  const hadSession = authSession.session() !== null;
  const accessToken = authSession.accessToken;
  const isApiRequest = request.url.startsWith('/api/');

  const authenticatedRequest =
    accessToken && isApiRequest
      ? request.clone({
          setHeaders: {
            Authorization: `Bearer ${accessToken}`
          }
        })
      : request;

  return next(authenticatedRequest).pipe(
    catchError((error: HttpErrorResponse) => {
      if (
        isApiRequest &&
        error.status === 401 &&
        hadSession
      ) {
        const returnUrl = router.url;

        authSession.clear();

        void router.navigate(['/login'], {
          queryParams: {
            returnUrl,
            reason: 'session-expired'
          }
        });
      }

      return throwError(() => error);
    })
  );
};