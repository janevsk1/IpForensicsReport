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
      /*
       * Only clear an existing authenticated session.
       * A failed login also returns 401, but no token exists then.
       */
      if (error.status === 401 && accessToken) {
        const returnUrl = router.url;

        authSession.clear();

        void router.navigate(['/login'], {
          queryParams: {
            returnUrl
          }
        });
      }

      return throwError(() => error);
    })
  );
};