export interface Department {
    id: string;
    name: string;
    description?: string;
    isActive: boolean;
    createdAt: Date;
    updatedAt?: Date;
}

export interface CreateDepartmentDto {
    name: string;
    description?: string;
    isActive: boolean;
}

export interface UpdateDepartmentDto {
    name: string;
    description?: string;
    isActive: boolean;
}

export interface PaginatedResponse<T> {
    items: T[];
    totalCount: number;
    page: number;
    pageSize: number;
    totalPages: number;
}
