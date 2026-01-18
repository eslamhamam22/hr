# Business Requirements Document (BRD)
## Enterprise HR Leave & Overtime System (v2)

**Date:** January 12, 2026
**Version:** 2.0
**Status:** Draft

---

## 1. Executive Summary
The Enterprise HR Leave & Overtime System (v2) aims to modernize the existing human resources management processes. The new system will streamline leave requests, overtime tracking, and attendance monitoring through a robust, role-based web application. It introduces advanced reporting capabilities for HR personnel and ensures strict security via granular permissions.

## 2. Project Scope
The scope of this project includes the development of a web-based application comprising a .NET 10 backend and an Angular 19 frontend. The system will handle user authentication, leave management workflows, overtime auditing, and integration with external attendance hardware/databases.

### In Scope
-   User Authentication & Authorization (RBAC).
-   Leave Request Management (Submission, Approval, Rejection).
-   Overtime Tracking & Auditing.
-   Attendance Monitoring & Reporting.
-   HR Reporting Dashboard.
-   External Attendance Data Synchronization.

### Out of Scope
-   Payroll processing (though data export for payroll is included).
-   Mobile native apps (responsive web design is sufficient).

## 3. User Roles & Permissions
The system defines four primary user roles with specific access levels:

1.  **Employee**
    -   View personal attendance records.
    -   Submit leave and overtime requests.
    -   Track status of own requests (`Draft` -> `Submitted` -> `Approved`/`Rejected`).

2.  **Manager**
    -   All Employee permissions.
    -   Approve or reject requests from direct reports.

3.  **HR (Human Resources)**
    -   **Global Visibility:** View all employee records (leaves, overtime, attendance) across the organization.
    -   **Employee Management:** Add new employees and assign them to a Manager to establish the approval pipeline.
    -   **Reporting:** Generate and view summaries (Attendance, Leave, Overtime).
    -   **Document Tracking:** Monitor contract end dates and expired documents.
    -   **Overrides:** Ability to finalize requests if managers are unavailable.
    -   **Sync:** Trigger manual synchronization with external attendance providers.

4.  **Admin**
    -   **System Configuration:** Manage global settings and leave policies.
    -   **User Management:** Create, edit, and deactivate users; assign roles and managers.
    -   **Define Workflows:** Configure leave policies and approval logic.

## 4. Functional Requirements

### 4.1. Authentication & Security
-   **REQ-AUTH-01:** System must use JWT-based authentication.
-   **REQ-AUTH-02:** Access must be restricted based on the four defined roles.
-   **REQ-AUTH-03:** Specific routes and API endpoints must be protected by Authorization Policies (e.g., `RequireHRRole`).

### 4.2. User & Employee Management
-   **REQ-USR-01:** HR and Admin users must be able to create new employee profiles.
-   **REQ-USR-02:** Every employee **must** be assigned to a Manager during creation or update to handle their request pipeline.
-   **REQ-USR-03:** Admin can manage roles (Employee, Manager, HR, Admin) for any user.

### 4.3. Leave & Overtime Management
-   **REQ-LVE-01:** **Sequential Approval Workflow:** `Draft` -> `Submitted` -> `Pending Manager` -> `Pending HR/Admin` -> `Approved`.
    -   **Constraint:** A request **must** be approved by the Manager before it becomes visible or actionable for HR/Admin.
    -   **Final Decision:** The Admin (or authorized HR) makes the final decision to Approve or Reject the request after the Manager's approval.
-   **REQ-LVE-02:** HR must have "Override" capability to approve/reject pending requests at any stage.
-   **REQ-LVE-03:** Employees must be able to view their leave balance vs. taken days.

### 4.3. Attendance System
-   **REQ-ATT-01:** System must ingest attendance logs via an `IAttendanceProvider` interface.
-   **REQ-ATT-02:** HR users must be able to trigger a "Manual Sync" to fetch latest logs from external SQL DB or API.

### 4.4. Reporting & Analytics (HR Module)
-   **REQ-RPT-01:** **Attendance Report:** Summary of work hours, lates, and absences by date range.
-   **REQ-RPT-02:** **Leave Summary:** Report showing total leave days taken vs. remaining per employee.
-   **REQ-RPT-03:** **Overtime Audit:** Report detailed costs and hours for overtime per department.
-   **REQ-RPT-04:** Reports must support filtering by Department, Employee ID, and Date Range.
-   **REQ-RPT-05:** Data must be exportable to Excel/CSV and PDF formats.

## 5. Non-Functional Requirements

### 5.1. Technology Stack
-   **Backend:** .NET 10 (C# 14) Web API, following **Clean Architecture** (Domain, Infrastructure, API, Tests).
-   **Frontend:** Angular 19 (Standalone Components, Signals, New Control Flow).
-   **Database:** SQL Server managed via Entity Framework Core.

### 5.2. Performance & Usability
-   **REQ-PERF-01:** Reporting queries must use optimized DTOs (Read-Only) to ensure fast load times for large datasets.
-   **REQ-UI-01:** HR Dashboard must use Signal-based filtering for high-performance UI reactivity.

## 6. Interface Requirements
*Refer to the following screenshots in the `docs/` folder for visual guidance on UI layout and flow:*
-   `Screenshot 2026-01-12 104931.png`
-   `Screenshot 2026-01-12 105015.png`
-   `Screenshot 2026-01-12 105043.png`
-   `Screenshot 2026-01-12 105058.png`
-   `Screenshot 2026-01-12 105113.png`
-   `Screenshot 2026-01-12 105130.png`
-   `Screenshot 2026-01-12 105147.png`
-   `Screenshot 2026-01-12 105213.png`

## 7. Data Requirements
-   **Roles Table:** Define roles (Employee, Manager, HR, Admin).
-   **UserRoles Table:** Map users to roles.
-   **ApprovalLogs:** Audit trail for every state change in a request, specifically highlighting HR overrides.
-   **ReportTemplates:** (Optional) Metadata for report configurations.
