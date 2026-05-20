import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, throwError } from 'rxjs';

import { AuthService } from './auth.service';

function isPublicAuthUrl(url: string): boolean {
  return url.includes('/api/auth/login') || url.includes('/api/auth/register');
}

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const auth = inject(AuthService);
  const router = inject(Router);
  const url = req.url;

  let outbound = req;
  if (!isPublicAuthUrl(url)) {
    const token = auth.token();
    if (token && !url.startsWith('http')) {
      outbound = req.clone({ setHeaders: { Authorization: `Bearer ${token}` } });
    }
  }

  return next(outbound).pipe(
    catchError((err: HttpErrorResponse) => {
      if (err.status === 401 && !isPublicAuthUrl(url)) {
        auth.logout();
        void router.navigateByUrl('/login');
      }

      return throwError(() => err);
    }),
  );
};
