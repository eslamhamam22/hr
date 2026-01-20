import { Component, Input, Output, EventEmitter, OnChanges, SimpleChanges } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Department, CreateDepartmentDto, UpdateDepartmentDto } from '../../../../../core/models/department.model';
import { DepartmentService } from '../../../../../core/services/department.service';

@Component({
    selector: 'app-department-modal',
    standalone: true,
    imports: [CommonModule, FormsModule, ReactiveFormsModule],
    templateUrl: './department-modal.component.html',
    styleUrls: ['./department-modal.component.scss']
})
export class DepartmentModalComponent implements OnChanges {
    @Input() isOpen = false;
    @Input() department: Department | null = null;
    @Input() mode: 'create' | 'edit' = 'create';

    @Output() saved = new EventEmitter<void>();
    @Output() cancelled = new EventEmitter<void>();

    departmentForm!: FormGroup;
    isSubmitting = false;
    errorMessage = '';

    constructor(
        private fb: FormBuilder,
        private departmentService: DepartmentService
    ) {
        this.initForm();
    }

    ngOnChanges(changes: SimpleChanges): void {
        if (changes['isOpen'] && this.isOpen) {
            this.resetForm();
            if (this.mode === 'edit' && this.department) {
                this.populateForm();
            }
        }
    }

    private initForm(): void {
        this.departmentForm = this.fb.group({
            name: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(100)]],
            description: ['', Validators.maxLength(500)],
            isActive: [true]
        });
    }

    private resetForm(): void {
        this.departmentForm.reset({
            name: '',
            description: '',
            isActive: true
        });
        this.errorMessage = '';
        this.isSubmitting = false;
    }

    private populateForm(): void {
        if (this.department) {
            this.departmentForm.patchValue({
                name: this.department.name,
                description: this.department.description || '',
                isActive: this.department.isActive
            });
        }
    }

    get title(): string {
        return this.mode === 'create' ? 'Add New Department' : 'Edit Department';
    }

    get submitButtonText(): string {
        if (this.isSubmitting) {
            return this.mode === 'create' ? 'Creating...' : 'Saving...';
        }
        return this.mode === 'create' ? 'Create Department' : 'Save Changes';
    }

    onSubmit(): void {
        if (this.departmentForm.invalid) {
            this.departmentForm.markAllAsTouched();
            return;
        }

        this.isSubmitting = true;
        this.errorMessage = '';

        const formValue = this.departmentForm.value;

        if (this.mode === 'create') {
            const createDto: CreateDepartmentDto = {
                name: formValue.name,
                description: formValue.description || undefined,
                isActive: formValue.isActive
            };

            this.departmentService.createDepartment(createDto).subscribe({
                next: () => {
                    this.isSubmitting = false;
                    this.saved.emit();
                },
                error: (error) => {
                    this.isSubmitting = false;
                    this.errorMessage = error.error?.message || 'Failed to create department. Please try again.';
                }
            });
        } else if (this.department) {
            const updateDto: UpdateDepartmentDto = {
                name: formValue.name,
                description: formValue.description || undefined,
                isActive: formValue.isActive
            };

            this.departmentService.updateDepartment(this.department.id, updateDto).subscribe({
                next: () => {
                    this.isSubmitting = false;
                    this.saved.emit();
                },
                error: (error) => {
                    this.isSubmitting = false;
                    this.errorMessage = error.error?.message || 'Failed to update department. Please try again.';
                }
            });
        }
    }

    onCancel(): void {
        this.cancelled.emit();
    }

    isFieldInvalid(fieldName: string): boolean {
        const field = this.departmentForm.get(fieldName);
        return field ? field.invalid && field.touched : false;
    }

    getFieldError(fieldName: string): string {
        const field = this.departmentForm.get(fieldName);
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
            name: 'Department Name',
            description: 'Description'
        };
        return labels[fieldName] || fieldName;
    }
}
