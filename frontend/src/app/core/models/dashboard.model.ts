export interface EmployeeDashboard {
    employeeName: string;
    departmentName: string;
    role: string;
    leaveSummary: LeaveSummary;
    overtimeSummary: OvertimeSummary;
    workFromHomeSummary: WorkFromHomeSummary;
    timeOffSummary: TimeOffSummary;
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

export interface WorkFromHomeSummary {
    totalDays: number;
    totalRequests: number;
    pendingRequests: number;
    approvedRequests: number;
}

export interface TimeOffSummary {
    totalRequests: number;
    pendingRequests: number;
    approvedRequests: number;
    rejectedRequests: number;
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
    workFromHomeDays: number;
    timeOffCount: number;
    pendingRequests: number;
}

export interface MonthlyTrend {
    month: string;
    leaveRequests: number;
    overtimeRequests: number;
    workFromHomeRequests: number;
    timeOffRequests: number;
}

export interface AdminDashboard extends EmployeeDashboard {
    totalEmployees: number;
    totalDepartments: number;
    systemLeaveStats: RequestStats;
    systemOvertimeStats: RequestStats;
    systemWorkFromHomeStats: RequestStats;
    systemTimeOffStats: RequestStats;
    requestsByDepartment: DepartmentRequestCount[];
    systemMonthlyTrend: MonthlyTrend[];
    systemLeaveTypeDistribution: LeaveTypeCount[];
}

export interface RequestStats {
    total: number;
    pending: number;
    approved: number;
    rejected: number;
}

export interface DepartmentRequestCount {
    departmentName: string;
    leaveRequests: number;
    overtimeRequests: number;
    workFromHomeRequests: number;
    timeOffRequests: number;
    totalRequests: number;
}
