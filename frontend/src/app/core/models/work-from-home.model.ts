export interface WorkFromHomeRequest {
    id: string;
    userId: string;
    userName?: string;
    fromDate: string; // ISO Date string
    toDate: string;   // ISO Date string
    totalDays: number;
    reason: string;
    status: string;
    managerId?: string;
    approvedByHRId?: string;
    approvedByHRName?: string;
    submittedAt?: string;
    approvedAt?: string;
    rejectionReason?: string;
    createdAt: string;
}

export interface CreateWorkFromHomeRequest {
    fromDate: string;
    toDate: string;
    reason: string;
}

export interface PaginatedResult<T> {
    items: T[];
    totalCount: number;
    page: number;
    pageSize: number;
    totalPages: number;
    hasNextPage: boolean;
    hasPreviousPage: boolean;
}
