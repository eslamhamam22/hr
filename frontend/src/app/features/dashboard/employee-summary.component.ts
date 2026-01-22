import { Component, OnInit, signal, computed, AfterViewInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { AuthService } from '../../core/auth/auth.service';
import { DashboardService } from '../../core/services/dashboard.service';
import { EmployeeDashboard, ManagerDashboard, AdminDashboard } from '../../core/models/dashboard.model';
import { RoleType, RoleTypeLabels, getRoleLabel } from '../../core/models/role-type.enum';
import { Chart, registerables } from 'chart.js';

Chart.register(...registerables);

@Component({
  selector: 'app-employee-summary',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './employee-summary.component.html',
  styleUrls: ['./employee-summary.component.scss']
})
export class DashboardComponent implements OnInit, AfterViewInit, OnDestroy {
  dashboard = signal<EmployeeDashboard | ManagerDashboard | AdminDashboard | null>(null);
  isLoading = signal(true);
  errorMessage = signal('');
  
  private charts: Chart[] = [];

  // Computed properties
  isAdmin = computed(() => {
    debugger;
    const user = this.authService.user();
    var userRole = user?.role.toString();
    var roleAdmin = RoleTypeLabels[RoleType.Admin];
    var roleHR = RoleTypeLabels[RoleType.HR];

    return userRole == roleHR || userRole == roleAdmin;
  });

  isManager = computed(() => {
    const user = this.authService.user();
    return user?.role.toString() == RoleTypeLabels[RoleType.Manager];
  });

  managerDashboard = computed(() => {
    const dash = this.dashboard();
    if (this.isManager() && dash && 'teamSummary' in dash) {
      return dash as ManagerDashboard;
    }
    return null;
  });

  adminDashboard = computed(() => {
      const dash = this.dashboard();
      if (this.isAdmin() && dash && 'systemLeaveStats' in dash) {
          return dash as AdminDashboard;
      }
      return null;
  });

  constructor(
    public authService: AuthService,
    private dashboardService: DashboardService,
    private router: Router
  ) { }

  ngOnInit(): void {
    debugger;
    this.loadDashboard();
  }
  
  ngAfterViewInit(): void {
      // Charts are initialized after data load
  }
  
  ngOnDestroy(): void {
      this.destroyCharts();
  }

  loadDashboard(): void {
    debugger;
    this.isLoading.set(true);
    this.errorMessage.set('');

    const userRole = this.authService.user()?.role.toString();

    let request;
    if (userRole == RoleTypeLabels[RoleType.Admin] || userRole == RoleTypeLabels[RoleType.HR]) {
        request = this.dashboardService.getAdminDashboard();
    } else if (userRole == RoleTypeLabels[RoleType.Manager]) {
        request = this.dashboardService.getManagerDashboard();
    } else {
        request = this.dashboardService.getEmployeeDashboard();
    }

    request.subscribe({
        next: (data) => {
          this.dashboard.set(data);
          this.isLoading.set(false);
          // Small delay to allow DOM to update
          setTimeout(() => this.initCharts(), 100);
        },
        error: (error) => {
          console.error('Error loading dashboard:', error);
          this.errorMessage.set('Failed to load dashboard data');
          this.isLoading.set(false);
        }
    });
  }
  
  destroyCharts(): void {
      this.charts.forEach(c => c.destroy());
      this.charts = [];
  }
  
  initCharts(): void {
      this.destroyCharts();
      
      if (this.isManager()) {
          this.initManagerCharts();
      } else if (this.isAdmin()) {
          this.initAdminCharts();
      }
  }
  
  initManagerCharts(): void {
      const dash = this.managerDashboard();
      if (!dash) return;
      
      // Team Requests by Type (Pie/Doughnut)
      const typeCtx = document.getElementById('teamTypeChart') as HTMLCanvasElement;
      if (typeCtx) {
          this.charts.push(new Chart(typeCtx, {
              type: 'doughnut',
              data: {
                  labels: dash.teamLeaveByType.map(t => t.leaveType),
                  datasets: [{
                      data: dash.teamLeaveByType.map(t => t.count),
                      backgroundColor: ['#4e73df', '#1cc88a', '#36b9cc', '#f6c23e', '#e74a3b']
                  }]
              },
              options: {
                  responsive: true,
                  maintainAspectRatio: false,
                  plugins: {
                      legend: { position: 'right' }
                  }
              }
          }));
      }
      
      // Monthly Trend (Line/Bar)
      const trendCtx = document.getElementById('teamTrendChart') as HTMLCanvasElement;
      if (trendCtx) {
          this.charts.push(new Chart(trendCtx, {
              type: 'bar',
              data: {
                  labels: dash.monthlyTrend.map(t => t.month).reverse(),
                  datasets: [
                      {
                          label: 'Leave Requests',
                          data: dash.monthlyTrend.map(t => t.leaveRequests).reverse(),
                          backgroundColor: '#4e73df'
                      },
                      {
                          label: 'Overtime Requests',
                          data: dash.monthlyTrend.map(t => t.overtimeRequests).reverse(),
                          backgroundColor: '#1cc88a'
                      }
                  ]
              },
              options: {
                  responsive: true,
                  maintainAspectRatio: false,
                  scales: {
                      y: { beginAtZero: true, ticks: { precision: 0 } }
                  }
              }
          }));
      }
  }
  
  initAdminCharts(): void {
      const dash = this.adminDashboard();
      if (!dash) return;
      
      // Requests by Department (Bar)
      const deptCtx = document.getElementById('deptChart') as HTMLCanvasElement;
      if (deptCtx) {
          this.charts.push(new Chart(deptCtx, {
              type: 'bar',
              data: {
                  labels: dash.requestsByDepartment.map(d => d.departmentName),
                  datasets: [{
                      label: 'Total Requests',
                      data: dash.requestsByDepartment.map(d => d.totalRequests),
                      backgroundColor: '#36b9cc'
                  }]
              },
              options: {
                  responsive: true,
                  maintainAspectRatio: false,
                  scales: {
                      y: { beginAtZero: true, ticks: { precision: 0 } }
                  }
              }
          }));
      }
      
      // System Trend (Line)
      const sysTrendCtx = document.getElementById('sysTrendChart') as HTMLCanvasElement;
      if (sysTrendCtx) {
          this.charts.push(new Chart(sysTrendCtx, {
              type: 'line',
              data: {
                  labels: dash.systemMonthlyTrend.map(t => t.month).reverse(),
                  datasets: [
                      {
                          label: 'Leave',
                          data: dash.systemMonthlyTrend.map(t => t.leaveRequests).reverse(),
                          borderColor: '#4e73df',
                          tension: 0.1
                      },
                      {
                          label: 'Overtime',
                          data: dash.systemMonthlyTrend.map(t => t.overtimeRequests).reverse(),
                          borderColor: '#1cc88a',
                          tension: 0.1
                      }
                  ]
              },
              options: {
                  responsive: true,
                  maintainAspectRatio: false
              }
          }));
      }
      
      // Leave Type Distribution (Pie)
      const distCtx = document.getElementById('distChart') as HTMLCanvasElement;
      if (distCtx) {
          this.charts.push(new Chart(distCtx, {
              type: 'pie',
              data: {
                  labels: dash.systemLeaveTypeDistribution.map(d => d.leaveType),
                  datasets: [{
                      data: dash.systemLeaveTypeDistribution.map(d => d.count),
                      backgroundColor: ['#4e73df', '#1cc88a', '#36b9cc', '#f6c23e', '#e74a3b', '#858796']
                  }]
              },
              options: {
                  responsive: true,
                  maintainAspectRatio: false,
                  plugins: {
                      legend: { position: 'right' }
                  }
              }
          }));
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
}