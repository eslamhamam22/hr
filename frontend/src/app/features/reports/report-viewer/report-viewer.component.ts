import { Component, OnInit, signal } from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ReportService } from '../../../core/services/report.service';
import { ReportFiltersComponent } from '../report-filters/report-filters.component';

@Component({
    selector: 'app-report-viewer',
    standalone: true,
    imports: [CommonModule, FormsModule, ReportFiltersComponent],
    providers: [DatePipe],
    templateUrl: './report-viewer.component.html',
    styleUrls: ['./report-viewer.component.scss']
})
export class ReportViewerComponent {
    selectedReport = '';
    reportData = signal<any[]>([]);
    reportHeaders = signal<string[]>([]);
    isLoading = signal(false);
    errorMessage = signal('');

    // Store current filters for export
    currentFilters: any = {};

    constructor(
        private reportService: ReportService,
        private datePipe: DatePipe
    ) { }

    onReportChange(): void {
        this.reportData.set([]);
        this.reportHeaders.set([]);
        this.errorMessage.set('');
    }

    onFilterApplied(filters: any): void {
        this.isLoading.set(true);
        this.errorMessage.set('');
        this.currentFilters = filters;
        this.reportData.set([]);

        let request;
        const startDate = filters.startDate ? new Date(filters.startDate) : new Date();
        const endDate = filters.endDate ? new Date(filters.endDate) : new Date();

        switch (filters.reportType) {
            case 'attendance':
                request = this.reportService.getAttendanceReport(
                    startDate,
                    endDate,
                    filters.departmentId,
                    filters.employeeId
                );
                break;
            case 'leave-summary':
                request = this.reportService.getLeaveSummaryReport(filters.departmentId);
                break;
            case 'overtime':
                request = this.reportService.getOvertimeAuditReport(
                    startDate,
                    endDate,
                    filters.departmentId
                );
                break;
            case 'time-off':
                request = this.reportService.getTimeOffReport(
                    startDate,
                    endDate,
                    filters.departmentId
                );
                break;
            case 'work-from-home':
                request = this.reportService.getWorkFromHomeReport(
                    startDate,
                    endDate,
                    filters.departmentId
                );
                break;
            case 'monthly-work-summary':
                request = this.reportService.getMonthlyWorkSummaryReport(
                    filters.year,
                    filters.month,
                    filters.departmentId,
                    filters.employeeId
                );
                break;
            case 'monthly-work-details':
                if (!filters.employeeId) {
                    this.errorMessage.set('Please select an employee for the details report.');
                    this.isLoading.set(false);
                    return;
                }
                request = this.reportService.getMonthlyWorkDetailsReport(
                    filters.employeeId,
                    filters.year,
                    filters.month
                );
                break;
            default:
                this.isLoading.set(false);
                return;
        }

        request.subscribe({
            next: (data) => {
                // Special handling for monthly-work-details which returns a single object with dailyDetails
                if (filters.reportType === 'monthly-work-details' && data && data.dailyDetails) {
                    this.reportData.set(data.dailyDetails);
                    if (data.dailyDetails.length > 0) {
                        this.generateHeaders(data.dailyDetails[0]);
                    }
                    // Store the summary info for possible display
                    this.currentFilters.detailsSummary = {
                        employeeName: data.employeeName,
                        departmentName: data.departmentName,
                        year: data.year,
                        month: data.month,
                        workingDaysInMonth: data.workingDaysInMonth,
                        expectedHours: data.expectedHours,
                        totalAttendanceHours: data.totalAttendanceHours,
                        totalLeaveHours: data.totalLeaveHours,
                        totalWorkFromHomeHours: data.totalWorkFromHomeHours,
                        totalTimeOffHours: data.totalTimeOffHours,
                        grandTotalHours: data.grandTotalHours,
                        totalDaysPresent: data.totalDaysPresent,
                        totalDaysOnLeave: data.totalDaysOnLeave,
                        totalDaysWorkFromHome: data.totalDaysWorkFromHome,
                        totalDaysAbsent: data.totalDaysAbsent
                    };
                } else if (Array.isArray(data)) {
                    this.reportData.set(data);
                    if (data.length > 0) {
                        this.generateHeaders(data[0]);
                    }
                } else {
                    this.reportData.set([data]);
                    this.generateHeaders(data);
                }
                this.isLoading.set(false);
            },
            error: (err) => {
                console.error('Error loading report', err);
                this.errorMessage.set('Failed to load report data. ' + (err.error?.message || err.message));
                this.isLoading.set(false);
            }
        });
    }

    generateHeaders(row: any): void {
        if (!row) return;
        // Extract keys as headers, format them nicely
        // Exclude ID and technical fields
        const excludedKeys = ['id', 'userId', 'managerId', 'approvedByHRId'];
        const keys = Object.keys(row).filter(k =>
            !excludedKeys.includes(k) &&
            (typeof row[k] !== 'object' || row[k] === null || row[k] instanceof Date)
        );
        this.reportHeaders.set(keys);
    }

    formatHeader(header: string): string {
        // split camelCase to Camel Case
        return header.replace(/([A-Z])/g, ' $1').replace(/^./, str => str.toUpperCase());
    }

    formatValue(header: string, value: any): string {
        if (value === null || value === undefined) return '-';

        // Check for date/time fields based on header name
        const timeFields = ['startTime', 'checkIn', 'checkOut'];
        const dateTimeFields = ['start', 'end', 'submitted'];
        const dateFields = ['date', 'startDate', 'endDate', 'from', 'to'];

        const lowerHeader = header.toLowerCase();

        // Check if value is likely a date
        const isDate = !isNaN(Date.parse(value)) && typeof value === 'string' &&
            (value.includes('-') || value.includes('T') || value.includes(':'));

        if (dateTimeFields.includes(lowerHeader)) {
            return this.datePipe.transform(value, 'dd MMM yyyy HH:mm') || value;
        }

        if (dateFields.includes(lowerHeader) || (isDate && !lowerHeader.includes('time'))) {
            return this.datePipe.transform(value, 'dd MMM yyyy') || value;
        }

        return value;
    }

    exportExcel(): void {
        this.reportService.exportToExcel(this.reportData(), `report-${this.selectedReport}.xlsx`);
    }

    exportCsv(): void {
        this.reportService.exportToCsv(this.reportData(), `report-${this.selectedReport}.csv`);
    }

    exportPdf(): void {
        this.reportService.exportToPdf(this.reportData(), `report-${this.selectedReport}.pdf`);
    }
}
