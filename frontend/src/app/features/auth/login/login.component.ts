import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../../core/auth/auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.scss']
})
export class LoginComponent {
  username = '';
  password = '';

  constructor(
    public authService: AuthService,
    private router: Router
  ) {}

  async onLogin(): Promise<void> {
    const success = await this.authService.login(this.username, this.password);
    if (success) {
      this.router.navigate(['/dashboard']);
    }
  }
}
