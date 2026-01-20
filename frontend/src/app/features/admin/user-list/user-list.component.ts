import { Component, OnInit, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { UserService } from '../../../core/services/user.service';
import { User } from '../../../core/models/user.model';
import { DataTableComponent, DataTableColumn, PaginationConfig } from '../../../shared/components/data-table/data-table.component';
import { ConfirmDialogComponent } from '../../../shared/components/confirm-dialog/confirm-dialog.component';
import { UserModalComponent } from './user-modal/user-modal.component';
import { Subject, debounceTime, distinctUntilChanged } from 'rxjs';
import { getRoleLabel } from '../../../core/models/role-type.enum';

@Component({
  selector: 'app-user-list',
  standalone: true,
  imports: [CommonModule, DataTableComponent, ConfirmDialogComponent, UserModalComponent],
  templateUrl: './user-list.component.html',
  styleUrls: ['./user-list.component.scss']
})
export class UserListComponent implements OnInit {
  users = signal<User[]>([]);
  isLoading = signal(false);

  // Pagination
  currentPage = signal(1);
  pageSize = signal(10);
  totalItems = signal(0);

  paginationConfig = computed<PaginationConfig>(() => ({
    currentPage: this.currentPage(),
    pageSize: this.pageSize(),
    totalItems: this.totalItems()
  }));

  // Columns
  columns: DataTableColumn[] = [
    { header: 'Username', field: 'username', sortable: true },
    { header: 'Full Name', field: 'fullName', sortable: true },
    { header: 'Email', field: 'email', sortable: true },
    { header: 'Role', field: 'role', sortable: true },
    { header: 'Manager', field: 'managerName', sortable: false },
    { header: 'Department', field: 'departmentName', sortable: false },
    { header: 'Status', field: 'status', sortable: false },
    { header: 'Actions', field: 'actions', sortable: false }
  ];

  // Search
  searchTerm = '';
  private searchSubject = new Subject<string>();

  // Modal state
  isModalOpen = false;
  modalMode: 'create' | 'edit' = 'create';
  selectedUser: User | null = null;

  // Delete confirmation dialog state
  isDeleteDialogOpen = false;
  userToDelete: User | null = null;

  constructor(private userService: UserService) {
    this.searchSubject.pipe(
      debounceTime(300),
      distinctUntilChanged()
    ).subscribe(term => {
      this.searchTerm = term;
      this.currentPage.set(1);
      this.loadUsers();
    });
  }

  ngOnInit(): void {
    this.loadUsers();
  }

  loadUsers(): void {
    this.isLoading.set(true);
    this.userService.getUsers(this.currentPage(), this.pageSize(), this.searchTerm)
      .subscribe({
        next: (response) => {
          this.users.set(response.items);
          this.totalItems.set(response.totalCount);
          this.isLoading.set(false);
        },
        error: (error) => {
          console.error('Error loading users:', error);
          this.isLoading.set(false);
        }
      });
  }

  onSearch(term: string): void {
    this.searchSubject.next(term);
  }

  onPageChange(page: number): void {
    this.currentPage.set(page);
    this.loadUsers();
  }

  onPageSizeChange(size: number): void {
    this.pageSize.set(size);
    this.currentPage.set(1);
    this.loadUsers();
  }

  // Get role label for display
  getRoleDisplayLabel(role: number): string {
    return getRoleLabel(role);
  }

  // Modal operations
  openCreateDialog(): void {
    this.modalMode = 'create';
    this.selectedUser = null;
    this.isModalOpen = true;
  }

  editUser(user: User): void {
    this.modalMode = 'edit';
    this.selectedUser = user;
    this.isModalOpen = true;
  }

  onModalSaved(): void {
    this.isModalOpen = false;
    this.selectedUser = null;
    this.loadUsers();
  }

  onModalCancelled(): void {
    this.isModalOpen = false;
    this.selectedUser = null;
  }

  // Delete operations
  deleteUser(user: User): void {
    this.userToDelete = user;
    this.isDeleteDialogOpen = true;
  }

  onDeleteConfirmed(): void {
    if (this.userToDelete) {
      this.userService.deleteUser(this.userToDelete.id).subscribe({
        next: () => {
          this.isDeleteDialogOpen = false;
          this.userToDelete = null;
          this.loadUsers();
        },
        error: (error) => {
          console.error('Error deleting user:', error);
          this.isDeleteDialogOpen = false;
          this.userToDelete = null;
          alert('Failed to delete user. Please try again.');
        }
      });
    }
  }

  onDeleteCancelled(): void {
    this.isDeleteDialogOpen = false;
    this.userToDelete = null;
  }
}
