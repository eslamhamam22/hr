import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';
import * as XLSX from 'xlsx';
import jsPDF from 'jspdf';
import autoTable from 'jspdf-autotable';

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

  getTimeOffReport(
    startDate: Date,
    endDate: Date,
    departmentId?: string
  ): Observable<any> {
    const params = {
      startDate: startDate.toISOString().split('T')[0],
      endDate: endDate.toISOString().split('T')[0],
      departmentId
    };
    return this.apiService.get('/reports/time-off', params);
  }

  getWorkFromHomeReport(
    startDate: Date,
    endDate: Date,
    departmentId?: string
  ): Observable<any> {
    const params = {
      startDate: startDate.toISOString().split('T')[0],
      endDate: endDate.toISOString().split('T')[0],
      departmentId
    };
    return this.apiService.get('/reports/work-from-home', params);
  }

  private formatDataForExport(data: any[]): any[] {
    if (!data || data.length === 0) return [];
    
    // Format dates and handle nulls
    return data.map(row => {
      const newRow: any = {};
      Object.keys(row).forEach(key => {
        // Skip ID fields
        if (key.toLowerCase().includes('id') && key.length > 2 && key !== 'valid') {
            // Keep if it's something like 'paid' but skip 'userId', 'departmentId'
            if (key.toLowerCase().endsWith('id')) return;
        }
        
        const value = row[key];
        let formattedValue = value;

        if (value instanceof Date) {
            formattedValue = value.toLocaleDateString();
        } else if (typeof value === 'string' && !isNaN(Date.parse(value)) && (value.includes('-') || value.includes('/'))) {
             // Basic check for ISO strings
             if (value.includes('T')) {
                 const date = new Date(value);
                 formattedValue = value.length > 10 ? date.toLocaleString() : date.toLocaleDateString();
             }
        }
        
        // Convert camelCase key to Title Case
        const titleKey = key.replace(/([A-Z])/g, ' $1').replace(/^./, str => str.toUpperCase());
        newRow[titleKey] = formattedValue ?? '';
      });
      return newRow;
    });
  }

  exportToExcel(data: any[], filename: string): void {
    const formattedData = this.formatDataForExport(data);
    const ws: XLSX.WorkSheet = XLSX.utils.json_to_sheet(formattedData);
    const wb: XLSX.WorkBook = XLSX.utils.book_new();
    XLSX.utils.book_append_sheet(wb, ws, 'Report');
    XLSX.writeFile(wb, filename);
  }

  exportToCsv(data: any[], filename: string): void {
    const formattedData = this.formatDataForExport(data);
    const ws: XLSX.WorkSheet = XLSX.utils.json_to_sheet(formattedData);
    const csv = XLSX.utils.sheet_to_csv(ws);
    
    const blob = new Blob([csv], { type: 'text/csv;charset=utf-8;' });
    const link = document.createElement('a');
    const url = URL.createObjectURL(blob);
    link.setAttribute('href', url);
    link.setAttribute('download', filename);
    link.style.visibility = 'hidden';
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
  }

  exportToPdf(data: any[], filename: string): void {
    const formattedData = this.formatDataForExport(data);
    if (formattedData.length === 0) return;

    const doc = new jsPDF();
    const headers = Object.keys(formattedData[0]);
    const rows = formattedData.map(row => Object.values(row));

    autoTable(doc, {
      head: [headers],
      body: rows as any[][],
      styles: { fontSize: 8 },
      headStyles: { fillColor: [102, 126, 234] } // Matches our theme color #667eea
    });

    doc.save(filename);
  }
}