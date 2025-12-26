import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class ApiService {

  private readonly baseUrl = 'http://localhost:5000';

  constructor(private http: HttpClient) {}

  /** DEV ONLY */
  getDevToken(): Observable<{ token: string }> {
    return this.http.post<{ token: string }>(
      `${this.baseUrl}/dev/auth/token`,
      {}
    );
  }

  getUser(userId: number, token: string): Observable<any> {
    const headers = new HttpHeaders({
      Authorization: `Bearer ${token}`
    });

    return this.http.get(
      `${this.baseUrl}/users/${userId}`,
      { headers }
    );
  }
}
