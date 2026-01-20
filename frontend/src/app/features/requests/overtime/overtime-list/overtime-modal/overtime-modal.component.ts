import { Component, Input, Output, EventEmitter, OnChanges, SimpleChanges } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { OvertimeService } from '../../../../../core/services/overtime.service';

@Component({
    selector: 'app-overtime-modal',
    standalone: true,
    imports: [CommonModule, FormsModule, ReactiveFormsModule],
    templateUrl: './overtime-modal.component.html',
    styleUrls: ['./overtime-modal.component.scss']
})
export class OvertimeModalComponent implements OnChanges {
    @Input() isOpen = false;

    @Output() saved = new EventEmitter<void>();
    @Output() cancelled = new EventEmitter<void>();

    overtimeForm!: FormGroup;
    isSubmitting = false;
    errorMessage = '';
    calculatedHours = 0;

    // Min datetime for validation
    minDate = new Date().toISOString().slice(0, 16);

    constructor(
        private fb: FormBuilder,
        private overtimeService: OvertimeService
    ) {
        this.initForm();
    }

    ngOnChanges(changes: SimpleChanges): void {
        if (changes['isOpen'] && this.isOpen) {
            this.resetForm();
        }
    }

    private initForm(): void {
        this.overtimeForm = this.fb.group({
            startDateTime: ['', Validators.required],
            endDateTime: ['', Validators.required],
            reason: ['', [Validators.required, Validators.minLength(10), Validators.maxLength(500)]]
        });

        // Calculate hours when dates change
        this.overtimeForm.get('startDateTime')?.valueChanges.subscribe(() => this.calculateHours());
        this.overtimeForm.get('endDateTime')?.valueChanges.subscribe(() => this.calculateHours());
    }

    private resetForm(): void {
        this.overtimeForm.reset({
            startDateTime: '',
            endDateTime: '',
            reason: ''
        });
        this.errorMessage = '';
        this.isSubmitting = false;
        this.calculatedHours = 0;
    }

    private calculateHours(): void {
        const start = this.overtimeForm.get('startDateTime')?.value;
        const end = this.overtimeForm.get('endDateTime')?.value;

        if (start && end) {
            const startDate = new Date(start);
            const endDate = new Date(end);
            const diffMs = endDate.getTime() - startDate.getTime();
            this.calculatedHours = Math.max(0, Math.round((diffMs / (1000 * 60 * 60)) * 100) / 100);
        } else {
            this.calculatedHours = 0;
        }
    }

    get title(): string {
        return 'New Overtime Request';
    }

    get submitButtonText(): string {
        return this.isSubmitting ? 'Submitting...' : 'Submit Request';
    }

    onSubmit(): void {
        if (this.overtimeForm.invalid) {
            this.overtimeForm.markAllAsTouched();
            return;
        }

        const startDateTime = new Date(this.overtimeForm.value.startDateTime);
        const endDateTime = new Date(this.overtimeForm.value.endDateTime);

        if (endDateTime <= startDateTime) {
            this.errorMessage = 'End time must be after start time';
            return;
        }

        if (this.calculatedHours <= 0) {
            this.errorMessage = 'Invalid time range';
            return;
        }

        this.isSubmitting = true;
        this.errorMessage = '';

        const formValue = this.overtimeForm.value;

        this.overtimeService.createOvertime({
            startDateTime: new Date(formValue.startDateTime),
            endDateTime: new Date(formValue.endDateTime),
            hoursWorked: this.calculatedHours,
            reason: formValue.reason
        }).subscribe({
            next: (response) => {
                // Auto-submit the request after creation
                this.overtimeService.submitOvertime(response.id).subscribe({
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
        const field = this.overtimeForm.get(fieldName);
        return field ? field.invalid && field.touched : false;
    }

    getFieldError(fieldName: string): string {
        const field = this.overtimeForm.get(fieldName);
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
            startDateTime: 'Start Date & Time',
            endDateTime: 'End Date & Time',
            reason: 'Reason'
        };
        return labels[fieldName] || fieldName;
    }
}
