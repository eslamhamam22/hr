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
}

export interface CreateLeaveRequest {
  leaveTypeId: number;
  startDate: Date;
  endDate: Date;
  reason: string;
}

export interface CreateOvertimeRequest {
  startDateTime: Date;
  endDateTime: Date;
  reason: string;
}
