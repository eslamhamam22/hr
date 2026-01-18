import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { ApprovalLogService } from '../../../../core/services/approval-log.service';
import { ApprovalLog } from '../../../../core/models/approval-log.model';

@Component({
  selector: 'app-approval-log-detail',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './approval-log-detail.component.html',
  styleUrls: ['./approval-log-detail.component.scss']
})
export class ApprovalLogDetailComponent implements OnInit {
  approvalLog: ApprovalLog | null = null;
  loading: boolean = true;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private approvalLogService: ApprovalLogService
  ) { }

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.approvalLogService.getApprovalLogById(id).subscribe({
        next: (log) => {
          this.approvalLog = log;
          this.loading = false;
        },
        error: () => {
          this.loading = false;
        }
      });
    }
  }

  formatDateTime(date: Date): string {
    return new Date(date).toLocaleString();
  }

  goBack(): void {
    this.router.navigate(['/admin/approval-logs']);
  }
}
