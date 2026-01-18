import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';

@Injectable({
  providedIn: 'root'
})
export class ReportService {
  constructor(private apiService: ApiService) {}

  getAttendanceReport(
    startDate: Date,
    endDate: Date,
    departmentId?: string,
    employeeId?: string
  ): Observable<any> {
    const params = {
      startDate: startDate.toISOString().split('T')[0],
      endDate: endDate.toISOString().split('T')[0],
      departmentId,
      employeeId
    };
    return this.apiService.get('/reports/attendance', params);
  }

  getLeaveSummaryReport(departmentId?: string): Observable<any> {
    const params = { departmentId };
    return this.apiService.get('/reports/leave-summary', params);
  }

  getOvertimeAuditReport(
    startDate: Date,
    endDate: Date,
    departmentId?: string
  ): Observable<any> {
    const params = {
      startDate: startDate.toISOString().split('T')[0],
      endDate: endDate.toISOString().split('T')[0],
      departmentId
    };
    return this.apiService.get('/reports/overtime-audit', params);
  }

  exportToExcel(data: any[], filename: string): void {
    // Implementation for Excel export
    console.log('Exporting to Excel:', filename);
  }

  exportToCsv(data: any[], filename: string): void {
    // Implementation for CSV export
    console.log('Exporting to CSV:', filename);
  }

  exportToPdf(data: any[], filename: string): void {
    // Implementation for PDF export
    console.log('Exporting to PDF:', filename);
  }
}
