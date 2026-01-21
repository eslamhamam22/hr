import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { DataTableComponent, DataTableColumn } from '../../../shared/components/data-table/data-table.component';
import { StatusColorPipe } from '../../../shared/pipes/status-color.pipe';
import { RequestService } from '../../../core/services/request.service';
import { Request } from '../../../core/models/request.model';
import { AuthService } from '@core/auth/auth.service';
import { RequestDetailsModalComponent } from './request-details-modal/request-details-modal.component';

@Component({
    selector: 'app-all-requests',
    standalone: true,
    imports: [CommonModule, FormsModule, DataTableComponent, StatusColorPipe, RequestDetailsModalComponent],
    providers: [StatusColorPipe],
    templateUrl: './all-requests.component.html',
    styleUrls: ['./all-requests.component.scss']
})
export class AllRequestsComponent implements OnInit {
    requests = signal<Request[]>([]);
    filteredRequests = signal<Request[]>([]);
    isLoading = signal(false);
    errorMessage = signal('');

    // Modal state
    selectedRequest: Request | null = null;
    isModalOpen = false;

    // Filter options
    selectedUserId = '';
    selectedDepartmentId: string | null = null;
    selectedStatus = '';
    fromDate = '';
    toDate = '';

    // Available filter options (these should be loaded from backend)
    statusOptions = ['Draft', 'Submitted', 'PendingHR', 'Approved', 'Rejected'];

    columns: DataTableColumn[] = [
        { header: 'Employee', field: 'employeeName' },
        { header: 'Request Type', field: 'requestType' },
        { header: 'Start Date', field: 'startDate' },
        { header: 'End Date', field: 'endDate' },
        { header: 'Status', field: 'status' },
        { header: 'Submitted', field: 'submittedAt' },
        { header: 'Approved By', field: 'approvedByName' },
    ];

    constructor(
        private requestService: RequestService,
        private statusColorPipe: StatusColorPipe,
        private authService: AuthService
    ) { }

    getRowStyle = (request: Request) => {
        return { 'background-color': this.statusColorPipe.transform(request.status) };
    };

    ngOnInit(): void {
        this.loadRequests();
    }

    loadRequests(): void {
        this.isLoading.set(true);
        this.errorMessage.set('');

        this.requestService.getAllRequests(
            this.selectedUserId || undefined,
            this.selectedDepartmentId || undefined,
            this.selectedStatus || undefined,
            this.fromDate || undefined,
            this.toDate || undefined
        ).subscribe({
            next: (data) => {
                this.requests.set(data);
                this.filteredRequests.set(data);
                this.isLoading.set(false);
            },
            error: (error) => {
                this.errorMessage.set('Failed to load requests');
                this.isLoading.set(false);
                console.error('Error loading requests:', error);
            }
        });
    }

    applyFilters(): void {
        this.loadRequests();
    }

    clearFilters(): void {
        this.selectedUserId = '';
        this.selectedDepartmentId = null;
        this.selectedStatus = '';
        this.fromDate = '';
        this.toDate = '';
        this.loadRequests();
    }

    onRowClick(request: Request): void {
        this.selectedRequest = request;
        this.isModalOpen = true;
    }

    onModalClose(): void {
        this.isModalOpen = false;
        this.selectedRequest = null;
    }

    onApproveRequest(requestId: string): void {
        this.requestService.approveRequest(requestId).subscribe({
            next: () => {
                this.loadRequests();
                this.onModalClose();
            },
            error: (error) => {
                console.error('Error approving request:', error);
                this.errorMessage.set('Failed to approve request');
            }
        });
    }

    onRejectRequest(data: { id: string; reason: string }): void {
        this.requestService.rejectRequest(data.id, data.reason).subscribe({
            next: () => {
                this.loadRequests();
                this.onModalClose();
            },
            error: (error) => {
                console.error('Error rejecting request:', error);
                this.errorMessage.set('Failed to reject request');
            }
        });
    }
}
