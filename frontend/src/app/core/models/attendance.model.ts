export interface AttendanceLog {
    id: string;
    userId: string;
    userName?: string;
    date: Date;
    checkInTime: string;
    checkOutTime?: string;
    hoursWorked: number;
    isLate: boolean;
    isAbsent: boolean;
    notes?: string;
    createdAt: Date;
}

export interface AttendanceFilters {
    userId?: string;
    startDate?: Date;
    endDate?: Date;
    isLate?: boolean;
    isAbsent?: boolean;
}
