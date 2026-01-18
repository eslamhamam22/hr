import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AuthService } from '../../core/auth/auth.service';

@Component({
  selector: 'app-employee-summary',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './employee-summary.component.html',
  styleUrls: ['./employee-summary.component.scss']
})
export class DashboardComponent implements OnInit {
  userDepartment = signal('Engineering');
  leaveBalance = signal(15);

  constructor(public authService: AuthService) {}

  ngOnInit(): void {
    // Load dashboard data
  }

  navigateTo(page: string): void {
    // Navigation logic
    console.log('Navigate to:', page);
  }
}
