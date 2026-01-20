import { Component, Input, Output, EventEmitter, OnChanges, SimpleChanges } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { User } from '../../../../core/models/user.model';
import { DepartmentService } from '../../../../core/services/department.service';
import { UserService } from '../../../../core/services/user.service';
import { Department } from '../../../../core/models/department.model';
import { RoleType, getRoleOptions } from '../../../../core/models/role-type.enum';

@Component({
    selector: 'app-user-modal',
    standalone: true,
    imports: [CommonModule, FormsModule, ReactiveFormsModule],
    templateUrl: './user-modal.component.html',
    styleUrls: ['./user-modal.component.scss']
})
export class UserModalComponent implements OnChanges {
    @Input() isOpen = false;
    @Input() user: User | null = null;
    @Input() mode: 'create' | 'edit' = 'create';

    @Output() saved = new EventEmitter<void>();
    @Output() cancelled = new EventEmitter<void>();

    userForm!: FormGroup;
    departments: Department[] = [];
    managers: User[] = [];
    isSubmitting = false;
    errorMessage = '';

    // Role options from enum
    roleOptions = getRoleOptions();

    constructor(
        private fb: FormBuilder,
        private departmentService: DepartmentService,
        private userService: UserService
    ) {
        this.initForm();
        this.loadDepartments();
        this.loadManagers();
    }

    ngOnChanges(changes: SimpleChanges): void {
        if (changes['isOpen'] && this.isOpen) {
            this.resetForm();
            if (this.mode === 'edit' && this.user) {
                this.populateForm();
            }
        }
    }

    private initForm(): void {
        this.userForm = this.fb.group({
            username: ['', [Validators.required, Validators.minLength(3)]],
            fullName: ['', [Validators.required, Validators.minLength(2)]],
            email: ['', [Validators.required, Validators.email]],
            role: [RoleType.Employee, Validators.required],
            departmentId: [''],
            managerId: [''],
            password: ['', this.mode === 'create' ? [Validators.required, Validators.minLength(6)] : []],
            isActive: [true]
        });
    }

    private resetForm(): void {
        this.userForm.reset({
            username: '',
            fullName: '',
            email: '',
            role: RoleType.Employee,
            departmentId: '',
            managerId: '',
            password: '',
            isActive: true
        });
        this.errorMessage = '';
        this.isSubmitting = false;
        this.userForm.get('username')?.enable();

        // Update password validators based on mode
        const passwordControl = this.userForm.get('password');
        if (this.mode === 'create') {
            passwordControl?.setValidators([Validators.required, Validators.minLength(6)]);
        } else {
            passwordControl?.clearValidators();
        }
        passwordControl?.updateValueAndValidity();
    }

    private populateForm(): void {
        if (this.user) {
            this.userForm.patchValue({
                username: this.user.username,
                fullName: this.user.fullName,
                email: this.user.email,
                role: this.user.role,
                departmentId: this.user.departmentId || '',
                managerId: this.user.managerId || '',
                isActive: this.user.isActive
            });
            // Disable username editing in edit mode
            this.userForm.get('username')?.disable();
        }
    }

    private loadDepartments(): void {
        this.departmentService.getDepartments(1, 100).subscribe({
            next: (response) => {
                this.departments = response.items.filter(d => d.isActive);
            },
            error: (error) => {
                console.error('Error loading departments:', error);
            }
        });
    }

    private loadManagers(): void {
        this.userService.getUsers(1, 100).subscribe({
            next: (response) => {
                this.managers = response.items.filter(u =>
                    u.role === RoleType.Manager || u.role === RoleType.HR || u.role === RoleType.Admin
                );
            },
            error: (error) => {
                console.error('Error loading managers:', error);
            }
        });
    }

    get title(): string {
        return this.mode === 'create' ? 'Add New User' : 'Edit User';
    }

    get submitButtonText(): string {
        if (this.isSubmitting) {
            return this.mode === 'create' ? 'Creating...' : 'Saving...';
        }
        return this.mode === 'create' ? 'Create User' : 'Save Changes';
    }

    onSubmit(): void {
        if (this.userForm.invalid) {
            this.userForm.markAllAsTouched();
            return;
        }

        this.isSubmitting = true;
        this.errorMessage = '';

        const formValue = this.userForm.getRawValue();

        if (this.mode === 'create') {
            this.userService.createUser({
                username: formValue.username,
                fullName: formValue.fullName,
                email: formValue.email,
                role: formValue.role,
                departmentId: formValue.departmentId || undefined,
                managerId: formValue.managerId || undefined,
                password: formValue.password
            }).subscribe({
                next: () => {
                    this.isSubmitting = false;
                    this.saved.emit();
                },
                error: (error) => {
                    this.isSubmitting = false;
                    this.errorMessage = error.error?.message || 'Failed to create user. Please try again.';
                }
            });
        } else if (this.user) {
            this.userService.updateUser(this.user.id, {
                fullName: formValue.fullName,
                email: formValue.email,
                role: formValue.role,
                departmentId: formValue.departmentId || undefined,
                managerId: formValue.managerId || undefined,
                password: formValue.password || undefined,
                isActive: formValue.isActive
            }).subscribe({
                next: () => {
                    this.isSubmitting = false;
                    this.saved.emit();
                },
                error: (error) => {
                    this.isSubmitting = false;
                    this.errorMessage = error.error?.message || 'Failed to update user. Please try again.';
                }
            });
        }
    }

    onCancel(): void {
        this.cancelled.emit();
    }

    isFieldInvalid(fieldName: string): boolean {
        const field = this.userForm.get(fieldName);
        return field ? field.invalid && field.touched : false;
    }

    getFieldError(fieldName: string): string {
        const field = this.userForm.get(fieldName);
        if (field?.errors) {
            if (field.errors['required']) return `${this.getFieldLabel(fieldName)} is required`;
            if (field.errors['email']) return 'Please enter a valid email';
            if (field.errors['minlength']) {
                const minLength = field.errors['minlength'].requiredLength;
                return `${this.getFieldLabel(fieldName)} must be at least ${minLength} characters`;
            }
        }
        return '';
    }

    private getFieldLabel(fieldName: string): string {
        const labels: { [key: string]: string } = {
            username: 'Username',
            fullName: 'Full Name',
            email: 'Email',
            role: 'Role',
            password: 'Password'
        };
        return labels[fieldName] || fieldName;
    }
}
