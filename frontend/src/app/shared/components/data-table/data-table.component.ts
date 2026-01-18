import { Component, Input, Output, EventEmitter, ContentChild, TemplateRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

export interface DataTableColumn {
  header: string;
  field: string;
  width?: string;
  sortable?: boolean;
}

export interface PaginationConfig {
  currentPage: number;
  pageSize: number;
  totalItems: number;
  pageSizeOptions?: number[];
}

@Component({
  selector: 'app-data-table',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './data-table.component.html',
  styleUrls: ['./data-table.component.scss']
})
export class DataTableComponent {
  @Input() columns: DataTableColumn[] = [];
  @Input() data: any[] = [];
  @Input() rowStyle?: (row: any) => { [key: string]: any };
  @Input() pagination?: PaginationConfig;
  @Input() loading: boolean = false;
  @Input() searchable: boolean = false;

  @Output() pageChange = new EventEmitter<number>();
  @Output() pageSizeChange = new EventEmitter<number>();
  @Output() sortChange = new EventEmitter<{ field: string, direction: 'asc' | 'desc' }>();
  @Output() searchChange = new EventEmitter<string>();
  @Output() rowClick = new EventEmitter<any>();

  @ContentChild('cellTemplate') cellTemplate?: TemplateRef<any>;
  @ContentChild('actionsTemplate') actionsTemplate?: TemplateRef<any>;

  searchTerm: string = '';
  sortField: string = '';
  sortDirection: 'asc' | 'desc' = 'asc';

  get totalPages(): number {
    if (!this.pagination) return 1;
    return Math.ceil(this.pagination.totalItems / this.pagination.pageSize);
  }

  get pageSizeOptions(): number[] {
    return this.pagination?.pageSizeOptions || [10, 25, 50, 100];
  }

  onSearch(term: string): void {
    this.searchTerm = term;
    this.searchChange.emit(term);
  }

  onSort(column: DataTableColumn): void {
    if (!column.sortable) return;

    if (this.sortField === column.field) {
      this.sortDirection = this.sortDirection === 'asc' ? 'desc' : 'asc';
    } else {
      this.sortField = column.field;
      this.sortDirection = 'asc';
    }

    this.sortChange.emit({ field: this.sortField, direction: this.sortDirection });
  }

  onPageChange(page: number): void {
    if (!this.pagination || page < 1 || page > this.totalPages) return;
    this.pageChange.emit(page);
  }

  onPageSizeChange(size: number): void {
    this.pageSizeChange.emit(size);
  }

  onRowClick(row: any): void {
    this.rowClick.emit(row);
  }

  getSortIcon(column: DataTableColumn): string {
    if (!column.sortable) return '';
    if (this.sortField !== column.field) return '⇅';
    return this.sortDirection === 'asc' ? '▲' : '▼';
  }

  getStartItem(): number {
    if (!this.pagination) return 0;
    return (this.pagination.currentPage - 1) * this.pagination.pageSize + 1;
  }

  getEndItem(): number {
    if (!this.pagination) return 0;
    return Math.min(this.pagination.currentPage * this.pagination.pageSize, this.pagination.totalItems);
  }
}

