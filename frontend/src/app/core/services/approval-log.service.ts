import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';
import { ApprovalLog, ApprovalLogFilters } from '../models/approval-log.model';
import { PaginatedResponse } from '../models/department.model';

@Injectable({
    providedIn: 'root'
})
export class ApprovalLogService {
    private readonly baseUrl = '/approval-logs';

    constructor(private apiService: ApiService) { }

    getApprovalLogs(
        page: number = 1,
        pageSize: number = 10,
        filters?: ApprovalLogFilters
    ): Observable<PaginatedResponse<ApprovalLog>> {
        const params: any = {
            page,
            pageSize
        };

        if (filters?.requestType) params.requestType = filters.requestType;
        if (filters?.approvedByUserId) params.approvedByUserId = filters.approvedByUserId;
        if (filters?.approved !== undefined && filters?.approved !== null) params.approved = filters.approved;

        return this.apiService.get<PaginatedResponse<ApprovalLog>>(this.baseUrl, params);
    }

    getApprovalLogById(id: string): Observable<ApprovalLog> {
        return this.apiService.get<ApprovalLog>(`${this.baseUrl}/${id}`);
    }

    getApprovalLogsForRequest(requestId: string): Observable<ApprovalLog[]> {
        return this.apiService.get<ApprovalLog[]>(`${this.baseUrl}/request/${requestId}`);
    }
}
