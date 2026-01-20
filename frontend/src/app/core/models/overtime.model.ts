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

/**
 * Request status enum - matches backend RequestStatus
 * Workflow: Draft -> Submitted -> (Manager approval) -> PendingHR -> (HR approval) -> Approved
 *           Any pending status can be -> Rejected
 */
export enum RequestStatus {
    Draft = 1,
    Submitted = 2,        // Waiting for Manager approval
    PendingManager = 3,   // Legacy, same as Submitted
    PendingHR = 4,        // Manager approved, waiting for HR
    Approved = 5,         // Final approved
    Rejected = 6,         // Rejected at any stage
    Cancelled = 7,        // Cancelled by user
    Withdrawn = 8         // Withdrawn by user
}

export function getStatusLabel(status: RequestStatus | number): string {
    switch (status) {
        case RequestStatus.Draft: return 'Draft';
        case RequestStatus.Submitted: return 'Pending Manager';
        case RequestStatus.PendingManager: return 'Pending Manager';
        case RequestStatus.PendingHR: return 'Pending HR';
        case RequestStatus.Approved: return 'Approved';
        case RequestStatus.Rejected: return 'Rejected';
        case RequestStatus.Cancelled: return 'Cancelled';
        case RequestStatus.Withdrawn: return 'Withdrawn';
        default: return 'Unknown';
    }
}

export function getStatusClass(status: RequestStatus | number): string {
    switch (status) {
        case RequestStatus.Draft: return 'status-draft';
        case RequestStatus.Submitted: return 'status-pending';
        case RequestStatus.PendingManager: return 'status-pending';
        case RequestStatus.PendingHR: return 'status-pending-hr';
        case RequestStatus.Approved: return 'status-approved';
        case RequestStatus.Rejected: return 'status-rejected';
        case RequestStatus.Cancelled: return 'status-cancelled';
        case RequestStatus.Withdrawn: return 'status-withdrawn';
        default: return '';
    }
}

/**
 * Check if request is in a pending state (awaiting any approval)
 */
export function isPendingApproval(status: RequestStatus | number): boolean {
    return status === RequestStatus.Submitted ||
        status === RequestStatus.PendingManager ||
        status === RequestStatus.PendingHR;
}

/**
 * Check if request can be approved by the current user role
 */
export function canApprove(status: RequestStatus | number, isManager: boolean, isHR: boolean): boolean {
    if (status === RequestStatus.Submitted && (isManager || isHR)) return true;
    if (status === RequestStatus.PendingHR && isHR) return true;
    return false;
}
