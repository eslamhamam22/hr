import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ApprovalLog, ApprovalLogFilters } from '../models/approval-log.model';
import { PaginatedResponse } from '../models/department.model';
import { ApiService } from './api.service';

@Injectable({
    providedIn: 'root'
})
export class ApprovalLogService {
    private apiUrl = '/approval-logs';

    constructor(private apiService: ApiService) { }

    getApprovalLogs(page: number = 1, pageSize: number = 10, filters?: ApprovalLogFilters): Observable<PaginatedResponse<ApprovalLog>> {
        const params: any = {
            page: page,
            pageSize: pageSize
        };

        if (filters) {
            if (filters.requestType) params.requestType = filters.requestType;
            if (filters.approvedByUserId) params.approvedByUserId = filters.approvedByUserId;
            if (filters.approved !== undefined) params.approved = filters.approved;
            if (filters.startDate) params.startDate = filters.startDate.toISOString();
            if (filters.endDate) params.endDate = filters.endDate.toISOString();
        }

        return this.apiService.get<PaginatedResponse<ApprovalLog>>(this.apiUrl, params);
    }

    getApprovalLogById(id: string): Observable<ApprovalLog> {
        return this.apiService.get<ApprovalLog>(`${this.apiUrl}/${id}`);
    }

    getLogsByRequest(requestId: string): Observable<ApprovalLog[]> {
        return this.apiService.get<ApprovalLog[]>(`${this.apiUrl}/request/${requestId}`);
    }
}
