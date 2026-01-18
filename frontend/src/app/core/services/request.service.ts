import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';
import { Request } from '../models/request.model';

@Injectable({
  providedIn: 'root'
})
export class RequestService {
  constructor(private apiService: ApiService) {}

  createLeaveRequest(request: any): Observable<any> {
    return this.apiService.post('/requests/leave', request);
  }

  submitRequest(requestId: string): Observable<any> {
    return this.apiService.post(`/requests/${requestId}/submit`, {});
  }

  getRequestHistory(userId: string): Observable<Request[]> {
    return this.apiService.get(`/requests/history/${userId}`);
  }

  getPendingApprovals(): Observable<Request[]> {
    return this.apiService.get('/requests/pending-approvals');
  }

  approveRequest(requestId: string): Observable<any> {
    return this.apiService.post(`/requests/${requestId}/approve`, {});
  }

  rejectRequest(requestId: string, reason: string): Observable<any> {
    return this.apiService.post(`/requests/${requestId}/reject`, { reason });
  }
}
