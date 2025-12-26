import { inject } from '@angular/core';
import {
  HttpErrorResponse,
  HttpHandlerFn,
  HttpRequest
} from '@angular/common/http';
import { catchError, throwError } from 'rxjs';
import { AuthService } from './auth.service';

export function authInterceptor(
  req: HttpRequest<any>,
  next: HttpHandlerFn
) {
  const auth = inject(AuthService);
  const token = auth.token;

  const authReq = token
    ? req.clone({
        setHeaders: {
          Authorization: `Bearer ${token}`
        }
      })
    : req;

  return next(authReq).pipe(
    catchError((error: HttpErrorResponse) => {
      if (error.status === 401) {
        auth.clear();
      }
      return throwError(() => error);
    })
  );
}