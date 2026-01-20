export interface EmployeeDashboard {
    employeeName: string;
    departmentName: string;
    role: string;
    leaveSummary: LeaveSummary;
    overtimeSummary: OvertimeSummary;
    recentRequests: RecentRequest[];
}

export interface ManagerDashboard extends EmployeeDashboard {
    teamSummary: TeamMemberSummary[];
    pendingApprovalsCount: number;
    teamLeaveByType: LeaveTypeCount[];
    monthlyTrend: MonthlyTrend[];
}

export interface LeaveSummary {
    totalDaysUsed: number;
    totalDaysRemaining: number;
    pendingRequests: number;
    approvedRequests: number;
    rejectedRequests: number;
    byLeaveType: LeaveTypeCount[];
}

export interface OvertimeSummary {
    totalHours: number;
    totalRequests: number;
    pendingRequests: number;
    approvedRequests: number;
}

export interface LeaveTypeCount {
    leaveType: string;
    days: number;
    count: number;
}

export interface RecentRequest {
    id: string;
    requestType: string;
    status: string;
    startDate: Date;
    endDate: Date;
    submittedAt: Date;
    days?: number;
    hours?: number;
}

export interface TeamMemberSummary {
    userId: string;
    userName: string;
    leaveDays: number;
    overtimeHours: number;
    pendingRequests: number;
}

export interface MonthlyTrend {
    month: string;
    leaveRequests: number;
    overtimeRequests: number;
}
