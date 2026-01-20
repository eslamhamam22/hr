import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';
import { User } from '../models/user.model';
import { PaginatedResponse } from '../models/department.model';
import { RoleType } from '../models/role-type.enum';

export interface CreateUserDto {
    username: string;
    fullName: string;
    email: string;
    role: RoleType;
    managerId?: string;
    departmentId?: string;
    password?: string;
}

export interface UpdateUserDto {
    fullName: string;
    email: string;
    role: RoleType;
    managerId?: string;
    departmentId?: string;
    isActive: boolean;
    password?: string;
}

@Injectable({
    providedIn: 'root'
})
export class UserService {
    private apiUrl = '/users';

    constructor(private apiService: ApiService) { }

    getUsers(page: number = 1, pageSize: number = 10, search: string = ''): Observable<PaginatedResponse<User>> {
        const params: any = {
            page: page,
            pageSize: pageSize
        };

        if (search) {
            params.search = search;
        }

        return this.apiService.get<PaginatedResponse<User>>(this.apiUrl, params);
    }

    getUserById(id: string): Observable<User> {
        return this.apiService.get<User>(`${this.apiUrl}/${id}`);
    }

    createUser(user: CreateUserDto): Observable<User> {
        return this.apiService.post<User>(this.apiUrl, user);
    }

    updateUser(id: string, user: UpdateUserDto): Observable<void> {
        return this.apiService.put<void>(`${this.apiUrl}/${id}`, user);
    }

    deleteUser(id: string): Observable<void> {
        return this.apiService.delete<void>(`${this.apiUrl}/${id}`);
    }
}
