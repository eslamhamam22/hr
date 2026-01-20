/**
 * Types of leave - matches backend LeaveType enum
 */
export enum LeaveType {
  Sick = 1,
  Vacation = 2,
  Unpaid = 3,
  Maternity = 4,
  Paternity = 5,
  CompensatoryOff = 6
}

/**
 * Helper object for leave type display names
 */
export const LeaveTypeLabels: { [key in LeaveType]: string } = {
  [LeaveType.Sick]: 'Sick Leave',
  [LeaveType.Vacation]: 'Vacation',
  [LeaveType.Unpaid]: 'Unpaid Leave',
  [LeaveType.Maternity]: 'Maternity Leave',
  [LeaveType.Paternity]: 'Paternity Leave',
  [LeaveType.CompensatoryOff]: 'Compensatory Off'
};

/**
 * Get display label for a leave type
 */
export function getLeaveTypeLabel(leaveType: LeaveType | number): string {
  return LeaveTypeLabels[leaveType as LeaveType] || 'Unknown';
}

/**
 * Get all leave type options for dropdowns
 */
export function getLeaveTypeOptions(): { value: LeaveType; label: string }[] {
  return [
    { value: LeaveType.Sick, label: 'Sick Leave' },
    { value: LeaveType.Vacation, label: 'Vacation' },
    { value: LeaveType.Unpaid, label: 'Unpaid Leave' },
    { value: LeaveType.Maternity, label: 'Maternity Leave' },
    { value: LeaveType.Paternity, label: 'Paternity Leave' },
    { value: LeaveType.CompensatoryOff, label: 'Compensatory Off' }
  ];
}

export interface Request {
  id: string;
  employeeName: string;
  requestType: string;
  status: string;
  startDate: Date;
  endDate: Date;
  submittedAt: Date;
  approvedByName?: string;
  approvedAt?: Date;
  reason?: string;
}

export interface CreateLeaveRequest {
  leaveTypeId: number;
  startDate: string;
  endDate: string;
  reason: string;
}

export interface CreateOvertimeRequest {
  startDateTime: Date;
  endDateTime: Date;
  reason: string;
}
