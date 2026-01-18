import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

interface User {
  id: string;
  username: string;
  fullName: string;
  email: string;
  role: string;
  managerId?: string;
  isActive: boolean;
}

@Component({
  selector: 'app-user-list',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './user-list.component.html',
  styleUrls: ['./user-list.component.scss']
})
export class UserListComponent implements OnInit {
  users = signal<User[]>([]);
  filteredUsers = signal<User[]>([]);
  isLoading = signal(false);
  searchTerm = '';

  ngOnInit(): void {
    this.loadUsers();
  }

  loadUsers(): void {
    this.isLoading.set(true);
    // Mock data
    const mockUsers: User[] = [
      {
        id: '1',
        username: 'john.doe',
        fullName: 'John Doe',
        email: 'john.doe@example.com',
        role: 'Employee',
        managerId: '2',
        isActive: true
      },
      {
        id: '2',
        username: 'jane.smith',
        fullName: 'Jane Smith',
        email: 'jane.smith@example.com',
        role: 'Manager',
        isActive: true
      }
    ];
    this.users.set(mockUsers);
    this.filteredUsers.set(mockUsers);
    this.isLoading.set(false);
  }

  onSearch(): void {
    const term = this.searchTerm.toLowerCase();
    this.filteredUsers.set(
      this.users().filter(
        u =>
          u.username.toLowerCase().includes(term) ||
          u.fullName.toLowerCase().includes(term) ||
          u.email.toLowerCase().includes(term)
      )
    );
  }

  editUser(user: User): void {
    console.log('Edit user:', user);
  }

  deleteUser(userId: string): void {
    if (confirm('Are you sure you want to delete this user?')) {
      this.users.update(users => users.filter(u => u.id !== userId));
      this.onSearch();
    }
  }

  openCreateDialog(): void {
    console.log('Open create user dialog');
  }
}
