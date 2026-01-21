import { Component, Input, Output, EventEmitter, OnChanges, SimpleChanges } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { TimeOffService } from '../../../../../core/services/time-off.service';

@Component({
    selector: 'app-time-off-modal',
    standalone: true,
    imports: [CommonModule, FormsModule, ReactiveFormsModule],
    templateUrl: './time-off-modal.component.html',
    styles: [`
        .modal-overlay { position: fixed; top: 0; left: 0; width: 100%; height: 100%; background: rgba(0, 0, 0, 0.5); z-index: 50; display: flex; align-items: center; justify-content: center; opacity: 1; transition: opacity 0.3s ease; }
        .modal-content { background: white; border-radius: 8px; width: 100%; max-width: 600px; max-height: 90vh; overflow-y: auto; box-shadow: 0 4px 6px -1px rgba(0, 0, 0, 0.1), 0 2px 4px -1px rgba(0, 0, 0, 0.06); animation: slideIn 0.2s ease-out; }
        @keyframes slideIn { from { transform: translateY(-20px); opacity: 0; } to { transform: translateY(0); opacity: 1; } }
        .modal-header { display: flex; justify-content: space-between; align-items: center; padding: 20px 24px; border-bottom: 1px solid #e5e7eb; }
        .modal-header h2 { font-size: 1.25rem; font-weight: 600; color: #1f2937; margin: 0; }
        .btn-close { background: none; border: none; font-size: 1.5rem; color: #9ca3af; cursor: pointer; padding: 4px; transition: color 0.2s; }
        .btn-close:hover { color: #4b5563; }
        .modal-body { padding: 24px; }
        .modal-footer { padding: 20px 24px; background-color: #f9fafb; border-top: 1px solid #e5e7eb; display: flex; justify-content: flex-end; gap: 12px; }
        .form-row { display: grid; grid-template-columns: 1fr 1fr; gap: 20px; }
        .form-group { margin-bottom: 20px; }
        label { display: block; margin-bottom: 8px; font-weight: 500; color: #374151; font-size: 14px; }
        .required { color: #ef4444; }
        input, textarea { width: 100%; padding: 10px; border: 1px solid #d1d5db; border-radius: 6px; font-size: 14px; transition: border-color 0.2s; }
        input:focus, textarea:focus { outline: none; border-color: #3b82f6; ring: 2px solid #93c5fd; }
        .invalid { border-color: #ef4444; }
        .error-text { color: #ef4444; font-size: 12px; margin-top: 4px; display: block; }
        .char-count { display: block; text-align: right; font-size: 12px; color: #9ca3af; margin-top: 4px; }
        .alert-error { background-color: #fee2e2; border: 1px solid #fecaca; color: #b91c1c; padding: 12px; border-radius: 6px; margin-bottom: 20px; font-size: 14px; }
        .btn { padding: 10px 20px; border-radius: 6px; font-weight: 500; font-size: 14px; cursor: pointer; transition: all 0.2s; }
        .btn-secondary { background: white; border: 1px solid #d1d5db; color: #374151; }
        .btn-secondary:hover { background: #f9fafb; }
        .btn-primary { background: #2563eb; border: 1px solid #2563eb; color: white; }
        .btn-primary:hover { background: #1d4ed8; }
        .btn-primary:disabled { opacity: 0.7; cursor: not-allowed; }
        .info-box { background-color: #eff6ff; border: 1px solid #dbeafe; color: #1e40af; padding: 12px; border-radius: 6px; margin-bottom: 20px; font-size: 14px; display: flex; align-items: center; gap: 8px; }
    `]
})
export class TimeOffModalComponent implements OnChanges {
    @Input() isOpen = false;

    @Output() saved = new EventEmitter<void>();
    @Output() cancelled = new EventEmitter<void>();

    timeOffForm!: FormGroup;
    isSubmitting = false;
    errorMessage = '';

    constructor(
        private fb: FormBuilder,
        private timeOffService: TimeOffService
    ) {
        this.initForm();
    }

    ngOnChanges(changes: SimpleChanges): void {
        if (changes['isOpen'] && this.isOpen) {
            this.resetForm();
        }
    }

    private initForm(): void {
        this.timeOffForm = this.fb.group({
            date: ['', Validators.required],
            startTime: ['', Validators.required],
            reason: ['', [Validators.required, Validators.minLength(5), Validators.maxLength(500)]]
        });
    }

    private resetForm(): void {
        this.timeOffForm.reset({
            date: '',
            startTime: '',
            reason: ''
        });
        this.errorMessage = '';
        this.isSubmitting = false;
    }

    get title(): string {
        return 'New Time Off Request';
    }

    get submitButtonText(): string {
        return this.isSubmitting ? 'Submitting...' : 'Submit Request';
    }

    onSubmit(): void {
        if (this.timeOffForm.invalid) {
            this.timeOffForm.markAllAsTouched();
            return;
        }

        this.isSubmitting = true;
        this.errorMessage = '';

        const formValue = this.timeOffForm.value;
        let startTime = formValue.startTime;
        if (startTime.length === 5) {
            startTime += ':00';
        }

        this.timeOffService.createTimeOff({
            date: formValue.date,
            startTime: startTime,
            reason: formValue.reason
        }).subscribe({
            next: (response) => {
                // Auto-submit
                this.timeOffService.submitTimeOff(response.id).subscribe({
                    next: () => {
                        this.isSubmitting = false;
                        this.saved.emit();
                    },
                    error: (error) => {
                        // Even if submit fails, the request is created.
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
        const field = this.timeOffForm.get(fieldName);
        return field ? field.invalid && field.touched : false;
    }

    getFieldError(fieldName: string): string {
        const field = this.timeOffForm.get(fieldName);
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
            date: 'Date',
            startTime: 'Start Time',
            reason: 'Reason'
        };
        return labels[fieldName] || fieldName;
    }
}
