import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ReportService } from '../../../core/services/report.service';
import { ReportFiltersComponent } from '../report-filters/report-filters.component';

@Component({
  selector: 'app-report-viewer',
  standalone: true,
  imports: [CommonModule, FormsModule, ReportFiltersComponent],
  templateUrl: './report-viewer.component.html',
  styleUrls: ['./report-viewer.component.scss']
})
export class ReportViewerComponent {
  selectedReport = '';
  reportData = signal<any[]>([]);
  reportHeaders = signal<string[]>([]);
  isLoading = signal(false);
  errorMessage = signal('');

  constructor(private reportService: ReportService) {}

  onReportChange(): void {
    this.reportData.set([]);
    this.reportHeaders.set([]);
  }

  onFilterApplied(filters: any): void {
    this.isLoading.set(true);
    this.errorMessage.set('');

    // Load report data based on selected report and filters
    console.log('Report filter applied:', filters);
    
    this.isLoading.set(false);
  }

  exportExcel(): void {
    this.reportService.exportToExcel(this.reportData(), 'report.xlsx');
  }

  exportCsv(): void {
    this.reportService.exportToCsv(this.reportData(), 'report.csv');
  }

  exportPdf(): void {
    this.reportService.exportToPdf(this.reportData(), 'report.pdf');
  }
}
