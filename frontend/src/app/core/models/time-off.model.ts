import { RequestStatus } from './overtime.model';

export interface TimeOffRequest {
    id: string;
    userId: string;
    userName?: string;
    date: Date;
    startTime: string; // TimeSpan as string
    reason: string;
    status: RequestStatus;
    managerId?: string;
    approvedByHRId?: string;
    approvedByHRName?: string;
    submittedAt?: Date;
    approvedAt?: Date;
    rejectionReason?: string;
    createdAt: Date;
}

export interface CreateTimeOffDto {
    date: Date;
    startTime: string; // "HH:mm:ss"
    reason: string;
}
