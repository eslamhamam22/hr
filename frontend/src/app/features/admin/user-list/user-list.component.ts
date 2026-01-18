import { Component, OnInit, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { UserService } from '../../../core/services/user.service';
import { User } from '../../../core/models/user.model';
import { DataTableComponent, DataTableColumn, PaginationConfig } from '../../../shared/components/data-table/data-table.component';
import { Subject, debounceTime, distinctUntilChanged } from 'rxjs';

@Component({
  selector: 'app-user-list',
  standalone: true,
  imports: [CommonModule, DataTableComponent],
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
    debugger;
    this.isLoading.set(true);
    this.userService.getUsers(this.currentPage(), this.pageSize(), this.searchTerm)
      .subscribe({
        next: (response) => {
    debugger;
          this.users.set(response.items);
          this.totalItems.set(response.totalCount);
          this.isLoading.set(false);
        },
        error: (error) => {
    debugger;
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

  editUser(user: User): void {
    console.log('Edit user:', user);
  }

  deleteUser(user: User): void {
    if (confirm('Are you sure you want to delete this user?')) {
      this.userService.deleteUser(user.id).subscribe({
        next: () => {
          this.loadUsers();
        },
        error: (error) => {
          console.error('Error deleting user:', error);
          alert('Failed to delete user');
        }
      });
    }
  }

  openCreateDialog(): void {
    console.log('Open create user dialog');
  }
}
