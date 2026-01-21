import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';
import { WorkFromHomeRequest, CreateWorkFromHomeRequest, PaginatedResult } from '../models/work-from-home.model';

@Injectable({
    providedIn: 'root'
})
export class WorkFromHomeService {
    private readonly baseUrl = '/WorkFromHome';

    constructor(private apiService: ApiService) { }

    getWorkFromHomeRequests(
        page: number = 1,
        pageSize: number = 10,
        status?: string,
        userId?: string
    ): Observable<PaginatedResult<WorkFromHomeRequest>> {
        const params: any = { page, pageSize };
        if (status) params.status = status;
        if (userId) params.userId = userId;

        return this.apiService.get(this.baseUrl, params);
    }

    getWorkFromHomeRequestById(id: string): Observable<WorkFromHomeRequest> {
        return this.apiService.get(`${this.baseUrl}/${id}`);
    }

    createWorkFromHomeRequest(request: CreateWorkFromHomeRequest): Observable<WorkFromHomeRequest> {
        return this.apiService.post(this.baseUrl, request);
    }

    submitWorkFromHomeRequest(id: string): Observable<any> {
        return this.apiService.post(`${this.baseUrl}/${id}/submit`, {});
    }

    approveWorkFromHomeRequest(id: string): Observable<any> {
        return this.apiService.post(`${this.baseUrl}/${id}/approve`, {});
    }

    rejectWorkFromHomeRequest(id: string, reason: string): Observable<any> {
        return this.apiService.post(`${this.baseUrl}/${id}/reject`, { reason });
    }

    deleteWorkFromHomeRequest(id: string): Observable<any> {
        return this.apiService.delete(`${this.baseUrl}/${id}`);
    }
}
