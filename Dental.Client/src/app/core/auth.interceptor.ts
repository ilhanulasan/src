import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';

import { AuthService } from './auth.service';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const url = req.url;
  if (
    url.includes('/api/auth/login') ||
    url.includes('/api/auth/register')
  ) {
    return next(req);
  }

  const auth = inject(AuthService);
  const token = auth.token();
  if (!token || url.startsWith('http')) {
    return next(req);
  }

  return next(req.clone({ setHeaders: { Authorization: `Bearer ${token}` } }));
};
