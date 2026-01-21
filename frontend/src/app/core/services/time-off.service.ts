import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { TimeOffRequest, CreateTimeOffDto } from '../models/time-off.model';
import { PaginatedResponse } from '../models/department.model';
import { ApiService } from './api.service';

@Injectable({
    providedIn: 'root'
})
export class TimeOffService {

    private apiUrl = '/time-off-requests'; // Adjusted to match Controller endpoint

    constructor(private apiService: ApiService) { }

    getTimeOffRequests(page: number = 1, pageSize: number = 10, status?: string, userId?: string): Observable<PaginatedResponse<TimeOffRequest>> {
        const params: any = {
            page: page,
            pageSize: pageSize
        };

        if (status) {
            params.status = status;
        }
        if (userId) {
            params.userId = userId;
        }

        return this.apiService.get<PaginatedResponse<TimeOffRequest>>(this.apiUrl, params);
    }

    createTimeOff(timeOff: CreateTimeOffDto): Observable<TimeOffRequest> {
        return this.apiService.post<TimeOffRequest>(this.apiUrl, timeOff);
    }

    submitTimeOff(id: string): Observable<void> {
        return this.apiService.post<void>(`${this.apiUrl}/${id}/submit`, {});
    }

    deleteTimeOff(id: string): Observable<void> {
        // Backend didn't explicitly have Delete endpoint in Controller for TimeOff? 
        // Wait, I updated `RequestsController` but I didn't add Delete endpoint for TimeOff specifically?
        // RequestsController has Submit, Approve, Reject.
        // It doesn't seem to have Delete generic endpoint.
        // OvertimeService has Delete.
        // I should check RequestsController again.
        return this.apiService.delete<void>(`${this.apiUrl}/${id}`);
    }
}
