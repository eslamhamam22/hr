import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { DataTableComponent, DataTableColumn, PaginationConfig } from '../../../shared/components/data-table/data-table.component';
import { StatusColorPipe } from '../../../shared/pipes/status-color.pipe';
import { WorkFromHomeService } from '../../../core/services/work-from-home.service';
import { WorkFromHomeRequest } from '../../../core/models/work-from-home.model';
import { AuthService } from '@core/auth/auth.service';
import { CreateWfhModalComponent } from './create-wfh-modal/create-wfh-modal.component';
import { getStatusClass, getStatusLabel, RequestStatus } from '@core/models/overtime.model';

@Component({
    selector: 'app-work-from-home',
    standalone: true,
    imports: [CommonModule, FormsModule, DataTableComponent, StatusColorPipe, CreateWfhModalComponent],
    providers: [StatusColorPipe],
    templateUrl: './work-from-home.component.html',
    styleUrls: ['./work-from-home.component.scss']
})
export class WorkFromHomeComponent implements OnInit {
    requests = signal<WorkFromHomeRequest[]>([]);
    isLoading = signal(false);
    errorMessage = signal('');
    isModalOpen = false;

    columns: DataTableColumn[] = [
        { header: 'From Date', field: 'fromDate', sortable: true },
        { header: 'To Date', field: 'toDate', sortable: true },
        { header: 'Total Days', field: 'totalDays', sortable: true },
        { header: 'Status', field: 'status', sortable: true },
        { header: 'Submitted', field: 'submittedAt', sortable: true },
        { header: 'Approved By', field: 'approvedByHRName' },
    ];

    pagination: PaginationConfig = {
        currentPage: 1,
        pageSize: 10,
        totalItems: 0,
        pageSizeOptions: [10, 25, 50, 100]
    };

    constructor(
        private wfhService: WorkFromHomeService,
        private statusColorPipe: StatusColorPipe,
        private authService: AuthService
    ) { }

    getRowStyle = (request: WorkFromHomeRequest) => {
        return { 'background-color': this.statusColorPipe.transform(request.status) };
    };

    ngOnInit(): void {
        this.loadRequests();
    }

    loadRequests(): void {
        this.isLoading.set(true);
        this.errorMessage.set('');
        const userId = this.authService.user()?.id || '';

        this.wfhService.getWorkFromHomeRequests(1, 100, undefined, userId).subscribe({
            next: (data) => {
                this.requests.set(data.items);
                this.isLoading.set(false);
            },
            error: (error) => {
                this.errorMessage.set('Failed to load work from home requests');
                this.isLoading.set(false);
                console.error('Error loading WFH requests:', error);
            }
        });
    }

    onPageChange(page: number): void {
        this.pagination.currentPage = page;
        this.loadRequests();
    }

    onPageSizeChange(pageSize: number): void {
        this.pagination.pageSize = pageSize;
        this.pagination.currentPage = 1;
        this.loadRequests();
    }

    openCreateModal(): void {
        this.isModalOpen = true;
    }

    onModalSaved(): void {
        this.isModalOpen = false;
        this.loadRequests();
    }

    onModalCancelled(): void {
        this.isModalOpen = false;
    }

    onSubmitRequest(request: WorkFromHomeRequest): void {
        this.wfhService.submitWorkFromHomeRequest(request.id).subscribe({
            next: () => {
                this.loadRequests();
            },
            error: (error) => {
                console.error('Error submitting request:', error);
                this.errorMessage.set('Failed to submit request');
            }
        });
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
