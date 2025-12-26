import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class ApiService {

  private readonly baseUrl = 'http://localhost:5000';

  constructor(private http: HttpClient) {}

  // --------------------
  // TEMP: Auth (dev only)
  // --------------------

  getDevToken(): Observable<{ token: string }> {
    return this.http.post<{ token: string }>(
      `${this.baseUrl}/dev/auth/token`,
      {}
    );
  }

  // --------------------
  // Users
  // --------------------

  getUser(userId: number): Observable<any> {
    return this.http.get<any>(
      `${this.baseUrl}/users/${userId}`
    );
  }
}
