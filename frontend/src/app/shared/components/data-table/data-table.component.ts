import { Component, Input, ContentChild, TemplateRef } from '@angular/core';
import { CommonModule } from '@angular/common';

export interface DataTableColumn {
  header: string;
  field: string;
  width?: string;
}

@Component({
  selector: 'app-data-table',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './data-table.component.html',
  styleUrls: ['./data-table.component.scss']
})
export class DataTableComponent {
  @Input() columns: DataTableColumn[] = [];
  @Input() data: any[] = [];
  @Input() rowStyle?: (row: any) => { [key: string]: any };
  
  @ContentChild('cellTemplate') cellTemplate?: TemplateRef<any>;
}
