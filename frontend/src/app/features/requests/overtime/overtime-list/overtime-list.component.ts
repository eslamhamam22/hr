import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { OvertimeService } from '../../../../core/services/overtime.service';
import { OvertimeRequest, RequestStatus, getStatusLabel, getStatusClass } from '../../../../core/models/overtime.model';
import { PaginatedResponse } from '../../../../core/models/department.model';
import { DataTableComponent, DataTableColumn, PaginationConfig } from '../../../../shared/components/data-table/data-table.component';
import { ConfirmDialogComponent } from '../../../../shared/components/confirm-dialog/confirm-dialog.component';
import { OvertimeModalComponent } from './overtime-modal/overtime-modal.component';
import { AuthService } from '@core/auth/auth.service';

@Component({
    selector: 'app-overtime-list',
    standalone: true,
    imports: [CommonModule, DataTableComponent, ConfirmDialogComponent, OvertimeModalComponent],
    templateUrl: './overtime-list.component.html',
    styleUrls: ['./overtime-list.component.scss']
})
export class OvertimeListComponent implements OnInit {
    overtimeRequests: OvertimeRequest[] = [];
    loading: boolean = false;
    searchTerm: string = '';
    statusFilter: string = '';

    columns: DataTableColumn[] = [
        { header: 'Employee', field: 'userName', sortable: true, width: '20%' },
        { header: 'Start Date', field: 'startDateTime', sortable: true, width: '17%' },
        { header: 'End Date', field: 'endDateTime', sortable: true, width: '17%' },
        { header: 'Hours', field: 'hoursWorked', sortable: true, width: '10%' },
        { header: 'Status', field: 'status', sortable: true, width: '15%' },
        { header: 'Submitted', field: 'submittedAt', sortable: true, width: '15%' }
    ];

    pagination: PaginationConfig = {
        currentPage: 1,
        pageSize: 10,
        totalItems: 0,
        pageSizeOptions: [10, 25, 50, 100]
    };

    // Modal state
    isModalOpen = false;

    // Delete confirmation dialog state
    isDeleteDialogOpen = false;
    requestToDelete: OvertimeRequest | null = null;

    constructor(
        private overtimeService: OvertimeService,
        private router: Router,
        private authService: AuthService
    ) { }

    ngOnInit(): void {
        this.loadOvertimeRequests();
    }

    loadOvertimeRequests(): void {
        this.loading = true;
        const userId = this.authService.user()?.id || '';

        this.overtimeService
            .getOvertimeRequests(this.pagination.currentPage, this.pagination.pageSize, this.statusFilter, this.searchTerm, userId)
            .subscribe({
                next: (response: PaginatedResponse<OvertimeRequest>) => {
                    this.overtimeRequests = response.items;
                    this.pagination.totalItems = response.totalCount;
                    this.loading = false;
                },
                error: (error) => {
                    console.error('Error loading overtime requests:', error);
                    this.loading = false;
                }
            });
    }

    onPageChange(page: number): void {
        this.pagination.currentPage = page;
        this.loadOvertimeRequests();
    }

    onPageSizeChange(pageSize: number): void {
        this.pagination.pageSize = pageSize;
        this.pagination.currentPage = 1;
        this.loadOvertimeRequests();
    }

    onSearch(searchTerm: string): void {
        this.searchTerm = searchTerm;
        this.pagination.currentPage = 1;
        this.loadOvertimeRequests();
    }

    onRowClick(overtimeRequest: OvertimeRequest): void {
        this.router.navigate(['/requests/overtime', overtimeRequest.id]);
    }

    // Modal operations
    createNewRequest(): void {
        this.isModalOpen = true;
    }

    onModalSaved(): void {
        this.isModalOpen = false;
        this.loadOvertimeRequests();
    }

    onModalCancelled(): void {
        this.isModalOpen = false;
    }

    // Delete operations
    deleteRequest(request: OvertimeRequest, event: Event): void {
        event.stopPropagation();
        this.requestToDelete = request;
        this.isDeleteDialogOpen = true;
    }

    onDeleteConfirmed(): void {
        if (this.requestToDelete) {
            this.overtimeService.deleteOvertime(this.requestToDelete.id).subscribe({
                next: () => {
                    this.isDeleteDialogOpen = false;
                    this.requestToDelete = null;
                    this.loadOvertimeRequests();
                },
                error: (error) => {
                    console.error('Error deleting overtime request:', error);
                    this.isDeleteDialogOpen = false;
                    this.requestToDelete = null;
                    alert('Failed to delete request. Please try again.');
                }
            });
        }
    }

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

    formatDateTime(date: Date): string {
        return date ? new Date(date).toLocaleString() : '-';
    }

    formatDate(date: Date): string {
        return date ? new Date(date).toLocaleDateString() : '-';
    }
}
