import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterOutlet } from '@angular/router';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule, RouterOutlet],
  templateUrl: './app.component.html',
  styles: [`
    .app-container {
      min-height: 100vh;
      background: #f5f5f5;
    }
  `]
})
export class AppComponent {
  title = 'HR System';
}
