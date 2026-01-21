import { Component, EventEmitter, Input, Output, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { WorkFromHomeService } from '../../../../core/services/work-from-home.service';
import { CreateWorkFromHomeRequest } from '../../../../core/models/work-from-home.model';

@Component({
    selector: 'app-create-wfh-modal',
    standalone: true,
    imports: [CommonModule, FormsModule],
    templateUrl: './create-wfh-modal.component.html',
    styleUrls: ['./create-wfh-modal.component.scss']
})
export class CreateWfhModalComponent {
    @Input() isOpen = false;
    @Output() saved = new EventEmitter<void>();
    @Output() cancelled = new EventEmitter<void>();

    fromDate = '';
    toDate = '';
    reason = '';

    isSubmitting = signal(false);
    errorMessage = signal('');

    constructor(private wfhService: WorkFromHomeService) { }

    onSave(): void {
        if (!this.fromDate || !this.toDate || !this.reason) {
            this.errorMessage.set('Please fill in all fields');
            return;
        }

        // Basic date validation
        if (new Date(this.fromDate) > new Date(this.toDate)) {
            this.errorMessage.set('From Date cannot be later than To Date');
            return;
        }

        this.isSubmitting.set(true);
        this.errorMessage.set('');

        const request: CreateWorkFromHomeRequest = {
            fromDate: this.fromDate,
            toDate: this.toDate,
            reason: this.reason
        };

        this.wfhService.createWorkFromHomeRequest(request).subscribe({
            next: () => {
                this.isSubmitting.set(false);
                this.resetForm();
                this.saved.emit();
            },
            error: (error) => {
                this.isSubmitting.set(false);
                this.errorMessage.set('Failed to create request');
                console.error('Error creating WFH request:', error);
            }
        });
    }

    onCancel(): void {
        this.resetForm();
        this.cancelled.emit();
    }

    private resetForm(): void {
        this.fromDate = '';
        this.toDate = '';
        this.reason = '';
        this.errorMessage.set('');
    }
}
