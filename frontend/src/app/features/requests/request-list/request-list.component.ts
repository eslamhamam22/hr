import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { DataTableComponent, DataTableColumn } from '../../../shared/components/data-table/data-table.component';
import { StatusColorPipe } from '../../../shared/pipes/status-color.pipe';
import { RequestService } from '../../../core/services/request.service';
import { Request } from '../../../core/models/request.model';

@Component({
  selector: 'app-request-list',
  standalone: true,
  imports: [CommonModule, DataTableComponent, StatusColorPipe],
  providers: [StatusColorPipe],
  templateUrl: './request-list.component.html',
})
export class RequestListComponent implements OnInit {
  requests = signal<Request[]>([]);
  isLoading = signal(false);
  errorMessage = signal('');

  columns: DataTableColumn[] = [
    { header: 'Request Type', field: 'requestType' },
    { header: 'Start Date', field: 'startDate' },
    { header: 'End Date', field: 'endDate' },
    { header: 'Status', field: 'status' },
    { header: 'Submitted', field: 'submittedAt' },
    { header: 'Approved By', field: 'approvedByName' },
  ];

  constructor(
    private requestService: RequestService,
    private statusColorPipe: StatusColorPipe
  ) {}

  getRowStyle = (request: Request) => {
    return { 'background-color': this.statusColorPipe.transform(request.status) };
  };

  ngOnInit(): void {
    this.refreshData();
  }

  refreshData(): void {
    this.isLoading.set(true);
    this.errorMessage.set('');

    this.requestService.getRequestHistory('').subscribe({
      next: (data) => {
        this.requests.set(data);
        this.isLoading.set(false);
      },
      error: (error) => {
        this.errorMessage.set('Failed to load requests');
        this.isLoading.set(false);
      }
    });
  }
}
