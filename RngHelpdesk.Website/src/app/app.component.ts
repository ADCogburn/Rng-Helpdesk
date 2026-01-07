import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ApiService } from './api.service';
import { AuthService } from './auth.service';
import { switchMap } from 'rxjs';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './app.component.html'
})
export class AppComponent {

  user: any = null;
  error: string | null = null;

  constructor(
    private api: ApiService,
    private auth: AuthService
  ) {}

  getUser() {
  this.error = null;

  this.auth.ensureAuthenticated().pipe(
    switchMap(() => this.api.getUser(1))
    ).subscribe({
      next: user => {
        this.user = user;
      },
      error: err => {
        this.error = 'Unauthorized or failed user call';
        console.error(err);
      }
    });
  }
}