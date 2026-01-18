import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { ApprovalLogService } from '../../../../core/services/approval-log.service';
import { ApprovalLog } from '../../../../core/models/approval-log.model';
import { PaginatedResponse } from '../../../../core/models/department.model';
import { DataTableComponent, DataTableColumn, PaginationConfig } from '../../../../shared/components/data-table/data-table.component';

@Component({
  selector: 'app-approval-logs-list',
  standalone: true,
  imports: [CommonModule, DataTableComponent],
  templateUrl: './approval-logs-list.component.html',
  styleUrls: ['./approval-logs-list.component.scss']
})
export class ApprovalLogsListComponent implements OnInit {
  approvalLogs: ApprovalLog[] = [];
  loading: boolean = false;
  searchTerm: string = '';

  columns: DataTableColumn[] = [
    { header: 'Request Type', field: 'requestType', sortable: true, width: '15%' },
    { header: 'Approver', field: 'approvedByUserName', sortable: true, width: '20%' },
    { header: 'Decision', field: 'approved', sortable: true, width: '12%' },
    { header: 'Override', field: 'isOverride', width: '12%' },
    { header: 'Comments', field: 'comments', width: '26%' },
    { header: 'Date', field: 'createdAt', sortable: true, width: '15%' }
  ];

  pagination: PaginationConfig = {
    currentPage: 1,
    pageSize: 10,
    totalItems: 0
  };

  constructor(
    private approvalLogService: ApprovalLogService,
    private router: Router
  ) { }

  ngOnInit(): void {
    this.loadApprovalLogs();
  }

  loadApprovalLogs(): void {
    this.loading = true;
    this.approvalLogService
      .getApprovalLogs(this.pagination.currentPage, this.pagination.pageSize)
      .subscribe({
        next: (response: PaginatedResponse<ApprovalLog>) => {
          this.approvalLogs = response.items;
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
    this.loadApprovalLogs();
  }

  onPageSizeChange(pageSize: number): void {
    this.pagination.pageSize = pageSize;
    this.pagination.currentPage = 1;
    this.loadApprovalLogs();
  }

  onSearch(searchTerm: string): void {
    this.searchTerm = searchTerm;
    this.pagination.currentPage = 1;
    this.loadApprovalLogs();
  }

  onRowClick(log: ApprovalLog): void {
    this.router.navigate(['/admin/approval-logs', log.id]);
  }

  formatDateTime(date: Date): string {
    return date ? new Date(date).toLocaleString() : '-';
  }
}
