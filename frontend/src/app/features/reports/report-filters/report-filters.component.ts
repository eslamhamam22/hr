import { Component, Input, Output, EventEmitter, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-report-filters',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './report-filters.component.html',
  styleUrls: ['./report-filters.component.scss']
})
export class ReportFiltersComponent {
  @Input() reportType: string = '';
  @Output() filterApplied = new EventEmitter<any>();

  startDate = '';
  endDate = '';
  department = '';
  employee = '';

  applyFilters(): void {
    const filters = {
      reportType: this.reportType,
      startDate: this.startDate,
      endDate: this.endDate,
      department: this.department,
      employee: this.employee
    };
    this.filterApplied.emit(filters);
  }

  clearFilters(): void {
    this.startDate = '';
    this.endDate = '';
    this.department = '';
    this.employee = '';
    this.applyFilters();
  }
}
