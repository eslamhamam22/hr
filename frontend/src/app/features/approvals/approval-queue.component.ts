import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RequestService } from '../../core/services/request.service';
import { Request } from '../../core/models/request.model';
import { DataTableColumn, DataTableComponent } from '../../shared/components/data-table/data-table.component';

@Component({
  selector: 'app-approval-queue',
  standalone: true,
  imports: [CommonModule, DataTableComponent],
  templateUrl: './approval-queue.component.html',
})
export class ApprovalQueueComponent implements OnInit {
  pendingRequests = signal<Request[]>([]);
  isLoading = signal(false);
  errorMessage = signal('');

  // Columns definition for the data table
  columns: DataTableColumn[] = [
    { header: 'Employee', field: 'employeeName', sortable: true },
    { header: 'Type', field: 'requestType', sortable: true },
    { header: 'Period', field: 'period', sortable: false },
    { header: 'Submitted', field: 'submittedAt', sortable: true },
  ];

  constructor(private requestService: RequestService) { }

  ngOnInit(): void {
    this.loadPendingApprovals();
  }

  loadPendingApprovals(): void {
    this.isLoading.set(true);
    this.errorMessage.set('');
    this.requestService.getPendingApprovals().subscribe({
      next: (data) => {
        this.pendingRequests.set(data);
        this.isLoading.set(false);
      },
      error: () => {
        this.errorMessage.set('Failed to load pending approvals');
        this.isLoading.set(false);
      }
    });
  }

  approveRequest(requestId: string): void {
    this.requestService.approveRequest(requestId).subscribe({
      next: () => this.loadPendingApprovals(),
      error: () => this.errorMessage.set('Failed to approve request')
    });
  }

  rejectRequest(requestId: string): void {
    const reason = prompt('Enter rejection reason:');
    if (reason) {
      this.requestService.rejectRequest(requestId, reason).subscribe({
        next: () => this.loadPendingApprovals(),
        error: () => this.errorMessage.set('Failed to reject request')
      });
    }
  }
}
