import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { DepartmentService } from '../../../../core/services/department.service';
import { Department, PaginatedResponse } from '../../../../core/models/department.model';
import { DataTableComponent, DataTableColumn, PaginationConfig } from '../../../../shared/components/data-table/data-table.component';
import { ConfirmDialogComponent } from '../../../../shared/components/confirm-dialog/confirm-dialog.component';
import { DepartmentModalComponent } from './department-modal/department-modal.component';

@Component({
    selector: 'app-department-list',
    standalone: true,
    imports: [CommonModule, DataTableComponent, ConfirmDialogComponent, DepartmentModalComponent],
    templateUrl: './department-list.component.html',
    styleUrls: ['./department-list.component.scss']
})
export class DepartmentListComponent implements OnInit {
    departments: Department[] = [];
    loading: boolean = false;
    searchTerm: string = '';

    columns: DataTableColumn[] = [
        { header: 'Name', field: 'name', sortable: true, width: '30%' },
        { header: 'Description', field: 'description', width: '40%' },
        { header: 'Status', field: 'isActive', sortable: true, width: '15%' },
        { header: 'Created', field: 'createdAt', sortable: true, width: '15%' }
    ];

    pagination: PaginationConfig = {
        currentPage: 1,
        pageSize: 10,
        totalItems: 0,
        pageSizeOptions: [10, 25, 50, 100]
    };

    // Modal state
    isModalOpen = false;
    modalMode: 'create' | 'edit' = 'create';
    selectedDepartment: Department | null = null;

    // Delete confirmation dialog state
    isDeleteDialogOpen = false;
    departmentToDelete: Department | null = null;

    constructor(
        private departmentService: DepartmentService,
        private router: Router
    ) { }

    ngOnInit(): void {
        this.loadDepartments();
    }

    loadDepartments(): void {
        this.loading = true;

        this.departmentService
            .getDepartments(this.pagination.currentPage, this.pagination.pageSize, this.searchTerm)
            .subscribe({
                next: (response: PaginatedResponse<Department>) => {
                    this.departments = response.items;
                    this.pagination.totalItems = response.totalCount;
                    this.loading = false;
                },
                error: (error) => {
                    console.error('Error loading departments:', error);
                    this.loading = false;
                }
            });
    }

    onPageChange(page: number): void {
        this.pagination.currentPage = page;
        this.loadDepartments();
    }

    onPageSizeChange(pageSize: number): void {
        this.pagination.pageSize = pageSize;
        this.pagination.currentPage = 1;
        this.loadDepartments();
    }

    onSearch(searchTerm: string): void {
        this.searchTerm = searchTerm;
        this.pagination.currentPage = 1;
        this.loadDepartments();
    }

    onRowClick(department: Department): void {
        this.router.navigate(['/admin/departments', department.id]);
    }

    // Modal operations
    createNewDepartment(): void {
        this.modalMode = 'create';
        this.selectedDepartment = null;
        this.isModalOpen = true;
    }

    editDepartment(department: Department, event: Event): void {
        event.stopPropagation();
        this.modalMode = 'edit';
        this.selectedDepartment = department;
        this.isModalOpen = true;
    }

    onModalSaved(): void {
        this.isModalOpen = false;
        this.selectedDepartment = null;
        this.loadDepartments();
    }

    onModalCancelled(): void {
        this.isModalOpen = false;
        this.selectedDepartment = null;
    }

    // Delete operations
    deleteDepartment(department: Department, event: Event): void {
        event.stopPropagation();
        this.departmentToDelete = department;
        this.isDeleteDialogOpen = true;
    }

    onDeleteConfirmed(): void {
        if (this.departmentToDelete) {
            this.departmentService.deleteDepartment(this.departmentToDelete.id).subscribe({
                next: () => {
                    this.isDeleteDialogOpen = false;
                    this.departmentToDelete = null;
                    this.loadDepartments();
                },
                error: (error) => {
                    console.error('Error deleting department:', error);
                    this.isDeleteDialogOpen = false;
                    this.departmentToDelete = null;
                    alert('Failed to delete department. Please try again.');
                }
            });
        }
    }

    onDeleteCancelled(): void {
        this.isDeleteDialogOpen = false;
        this.departmentToDelete = null;
    }

    getStatusBadge(isActive: boolean): string {
        return isActive ? 'Active' : 'Inactive';
    }

    getStatusClass(isActive: boolean): string {
        return isActive ? 'status-active' : 'status-inactive';
    }

    formatDate(date: Date): string {
        return new Date(date).toLocaleDateString();
    }
}
