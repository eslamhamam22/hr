import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { OvertimeRequest, CreateOvertimeDto } from '../models/overtime.model';
import { PaginatedResponse } from '../models/department.model';
import { ApiService } from './api.service';

@Injectable({
    providedIn: 'root'
})
export class OvertimeService {

    private apiUrl = '/requests/overtime';

    constructor(private apiService: ApiService) { }

    getOvertimeRequests(page: number = 1, pageSize: number = 10, status?: string, search: string = ''): Observable<PaginatedResponse<OvertimeRequest>> {
        const params: any = {
            page: page,
            pageSize: pageSize
        };

        if (status) {
            params.status = status;
        }
        if (search) {
            params.search = search;
        }

        return this.apiService.get<PaginatedResponse<OvertimeRequest>>(this.apiUrl, params);
    }

    getOvertimeById(id: string): Observable<OvertimeRequest> {
        return this.apiService.get<OvertimeRequest>(`${this.apiUrl}/${id}`);
    }

    createOvertime(overtime: CreateOvertimeDto): Observable<OvertimeRequest> {
        return this.apiService.post<OvertimeRequest>(this.apiUrl, overtime);
    }

    submitOvertime(id: string): Observable<void> {
        return this.apiService.post<void>(`${this.apiUrl}/${id}/submit`, {});
    }

    deleteOvertime(id: string): Observable<void> {
        return this.apiService.delete<void>(`${this.apiUrl}/${id}`);
    }
}
