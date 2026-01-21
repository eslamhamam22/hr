import { Component, Input, Output, EventEmitter, OnChanges, SimpleChanges } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Request } from '../../../../core/models/request.model';
import { ApprovalLog } from '../../../../core/models/approval-log.model';
import { ApprovalLogService } from '../../../../core/services/approval-log.service';

@Component({
    selector: 'app-request-details-modal',
    standalone: true,
    imports: [CommonModule, FormsModule],
    templateUrl: './request-details-modal.component.html',
    styleUrls: ['./request-details-modal.component.scss']
})
export class RequestDetailsModalComponent implements OnChanges {
    @Input() request: Request | null = null;
    @Input() isOpen = false;
    @Output() close = new EventEmitter<void>();
    @Output() approve = new EventEmitter<string>();
    @Output() reject = new EventEmitter<{ id: string; reason: string }>();

    showRejectModal = false;
    rejectionReason = '';
    approvalLogs: ApprovalLog[] = [];

    constructor(private approvalLogService: ApprovalLogService) { }

    ngOnChanges(changes: SimpleChanges): void {
        if (changes['request'] && this.request && this.isOpen) {
            this.loadApprovalLogs();
        }
        if (changes['isOpen'] && this.isOpen && this.request) {
            this.loadApprovalLogs();
        }
    }

    loadApprovalLogs(): void {
        if (!this.request) return;

        this.approvalLogService.getApprovalLogsForRequest(this.request.id).subscribe({
            next: (logs) => {
                this.approvalLogs = logs;
            },
            error: (err) => {
                console.error('Error fetching approval logs', err);
                this.approvalLogs = [];
            }
        });
    }

    onClose(): void {
        this.close.emit();
        this.approvalLogs = [];
    }

    onApprove(): void {
        if (this.request) {
            this.approve.emit(this.request.id);
        }
    }

    onRejectClick(): void {
        this.showRejectModal = true;
    }

    onRejectConfirm(): void {
        if (this.request && this.rejectionReason.trim()) {
            this.reject.emit({ id: this.request.id, reason: this.rejectionReason });
            this.showRejectModal = false;
            this.rejectionReason = '';
        }
    }

    onRejectCancel(): void {
        this.showRejectModal = false;
        this.rejectionReason = '';
    }

    getStatusClass(status: string): string {
        const statusLower = status.toLowerCase();
        if (statusLower.includes('approved')) return 'status-approved';
        if (statusLower.includes('rejected')) return 'status-rejected';
        if (statusLower.includes('pending')) return 'status-pending';
        if (statusLower.includes('submitted')) return 'status-submitted';
        return 'status-draft';
    }
}
