import { Component, OnInit, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { AuthService } from '../../core/auth/auth.service';
import { DashboardService } from '../../core/services/dashboard.service';
import { EmployeeDashboard, ManagerDashboard, RecentRequest } from '../../core/models/dashboard.model';
import { RoleType, getRoleLabel } from '../../core/models/role-type.enum';

@Component({
  selector: 'app-employee-summary',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './employee-summary.component.html',
  styleUrls: ['./employee-summary.component.scss']
})
export class DashboardComponent implements OnInit {
  dashboard = signal<EmployeeDashboard | ManagerDashboard | null>(null);
  isLoading = signal(true);
  errorMessage = signal('');

  // Computed properties
  isManager = computed(() => {
    const user = this.authService.user();
    return user?.role === RoleType.Manager || user?.role === RoleType.HR || user?.role === RoleType.Admin;
  });

  managerDashboard = computed(() => {
    const dash = this.dashboard();
    if (this.isManager() && dash && 'teamSummary' in dash) {
      return dash as ManagerDashboard;
    }
    return null;
  });

  constructor(
    public authService: AuthService,
    private dashboardService: DashboardService,
    private router: Router
  ) { }

  ngOnInit(): void {
    this.loadDashboard();
  }

  loadDashboard(): void {
    this.isLoading.set(true);
    this.errorMessage.set('');

    if (this.isManager()) {
      this.dashboardService.getManagerDashboard().subscribe({
        next: (data) => {
          this.dashboard.set(data);
          this.isLoading.set(false);
        },
        error: (error) => {
          console.error('Error loading dashboard:', error);
          this.errorMessage.set('Failed to load dashboard data');
          this.isLoading.set(false);
        }
      });
    } else {
      this.dashboardService.getEmployeeDashboard().subscribe({
        next: (data) => {
          this.dashboard.set(data);
          this.isLoading.set(false);
        },
        error: (error) => {
          console.error('Error loading dashboard:', error);
          this.errorMessage.set('Failed to load dashboard data');
          this.isLoading.set(false);
        }
      });
    }
  }

  getUserRole(): string {
    const user = this.authService.user();
    return user ? getRoleLabel(user.role) : '';
  }

  navigateTo(page: string): void {
    this.router.navigate([`/${page}`]);
  }

  getStatusClass(status: string): string {
    switch (status?.toLowerCase()) {
      case 'approved': return 'status-approved';
      case 'rejected': return 'status-rejected';
      case 'submitted': return 'status-submitted';
      case 'draft': return 'status-draft';
      default: return 'status-pending';
    }
  }

  formatDate(date: Date | string): string {
    return new Date(date).toLocaleDateString('en-US', {
      month: 'short',
      day: 'numeric',
      year: 'numeric'
    });
  }

  getChartBarWidth(value: number, max: number): string {
    if (max === 0) return '0%';
    return `${Math.min(100, (value / max) * 100)}%`;
  }

  getMaxTeamValue(): number {
    const manager = this.managerDashboard();
    if (!manager) return 0;
    return Math.max(
      ...manager.teamSummary.map(m => m.leaveDays),
      ...manager.teamSummary.map(m => m.overtimeHours),
      1
    );
  }

  getMaxTrendValue(): number {
    const manager = this.managerDashboard();
    if (!manager) return 0;
    return Math.max(
      ...manager.monthlyTrend.map(t => t.leaveRequests + t.overtimeRequests),
      1
    );
  }
}
