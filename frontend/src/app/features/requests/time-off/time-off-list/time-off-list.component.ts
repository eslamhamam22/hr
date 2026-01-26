import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { TimeOffService } from '../../../../core/services/time-off.service';
import { TimeOffRequest } from '../../../../core/models/time-off.model';
import { RequestStatus, getStatusLabel, getStatusClass } from '../../../../core/models/overtime.model'; // Reusing status utils from overtime as they share same enum mostly
import { PaginatedResponse } from '../../../../core/models/department.model';
import { DataTableComponent, DataTableColumn, PaginationConfig } from '../../../../shared/components/data-table/data-table.component';
import { ConfirmDialogComponent } from '../../../../shared/components/confirm-dialog/confirm-dialog.component';
import { TimeOffModalComponent } from './time-off-modal/time-off-modal.component'; // Import modal
import { AuthService } from '../../../../core/auth/auth.service';
import { StatusColorPipe } from '@shared/pipes/status-color.pipe';

@Component({
    selector: 'app-time-off-list',
    standalone: true,
    imports: [CommonModule, DataTableComponent, ConfirmDialogComponent, StatusColorPipe, TimeOffModalComponent], 
    providers: [StatusColorPipe],
    templateUrl: './time-off-list.component.html',
    styles: [`
        .time-off-list-container { padding: 20px; }
        .page-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 24px; }
        .header-content h1 { font-size: 24px; font-weight: 600; color: #1f2937; margin: 0; }
        .subtitle { color: #6b7280; margin-top: 4px; font-size: 14px; }
        .btn-primary { background: #2563eb; color: white; border: none; padding: 10px 20px; border-radius: 6px; cursor: pointer; display: flex; align-items: center; gap: 8px; font-weight: 500; transition: background 0.2s; }
        .btn-primary:hover { background: #1d4ed8; }
        .status-badge { padding: 4px 12px; border-radius: 9999px; font-size: 12px; font-weight: 500; }
        .status-draft { background: #f3f4f6; color: #374151; }
        .status-pending { background: #fef3c7; color: #d97706; }
        .status-pending-hr { background: #fff7ed; color: #c2410c; }
        .status-approved { background: #d1fae5; color: #059669; }
        .status-rejected { background: #fee2e2; color: #dc2626; }
        .btn-icon { background: none; border: none; cursor: pointer; padding: 4px; border-radius: 4px; transition: background 0.2s; }
        .btn-icon:hover { background: #f3f4f6; }
        .action-buttons { display: flex; gap: 8px; }
    `]
})
export class TimeOffListComponent implements OnInit {
    timeOffRequests: TimeOffRequest[] = [];
    loading: boolean = false;

    columns: DataTableColumn[] = [
        { header: 'Employee', field: 'userName', sortable: true, width: '25%' },
        { header: 'Date', field: 'date', sortable: true, width: '20%' },
        { header: 'Start Time', field: 'startTime', sortable: true, width: '15%' },
        { header: 'Status', field: 'status', sortable: true, width: '20%' },
        { header: 'Submitted', field: 'submittedAt', sortable: true, width: '20%' }
    ];

    pagination: PaginationConfig = {
        currentPage: 1,
        pageSize: 10,
        totalItems: 0,
        pageSizeOptions: [10, 25, 50, 100]
    };

    isDeleteDialogOpen = false;
    requestToDelete: TimeOffRequest | null = null;

    // Modal state
    isModalOpen = false;

    constructor(
        private timeOffService: TimeOffService,
        private statusColorPipe: StatusColorPipe,
        private authService: AuthService
    ) { }

    ngOnInit(): void {
        this.loadTimeOffRequests();
    }

    loadTimeOffRequests(): void {
        this.loading = true;
        const userId = this.authService.user()?.id || '';

        this.timeOffService
            .getTimeOffRequests(this.pagination.currentPage, this.pagination.pageSize, undefined, userId)
            .subscribe({
                next: (response: PaginatedResponse<TimeOffRequest>) => {
                    this.timeOffRequests = response.items;
                    this.pagination.totalItems = response.totalCount;
                    this.loading = false;
                },
                error: (error) => {
                    console.error('Error loading time off requests:', error);
                    this.loading = false;
                }
            });
    }

    onPageChange(page: number): void {
        this.pagination.currentPage = page;
        this.loadTimeOffRequests();
    }

    onPageSizeChange(pageSize: number): void {
        this.pagination.pageSize = pageSize;
        this.pagination.currentPage = 1;
        this.loadTimeOffRequests();
    }

    createNewRequest(): void {
        this.isModalOpen = true;
    }

    onModalSaved(): void {
        this.isModalOpen = false;
        this.loadTimeOffRequests();
    }

    onModalCancelled(): void {
        this.isModalOpen = false;
    }

    deleteRequest(request: TimeOffRequest, event: Event): void {
        event.stopPropagation();
        this.requestToDelete = request;
        this.isDeleteDialogOpen = true;
    }

    onDeleteConfirmed(): void {
        if (this.requestToDelete) {
            this.timeOffService.deleteTimeOff(this.requestToDelete.id).subscribe({
                next: () => {
                    this.isDeleteDialogOpen = false;
                    this.requestToDelete = null;
                    this.loadTimeOffRequests();
                },
                error: (error) => {
                    console.error('Error deleting time off request:', error);
                    this.isDeleteDialogOpen = false;
                    this.requestToDelete = null;
                    alert('Failed to delete request. Please try again.');
                }
            });
        }
    }

    getRowStyle = (request: TimeOffRequest) => {
        return { 'background-color': this.statusColorPipe.transform(request.status.toString()) };
    };

    onDeleteCancelled(): void {
        this.isDeleteDialogOpen = false;
        this.requestToDelete = null;
    }

    getStatusLabel(status: RequestStatus): string {
        return getStatusLabel(status);
    }

    getStatusClass(status: RequestStatus): string {
        return getStatusClass(status);
    }

    formatDate(date: Date): string {
        return date ? new Date(date).toLocaleDateString() : '-';
    }
}
