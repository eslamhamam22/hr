import { Component, EventEmitter, Input, Output, OnChanges, SimpleChanges } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { WorkFromHomeService } from '../../../../core/services/work-from-home.service';

@Component({
    selector: 'app-create-wfh-modal',
    standalone: true,
    imports: [CommonModule, FormsModule, ReactiveFormsModule],
    templateUrl: './create-wfh-modal.component.html',
    styleUrls: ['./create-wfh-modal.component.scss']
})
export class CreateWfhModalComponent implements OnChanges {
    @Input() isOpen = false;
    @Output() saved = new EventEmitter<void>();
    @Output() cancelled = new EventEmitter<void>();

    wfhForm!: FormGroup;
    isSubmitting = false;
    errorMessage = '';
    calculatedDays = 0;

    constructor(
        private fb: FormBuilder,
        private wfhService: WorkFromHomeService
    ) {
        this.initForm();
    }

    ngOnChanges(changes: SimpleChanges): void {
        if (changes['isOpen'] && this.isOpen) {
            this.resetForm();
        }
    }

    private initForm(): void {
        this.wfhForm = this.fb.group({
            fromDate: ['', Validators.required],
            toDate: ['', Validators.required],
            reason: ['', [Validators.required, Validators.minLength(10), Validators.maxLength(500)]]
        });

        // Calculate days when dates change
        this.wfhForm.get('fromDate')?.valueChanges.subscribe(() => this.calculateDays());
        this.wfhForm.get('toDate')?.valueChanges.subscribe(() => this.calculateDays());
    }

    private resetForm(): void {
        this.wfhForm.reset({
            fromDate: '',
            toDate: '',
            reason: ''
        });
        this.errorMessage = '';
        this.isSubmitting = false;
        this.calculatedDays = 0;
    }

    private calculateDays(): void {
        const from = this.wfhForm.get('fromDate')?.value;
        const to = this.wfhForm.get('toDate')?.value;

        if (from && to) {
            const startDate = new Date(from);
            const endDate = new Date(to);

            // Normalize to start of day for accurate day calculation
            startDate.setHours(0, 0, 0, 0);
            endDate.setHours(0, 0, 0, 0);

            const diffMs = endDate.getTime() - startDate.getTime();
            // Add 1 day because it's inclusive (e.g. same day = 1 day)
            const days = Math.round(diffMs / (1000 * 60 * 60 * 24)) + 1;

            this.calculatedDays = Math.max(0, days);
        } else {
            this.calculatedDays = 0;
        }
    }

    get title(): string {
        return 'New Work From Home Request';
    }

    get submitButtonText(): string {
        return this.isSubmitting ? 'Submitting...' : 'Submit Request';
    }

    onSubmit(): void {
        if (this.wfhForm.invalid) {
            this.wfhForm.markAllAsTouched();
            return;
        }

        const fromDate = new Date(this.wfhForm.value.fromDate);
        const toDate = new Date(this.wfhForm.value.toDate);

        if (toDate < fromDate) {
            this.errorMessage = 'To Date must be at or after From Date';
            return;
        }

        if (this.calculatedDays <= 0) {
            this.errorMessage = 'Invalid date range';
            return;
        }

        this.isSubmitting = true;
        this.errorMessage = '';

        const formValue = this.wfhForm.value;

        this.wfhService.createWorkFromHomeRequest({
            fromDate: formValue.fromDate,
            toDate: formValue.toDate,
            reason: formValue.reason
        }).subscribe({
            next: (response) => {
                // Auto-submit the request after creation
                this.wfhService.submitWorkFromHomeRequest(response.id).subscribe({
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
        const field = this.wfhForm.get(fieldName);
        return field ? field.invalid && field.touched : false;
    }

    getFieldError(fieldName: string): string {
        const field = this.wfhForm.get(fieldName);
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
            fromDate: 'From Date',
            toDate: 'To Date',
            reason: 'Reason'
        };
        return labels[fieldName] || fieldName;
    }
}
