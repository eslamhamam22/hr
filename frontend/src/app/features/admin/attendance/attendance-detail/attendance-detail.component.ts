import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { AttendanceService } from '../../../../core/services/attendance.service';
import { AttendanceLog } from '../../../../core/models/attendance.model';

@Component({
  selector: 'app-attendance-detail',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './attendance-detail.component.html',
  styleUrls: ['./attendance-detail.component.scss']
})
export class AttendanceDetailComponent implements OnInit {
  attendanceLog: AttendanceLog | null = null;
  loading: boolean = true;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private attendanceService: AttendanceService
  ) { }

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.attendanceService.getAttendanceById(id).subscribe({
        next: (log) => {
          this.attendanceLog = log;
          this.loading = false;
        },
        error: () => {
          this.loading = false;
        }
      });
    }
  }

  formatDate(date: Date): string {
    return new Date(date).toLocaleDateString();
  }

  goBack(): void {
    this.router.navigate(['/admin/attendance']);
  }
}
