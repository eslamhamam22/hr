export interface ApprovalLog {
    id: string;
    requestId: string;
    requestType: string;
    approvedByUserId: string;
    approvedByUserName?: string;
    approved: boolean;
    comments?: string;
    isOverride: boolean;
    overrideReason?: string;
    createdAt: Date;
}

export interface ApprovalLogFilters {
    requestType?: string;
    approvedByUserId?: string;
    approved?: boolean;
    startDate?: Date;
    endDate?: Date;
}
