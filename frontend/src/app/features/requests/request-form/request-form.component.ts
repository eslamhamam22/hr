import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { RequestService } from '../../../core/services/request.service';

@Component({
  selector: 'app-request-form',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule],
  templateUrl: './request-form.component.html',
  styleUrls: ['./request-form.component.scss']
})
export class RequestFormComponent implements OnInit {
  requestForm!: FormGroup;
  isSubmitting = signal(false);
  successMessage = signal('');
  errorMessage = signal('');

  constructor(
    private fb: FormBuilder,
    private requestService: RequestService
  ) {
    this.initializeForm();
  }

  ngOnInit(): void {
    // Load initial data if needed
  }

  private initializeForm(): void {
    this.requestForm = this.fb.group({
      leaveTypeId: ['', Validators.required],
      startDate: ['', Validators.required],
      endDate: ['', Validators.required],
      reason: ['', [Validators.required, Validators.maxLength(500)]]
    });
  }

  onSubmit(): void {
    if (this.requestForm.invalid) return;

    this.isSubmitting.set(true);
    this.errorMessage.set('');
    this.successMessage.set('');

    const formValue = this.requestForm.value;
    this.requestService.createLeaveRequest(formValue).subscribe({
      next: () => {
        debugger;
        this.successMessage.set('Leave request submitted successfully!');
        this.requestForm.reset();
        this.isSubmitting.set(false);
      },
      error: (error) => {
        debugger;
        this.errorMessage.set('Failed to submit request. Please try again.');
        this.isSubmitting.set(false);
      }
    });
  }

  onCancel(): void {
    this.requestForm.reset();
    this.successMessage.set('');
    this.errorMessage.set('');
  }
}
