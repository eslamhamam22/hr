# Project Specification: Enterprise HR Leave & Overtime System (v2)

## 1. Tech Stack Requirements
- **Backend:** .NET 10 (C# 14) Web API
- **Frontend:** Angular 19 (Signals, Standalone, New Control Flow)
- **Database:** SQL Server (EF Core)
- **Auth:** JWT-based Role-Based Access Control (RBAC)

## 2. Updated Role Hierarchy & Permissions
The system must support four distinct roles:
1.  **Employee:** Can view own attendance, submit requests, and view own request status.
2.  **Manager:** Can approve/reject requests for their direct reports.
3.  **HR (Human Resources):** - **Global Visibility:** View all employees' leaves, overtime, and attendance records across all departments.
    - **Reporting:** Access to a dedicated "Reports" module to generate summaries.
    - **Document Tracking:** View expired documents or contract end dates.
4.  **Admin:** System configuration, user management, and setting leave policies.

## 3. Core Modules & Logic

### A. Reporting & Analytics Engine (HR Feature)
- **Attendance Report:** Generate a summary of total work hours, lates, and absences for a specific date range.
- **Leave Summary:** View total days taken vs. remaining for all employees.
- **Overtime Audit:** Report on total overtime costs/hours per department.
- **Export Formats:** Data must be prepared in a way that allows exporting to Excel/CSV or PDF.

### B. Multi-Level Approval Workflow
- `Draft` -> `Submitted` -> `Pending Manager` -> `Pending HR/Admin` -> `Approved`.
- **Note:** HR should have the power to "Override" or finalize a request if a manager is unavailable.

### C. External Attendance Integration
- Interface `IAttendanceProvider` to fetch logs from an external SQL DB or API.
- HR users must be able to trigger a "Manual Sync" from the UI.

## 4. Technical Requirements

### Backend (.NET 10)
- **Policy-Based Authorization:** Implement policies like `[Authorize(Policy = "RequireHRRole")]`.
- **DTOs for Reporting:** Optimized Read-Only models for large data fetches to avoid performance lag.
- **C# 14 Features:** Primary constructors for services and field-backed properties for entity logic.

### Frontend (Angular 19)
- **Role Guards:** Protect `/reports` and `/all-employees` routes so only HR/Admin can enter.
- **Signals-based Filtering:** HR Dashboard must include high-performance filters (Filter by Department, Employee ID, or Date).
- **Component Design:** A "Report Viewer" component with a data table and "Export" buttons.

## 5. Database Schema Additions
- `Roles` & `UserRoles` tables.
- `ReportTemplates`: (Optional) Store metadata for different HR report types.
- Ensure `ApprovalLogs` captures when an **HR user** overrides a manager's decision.

---
**Goal:** Generate the .NET 10 Auth Policy configuration, the SQL schema updated with Roles, and an Angular 19 "HR Report" component using the new `@for` and Signal-based filtering.