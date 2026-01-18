import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Department, CreateDepartmentDto, UpdateDepartmentDto, PaginatedResponse } from '../models/department.model';
import { ApiService } from './api.service';

@Injectable({
    providedIn: 'root'
})
export class DepartmentService {
    private apiUrl = '/departments';

    constructor(private apiService: ApiService) { }

    getDepartments(page: number = 1, pageSize: number = 10, search: string = ''): Observable<PaginatedResponse<Department>> {
        const params: any = {
            page: page,
            pageSize: pageSize
        };

        if (search) {
            params.search = search;
        }

        return this.apiService.get<PaginatedResponse<Department>>(this.apiUrl, params);
    }

    getDepartmentById(id: string): Observable<Department> {
        return this.apiService.get<Department>(`${this.apiUrl}/${id}`);
    }

    createDepartment(department: CreateDepartmentDto): Observable<Department> {
        return this.apiService.post<Department>(this.apiUrl, department);
    }

    updateDepartment(id: string, department: UpdateDepartmentDto): Observable<Department> {
        return this.apiService.put<Department>(`${this.apiUrl}/${id}`, department);
    }

    deleteDepartment(id: string): Observable<void> {
        return this.apiService.delete<void>(`${this.apiUrl}/${id}`);
    }
}
