import { Injectable } from '@angular/core';
import { ApiService } from './api.service';
import { Observable, of, map } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class AuthService {

  private _token: string | null = null;

  constructor(private api: ApiService) {}

  get token(): string | null {
    return this._token;
  }

  // TEMP: only for initial development
  loginDev(): Observable<void> {
    return this.api.getDevToken().pipe(
      map(response => {
        this._token = response.token;
        return void 0;
      })
    );
  }

  setToken(token: string) {
    this._token = token;
  }

  clear() {
    this._token = null;
  }

  ensureAuthenticated(): Observable<void> {
    return this.isAuthenticated
      ? of(void 0)
      : this.loginDev().pipe(map(() => void 0));
  }

  get isAuthenticated(): boolean {
    return !!this._token;
  }
}