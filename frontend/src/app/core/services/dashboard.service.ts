import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';
import { AdminDashboard, EmployeeDashboard, ManagerDashboard } from '../models/dashboard.model';

@Injectable({
    providedIn: 'root'
})
export class DashboardService {
    private apiUrl = '/dashboard';

    constructor(private apiService: ApiService) { }

    getEmployeeDashboard(): Observable<EmployeeDashboard> {
        return this.apiService.get<EmployeeDashboard>(`${this.apiUrl}/employee`);
    }

    getManagerDashboard(): Observable<ManagerDashboard> {
        return this.apiService.get<ManagerDashboard>(`${this.apiUrl}/manager`);
    }

    getAdminDashboard(): Observable<AdminDashboard> {
        return this.apiService.get<AdminDashboard>(`${this.apiUrl}/admin`);
    }
}
