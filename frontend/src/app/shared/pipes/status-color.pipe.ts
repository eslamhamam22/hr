import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
  name: 'statusColor',
  standalone: true
})
export class StatusColorPipe implements PipeTransform {
  transform(status: string): string {
    const colorMap: { [key: string]: string } = {
      'draft': '#f0f0f0',
      'submitted': '#fff3cd',
      'pendingmanager': '#cfe2ff',
      'pendinghr': '#cfe2ff',
      'approved': '#d1e7dd',
      'rejected': '#f8d7da',
      'cancelled': '#e2e3e5',
      'withdrawn': '#e2e3e5'
    };

    return colorMap[status?.toLowerCase()] || '#f0f0f0';
  }
}
