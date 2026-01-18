import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { AttendanceLog, AttendanceFilters } from '../models/attendance.model';
import { PaginatedResponse } from '../models/department.model';
import { ApiService } from './api.service';

@Injectable({
    providedIn: 'root'
})
export class AttendanceService {
    private apiUrl = '/attendance';

    constructor(private apiService: ApiService) { }

    getAttendanceLogs(page: number = 1, pageSize: number = 10, filters?: AttendanceFilters): Observable<PaginatedResponse<AttendanceLog>> {
        const params: any = {
            page: page,
            pageSize: pageSize
        };

        if (filters) {
            if (filters.userId) params.userId = filters.userId;
            if (filters.startDate) params.startDate = filters.startDate.toISOString();
            if (filters.endDate) params.endDate = filters.endDate.toISOString();
            if (filters.isLate !== undefined) params.isLate = filters.isLate;
            if (filters.isAbsent !== undefined) params.isAbsent = filters.isAbsent;
        }

        return this.apiService.get<PaginatedResponse<AttendanceLog>>(this.apiUrl, params);
    }

    getAttendanceById(id: string): Observable<AttendanceLog> {
        return this.apiService.get<AttendanceLog>(`${this.apiUrl}/${id}`);
    }

    getAttendanceByUser(userId: string, startDate?: Date, endDate?: Date): Observable<AttendanceLog[]> {
        const params: any = { userId: userId };

        if (startDate) params.startDate = startDate.toISOString();
        if (endDate) params.endDate = endDate.toISOString();

        return this.apiService.get<AttendanceLog[]>(`${this.apiUrl}/user`, params);
    }
}
