import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormGroup, FormControl, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { OvertimeService } from '../../../../core/services/overtime.service';

@Component({
    selector: 'app-overtime-form',
    standalone: true,
    imports: [CommonModule, ReactiveFormsModule],
    templateUrl: './overtime-form.component.html',
    styleUrls: ['./overtime-form.component.scss']
})
export class OvertimeFormComponent implements OnInit {
    overtimeForm!: FormGroup;
    submitting: boolean = false;
    errorMessage: string = '';

    constructor(
        private overtimeService: OvertimeService,
        private router: Router
    ) { }

    ngOnInit(): void {
        this.initializeForm();
    }

    initializeForm(): void {
        this.overtimeForm = new FormGroup({
            startDateTime: new FormControl('', [Validators.required]),
            endDateTime: new FormControl('', [Validators.required]),
            reason: new FormControl('', [Validators.required, Validators.minLength(10)])
        });

        // Calculate hours when dates change
        this.overtimeForm.get('startDateTime')?.valueChanges.subscribe(() => this.calculateHours());
        this.overtimeForm.get('endDateTime')?.valueChanges.subscribe(() => this.calculateHours());
    }

    calculateHours(): void {
        const start = this.overtimeForm.get('startDateTime')?.value;
        const end = this.overtimeForm.get('endDateTime')?.value;

        if (start && end) {
            const startDate = new Date(start);
            const endDate = new Date(end);
            const hours = (endDate.getTime() - startDate.getTime()) / (1000 * 60 * 60);

            if (hours > 0) {
                this.overtimeForm.patchValue({ hoursWorked: hours }, { emitEvent: false });
            }
        }
    }

    get hoursWorked(): number {
        const start = this.overtimeForm.get('startDateTime')?.value;
        const end = this.overtimeForm.get('endDateTime')?.value;

        if (start && end) {
            const startDate = new Date(start);
            const endDate = new Date(end);
            const hours = (endDate.getTime() - startDate.getTime()) / (1000 * 60 * 60);
            return hours > 0 ? Math.round(hours * 10) / 10 : 0;
        }
        return 0;
    }

    onSubmit(): void {
        if (this.overtimeForm.invalid) {
            this.overtimeForm.markAllAsTouched();
            return;
        }

        this.submitting = true;
        this.errorMessage = '';

        const formValue = this.overtimeForm.value;
        const overtimeRequest = {
            ...formValue,
            hoursWorked: this.hoursWorked
        };

        this.overtimeService.createOvertime(overtimeRequest).subscribe({
            next: (response) => {
                this.router.navigate(['/requests/overtime', response.id]);
            },
            error: (error) => {
                console.error('Error creating overtime request:', error);
                this.errorMessage = error.error?.message || 'Failed to create overtime request. Please try again.';
                this.submitting = false;
            }
        });
    }

    cancel(): void {
        this.router.navigate(['/requests/overtime']);
    }
}
