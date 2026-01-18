import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { AttendanceService } from '../../../../core/services/attendance.service';
import { AttendanceLog } from '../../../../core/models/attendance.model';
import { PaginatedResponse } from '../../../../core/models/department.model';
import { DataTableComponent, DataTableColumn, PaginationConfig } from '../../../../shared/components/data-table/data-table.component';

@Component({
  selector: 'app-attendance-list',
  standalone: true,
  imports: [CommonModule, DataTableComponent],
  templateUrl: './attendance-list.component.html',
  styleUrls: ['./attendance-list.component.scss']
})
export class AttendanceListComponent implements OnInit {
  attendanceLogs: AttendanceLog[] = [];
  loading: boolean = false;
  searchTerm: string = '';

  columns: DataTableColumn[] = [
    { header: 'Employee', field: 'userName', sortable: true, width: '25%' },
    { header: 'Date', field: 'date', sortable: true, width: '15%' },
    { header: 'Check In', field: 'checkInTime', width: '15%' },
    { header: 'Check Out', field: 'checkOutTime', width: '15%' },
    { header: 'Hours', field: 'hoursWorked', sortable: true, width: '10%' },
    { header: 'On Time', field: 'isLate', width: '10%' },
    { header: 'Status', field: 'isAbsent', width: '10%' }
  ];

  pagination: PaginationConfig = {
    currentPage: 1,
    pageSize: 10,
    totalItems: 0
  };

  constructor(
    private attendanceService: AttendanceService,
    private router: Router
  ) { }

  ngOnInit(): void {
    this.loadAttendanceLogs();
  }

  loadAttendanceLogs(): void {
    this.loading = true;
    this.attendanceService
      .getAttendanceLogs(this.pagination.currentPage, this.pagination.pageSize)
      .subscribe({
        next: (response: PaginatedResponse<AttendanceLog>) => {
          this.attendanceLogs = response.items;
          this.pagination.totalItems = response.totalCount;
          this.loading = false;
        },
        error: () => {
          this.loading = false;
        }
      });
  }

  onPageChange(page: number): void {
    this.pagination.currentPage = page;
    this.loadAttendanceLogs();
  }

  onPageSizeChange(pageSize: number): void {
    this.pagination.pageSize = pageSize;
    this.pagination.currentPage = 1;
    this.loadAttendanceLogs();
  }

  onSearch(searchTerm: string): void {
    this.searchTerm = searchTerm;
    this.pagination.currentPage = 1;
    this.loadAttendanceLogs();
  }

  onRowClick(log: AttendanceLog): void {
    this.router.navigate(['/admin/attendance', log.id]);
  }

  formatDate(date: Date): string {
    return date ? new Date(date).toLocaleDateString() : '-';
  }
}
