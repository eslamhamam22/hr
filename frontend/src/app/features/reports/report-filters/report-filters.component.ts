import { Component, Input, Output, EventEmitter, OnInit, signal, computed, effect } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../../../core/auth/auth.service';
import { ApiService } from '../../../core/services/api.service';
import { RoleType } from '../../../core/models/role-type.enum';

@Component({
  selector: 'app-report-filters',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './report-filters.component.html',
  styleUrls: ['./report-filters.component.scss']
})
export class ReportFiltersComponent implements OnInit {
  @Input() reportType: string = '';
  @Output() filterApplied = new EventEmitter<any>();

  startDate = '';
  endDate = '';
  departmentId = '';
  employeeId = '';

  departments = signal<any[]>([]);
  employees = signal<any[]>([]);
  
  // Computed role checks
  canFilterDepartment = computed(() => {
    return this.authService.hasRole('Admin') || this.authService.hasRole('HR');
  });

  canFilterEmployee = computed(() => {
    return this.authService.hasRole('Admin') || this.authService.hasRole('HR') || this.authService.hasRole('Manager');
  });

  constructor(
    private authService: AuthService,
    private apiService: ApiService
  ) {
      // Effect to reload employees when department changes if needed, 
      // but simpler to just trigger on change event
  }

  ngOnInit(): void {
    this.setDefaultDates();
    this.loadDepartments();
    this.loadEmployees();
  }

  setDefaultDates(): void {
    const today = new Date();
    const firstDay = new Date(today.getFullYear(), today.getMonth(), 1);
    
    this.startDate = firstDay.toISOString().split('T')[0];
    this.endDate = today.toISOString().split('T')[0];
    
    // Emit initial default filters
    // this.applyFilters(); 
  }

  loadDepartments(): void {
    if (!this.canFilterDepartment()) return;

    // Fetching with large page size to get all departments for dropdown
    this.apiService.get<any>('/departments', { page: 1, pageSize: 100 }).subscribe({
      next: (res) => {
        this.departments.set(res.items || []);
      },
      error: (err) => console.error('Failed to load departments', err)
    });
  }

  loadEmployees(): void {
    if (!this.canFilterEmployee()) return;

    const params: any = { page: 1, pageSize: 1000 };
    if (this.departmentId) {
        params.departmentId = this.departmentId;
    }

    // If manager, backend might filter automatically based on their team, 
    // or we might need to be careful. Currently getting all users.
    // Ideally we should have an endpoint /users/my-team for managers,
    // or rely on the backend to filter /users if the caller is a manager.
    // For now, let's fetch users.
    this.apiService.get<any>('/users', params).subscribe({
      next: (res) => {
        this.employees.set(res.items || []);
      },
      error: (err) => console.error('Failed to load employees', err)
    });
  }

  onDepartmentChange(): void {
    this.employeeId = ''; // Reset employee selection
    this.loadEmployees(); // Reload employees for selected department
    this.applyFilters();
  }

  applyFilters(): void {
    const filters = {
      reportType: this.reportType,
      startDate: this.startDate,
      endDate: this.endDate,
      departmentId: this.departmentId || null,
      employeeId: this.employeeId || null
    };
    this.filterApplied.emit(filters);
  }

  clearFilters(): void {
    this.setDefaultDates();
    this.departmentId = '';
    this.employeeId = '';
    this.onDepartmentChange(); // Reloads full employee list if was filtered
  }
}