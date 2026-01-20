import { Component, Input, Output, EventEmitter, OnChanges, SimpleChanges } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { RequestService } from '../../../../core/services/request.service';
import { LeaveType, getLeaveTypeOptions } from '../../../../core/models/request.model';

@Component({
    selector: 'app-request-modal',
    standalone: true,
    imports: [CommonModule, FormsModule, ReactiveFormsModule],
    templateUrl: './request-modal.component.html',
    styleUrls: ['./request-modal.component.scss']
})
export class RequestModalComponent implements OnChanges {
    @Input() isOpen = false;

    @Output() saved = new EventEmitter<void>();
    @Output() cancelled = new EventEmitter<void>();

    requestForm!: FormGroup;
    isSubmitting = false;
    errorMessage = '';

    // Leave type options from enum
    leaveTypeOptions = getLeaveTypeOptions();

    // Min date for validation (today)
    minDate = new Date().toISOString().split('T')[0];

    constructor(
        private fb: FormBuilder,
        private requestService: RequestService
    ) {
        this.initForm();
    }

    ngOnChanges(changes: SimpleChanges): void {
        if (changes['isOpen'] && this.isOpen) {
            this.resetForm();
        }
    }

    private initForm(): void {
        this.requestForm = this.fb.group({
            leaveTypeId: [LeaveType.Vacation, Validators.required],
            startDate: ['', Validators.required],
            endDate: ['', Validators.required],
            reason: ['', [Validators.required, Validators.minLength(10), Validators.maxLength(500)]]
        });
    }

    private resetForm(): void {
        const today = new Date().toISOString().split('T')[0];
        this.requestForm.reset({
            leaveTypeId: LeaveType.Vacation,
            startDate: today,
            endDate: today,
            reason: ''
        });
        this.errorMessage = '';
        this.isSubmitting = false;
    }

    get title(): string {
        return 'New Leave Request';
    }

    get submitButtonText(): string {
        return this.isSubmitting ? 'Submitting...' : 'Submit Request';
    }

    onSubmit(): void {
        if (this.requestForm.invalid) {
            this.requestForm.markAllAsTouched();
            return;
        }

        // Validate date range
        const startDate = new Date(this.requestForm.value.startDate);
        const endDate = new Date(this.requestForm.value.endDate);

        if (endDate < startDate) {
            this.errorMessage = 'End date cannot be before start date';
            return;
        }

        this.isSubmitting = true;
        this.errorMessage = '';

        const formValue = this.requestForm.value;

        this.requestService.createLeaveRequest({
            leaveTypeId: formValue.leaveTypeId,
            startDate: formValue.startDate,
            endDate: formValue.endDate,
            reason: formValue.reason
        }).subscribe({
            next: (response) => {
                // Auto-submit the request after creation
                this.requestService.submitRequest(response.id).subscribe({
                    next: () => {
                        this.isSubmitting = false;
                        this.saved.emit();
                    },
                    error: (error) => {
                        this.isSubmitting = false;
                        this.errorMessage = error.error?.message || 'Request created but failed to submit. Please try again.';
                    }
                });
            },
            error: (error) => {
                this.isSubmitting = false;
                this.errorMessage = error.error?.message || 'Failed to create request. Please try again.';
            }
        });
    }

    onCancel(): void {
        this.cancelled.emit();
    }

    isFieldInvalid(fieldName: string): boolean {
        const field = this.requestForm.get(fieldName);
        return field ? field.invalid && field.touched : false;
    }

    getFieldError(fieldName: string): string {
        const field = this.requestForm.get(fieldName);
        if (field?.errors) {
            if (field.errors['required']) return `${this.getFieldLabel(fieldName)} is required`;
            if (field.errors['minlength']) {
                const minLength = field.errors['minlength'].requiredLength;
                return `${this.getFieldLabel(fieldName)} must be at least ${minLength} characters`;
            }
            if (field.errors['maxlength']) {
                const maxLength = field.errors['maxlength'].requiredLength;
                return `${this.getFieldLabel(fieldName)} cannot exceed ${maxLength} characters`;
            }
        }
        return '';
    }

    private getFieldLabel(fieldName: string): string {
        const labels: { [key: string]: string } = {
            leaveTypeId: 'Leave Type',
            startDate: 'Start Date',
            endDate: 'End Date',
            reason: 'Reason'
        };
        return labels[fieldName] || fieldName;
    }
}
