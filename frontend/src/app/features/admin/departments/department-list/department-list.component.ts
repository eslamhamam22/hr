import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { DepartmentService } from '../../../../core/services/department.service';
import { Department, PaginatedResponse } from '../../../../core/models/department.model';
import { DataTableComponent, DataTableColumn, PaginationConfig } from '../../../../shared/components/data-table/data-table.component';

@Component({
    selector: 'app-department-list',
    standalone: true,
    imports: [CommonModule, DataTableComponent],
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

    createNewDepartment(): void {
        this.router.navigate(['/admin/departments/new']);
    }

    editDepartment(department: Department, event: Event): void {
        event.stopPropagation();
        this.router.navigate(['/admin/departments', department.id, 'edit']);
    }

    deleteDepartment(department: Department, event: Event): void {
        event.stopPropagation();

        if (confirm(`Are you sure you want to delete department "${department.name}"?`)) {
            this.departmentService.deleteDepartment(department.id).subscribe({
                next: () => {
                    this.loadDepartments();
                },
                error: (error) => {
                    console.error('Error deleting department:', error);
                    alert('Failed to delete department. Please try again.');
                }
            });
        }
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
