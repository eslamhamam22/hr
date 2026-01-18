export interface OvertimeRequest {
    id: string;
    userId: string;
    userName?: string;
    startDateTime: Date;
    endDateTime: Date;
    hoursWorked: number;
    reason: string;
    status: RequestStatus;
    managerId?: string;
    managerName?: string;
    approvedByHRId?: string;
    approvedByHRName?: string;
    submittedAt?: Date;
    approvedAt?: Date;
    rejectionReason?: string;
    isOverridden: boolean;
    createdAt: Date;
    updatedAt?: Date;
}

export interface CreateOvertimeDto {
    startDateTime: Date;
    endDateTime: Date;
    hoursWorked: number;
    reason: string;
}

export enum RequestStatus {
    Draft = 0,
    Submitted = 1,
    ManagerApproved = 2,
    HRApproved = 3,
    Approved = 4,
    Rejected = 5
}

export function getStatusLabel(status: RequestStatus): string {
    switch (status) {
        case RequestStatus.Draft: return 'Draft';
        case RequestStatus.Submitted: return 'Submitted';
        case RequestStatus.ManagerApproved: return 'Manager Approved';
        case RequestStatus.HRApproved: return 'HR Approved';
        case RequestStatus.Approved: return 'Approved';
        case RequestStatus.Rejected: return 'Rejected';
        default: return 'Unknown';
    }
}

export function getStatusClass(status: RequestStatus): string {
    switch (status) {
        case RequestStatus.Draft: return 'status-draft';
        case RequestStatus.Submitted: return 'status-submitted';
        case RequestStatus.ManagerApproved: return 'status-manager-approved';
        case RequestStatus.HRApproved: return 'status-hr-approved';
        case RequestStatus.Approved: return 'status-approved';
        case RequestStatus.Rejected: return 'status-rejected';
        default: return '';
    }
}
