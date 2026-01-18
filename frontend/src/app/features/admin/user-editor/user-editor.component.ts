import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';

@Component({
  selector: 'app-user-editor',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule],
  templateUrl: './user-editor.component.html',
})
export class UserEditorComponent implements OnInit {
  userForm!: FormGroup;
  isEditMode = false;
  isSubmitting = signal(false);
  successMessage = signal('');
  errorMessage = signal('');

  constructor(private fb: FormBuilder) {
    this.initializeForm();
  }

  ngOnInit(): void {
    // Load user data if editing
  }

  private initializeForm(): void {
    this.userForm = this.fb.group({
      username: ['', [Validators.required, Validators.minLength(3)]],
      email: ['', [Validators.required, Validators.email]],
      fullName: ['', Validators.required],
      role: ['Employee', Validators.required],
      managerId: [''],
      password: ['', this.isEditMode ? [] : Validators.required],
      isActive: [true]
    });
  }

  onSubmit(): void {
    if (this.userForm.invalid) return;

    this.isSubmitting.set(true);
    console.log('Form value:', this.userForm.value);
    
    setTimeout(() => {
      this.isSubmitting.set(false);
      this.successMessage.set('User saved successfully!');
    }, 1000);
  }

  onCancel(): void {
    this.userForm.reset();
    this.successMessage.set('');
    this.errorMessage.set('');
  }
}
