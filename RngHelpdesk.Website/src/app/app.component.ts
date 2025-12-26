import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ApiService } from './api.service';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './app.component.html'
})
export class AppComponent {

  token: string | null = null;
  user: any = null;
  error: string | null = null;

  constructor(private api: ApiService) {}

  getUser() {
    this.error = null;

    this.api.getDevToken().subscribe({
      next: auth => {
        this.token = auth.token;

        this.api.getUser(1, auth.token).subscribe({
          next: user => {
            this.user = user;
          },
          error: err => {
            this.error = 'Unauthorized or failed user call';
            console.error(err);
          }
        });
      },
      error: err => {
        this.error = 'Failed to get token';
        console.error(err);
      }
    });
  }
}