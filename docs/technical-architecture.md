# Technical Design Specification: Enterprise HR System

## 1. Solution Overview
This document details the folder structure, project organization, and database schema for the Enterprise HR Leave & Overtime System (v2). It adheres to **Clean Architecture** for the backend (.NET 10) and **Modular/Standalone** architecture for the frontend (Angular 19).

---

## 2. Backend Architecture (.NET 10)

### 2.1. Solution Structure (`HrSystem.sln`)
The solution is divided into five strictly defined projects.

#### 2.1.1. `HrSystem.Domain` (Class Library)
*No external dependencies. The core of the application.*

```text
HrSystem.Domain/
├── Common/
│   ├── Entity.cs           # Base class (Id, CreatedAt)
│   ├── IAggregateRoot.cs   # Marker interface
│   └── Result.cs           # Result pattern wrapper
├── Entities/
│   ├── User.cs             # Properties: Id, Username, ManagerId, Role...
│   ├── Department.cs       # Properties: Id, Name
│   ├── LeaveRequest.cs     # Business logic for leave
│   ├── OvertimeRequest.cs  # Business logic for overtime
│   ├── ApprovalLog.cs      # Audit trail
│   └── AttendanceLog.cs    # Daily attendance records
├── Enums/
│   ├── RoleType.cs         # Employee, Manager, HR, Admin
│   ├── RequestStatus.cs    # Draft, Submitted, PendingManager, Approved...
│   └── LeaveType.cs        # Sick, Vacation, Unpaid
├── Interfaces/
│   ├── IUnitOfWork.cs
│   ├── IRepository.cs      # Generic Repository
│   ├── IEmailService.cs    # Abstraction for notifications
│   └── IAttendanceProvider.cs # Abstraction for external sync
└── Specifications/         # For encapsulating query logic (optional)
```

#### 2.1.2. `HrSystem.Application` (Class Library)
*Depends on: `HrSystem.Domain`.*

```text
HrSystem.Application/
├── Common/
│   ├── Interfaces/
│   │   ├── IApplicationDbContext.cs
│   │   ├── ICurrentUserService.cs
│   │   └── IIdentityService.cs
│   ├── Mappings/           # Mapster/AutoMapper profiles
│   └── Behaviors/          # Pipeline behaviors (Logging, Validation)
├── DTOs/                   # Data Transfer Objects (Request/Response)
│   ├── Auth/
│   │   ├── LoginRequest.cs
│   │   └── LoginResponse.cs
│   ├── Requests/
│   │   ├── CreateLeaveDto.cs
│   │   └── RequestSummaryDto.cs
│   └── Reports/
│       └── AttendanceReportDto.cs
├── Services/               # Application Services (Use Cases)
│   ├── Auth/
│   │   └── AuthService.cs
│   ├── Requests/
│   │   └── LeaveRequestService.cs
│   └── Reports/
│       └── ReportService.cs
└── Validators/             # FluentValidation rules
    └── CreateLeaveDtoValidator.cs
```

#### 2.1.3. `HrSystem.Infrastructure` (Class Library)
*Depends on: `HrSystem.Domain`, `HrSystem.Application`.*

```text
HrSystem.Infrastructure/
├── Data/
│   ├── Context/
│   │   └── HrDbContext.cs  # Implements IApplicationDbContext
│   ├── Config/             # EntityTypeConfiguration classes
│   │   ├── UserConfig.cs
│   │   └── LeaveRequestConfig.cs
│   ├── Migrations/         # EF Core Migrations
│   └── Repositories/
│       ├── EfRepository.cs # Generic Implementation
│       └── LeaveRepository.cs
├── Services/
│   ├── SmtpEmailService.cs
│   ├── SqlAttendanceProvider.cs # Implementation connecting to legacy DB
│   └── IdentityService.cs  # Implementation of IIdentityService
└── DependencyInjection.cs  # Service registration extension
```

#### 2.1.4. `HrSystem.Api` (Web API)
*Depends on: `HrSystem.Application`, `HrSystem.Infrastructure`.*

```text
HrSystem.Api/
├── Controllers/
│   ├── AuthController.cs
│   ├── EmployeesController.cs
│   ├── RequestsController.cs
│   └── ReportsController.cs
├── Middleware/
│   ├── ErrorHandlingMiddleware.cs
│   └── JwtMiddleware.cs
├── Services/
│   └── CurrentUserService.cs # Implementation of ICurrentUserService
├── Program.cs              # Entry point, Policies, DI
└── appsettings.json
```

#### 2.1.5. `HrSystem.Tests` (xUnit)
*Depends on: `HrSystem.Domain`, `HrSystem.Application`.*

```text
HrSystem.Tests/
├── Domain/
│   ├── LeaveRequestTests.cs    # Test domain logic (e.g., validation)
│   └── UserTests.cs
├── Application/
│   ├── LeaveRequestServiceTests.cs # Test use case logic (mocking repos)
│   └── ValidatorTests.cs
└── Integration/
    └── ApiTests.cs             # Test endpoints with WebApplicationFactory
```

---

## 3. Frontend Architecture (Angular 19)

### 3.1. Project Structure (`src/app`)
Using **Standalone Components** and strict **Feature Modules**.

```text
src/app/
├── core/                       # Singleton services & guards
│   ├── auth/
│   │   ├── auth.service.ts     # Signals for User State
│   │   ├── auth.interceptor.ts # Appends JWT
│   │   └── auth.guard.ts       # Route protection
│   ├── services/
│   │   ├── api.service.ts      # Generic HTTP wrapper
│   │   └── signalR.service.ts  # (Optional) Real-time updates
│   └── models/
│       ├── user.model.ts
│       └── request.model.ts
├── shared/                     # Reusable UI components
│   ├── components/
│   │   ├── data-table/         # Generic table with @for
│      └── confirm-dialog/
│   └── pipes/
│       └── status-color.pipe.ts
├── features/
│   ├── auth/
│   │   └── login/
│   ├── dashboard/              # Employee landing page
│   │   └── employee-summary.component.ts
│   ├── requests/               # Leave & Overtime
│   │   ├── request-form/       # Create/Edit
│   │   └── request-list/       # History
│   ├── approvals/              # Manager Actions
│   │   └── approval-queue.component.ts
│   ├── admin/                  # User Management
│   │   ├── user-list/
│   │   └── user-editor/        # Assign ManagerId here
│   └── reports/                # HR Only
│       ├── report-viewer/
│       └── report-filters/     # Signals-based filtering
├── app.routes.ts               # Lazy loaded routes
└── app.config.ts               # ApplicationConfig (Providers)
```

---

## 4. Database Schema Details

### 4.1. Entity: `Users`
*Stores all actors in the system.*

| Column | Type | Nullable | Description |
| :--- | :--- | :--- | :--- |
| `Id` | `Guid` | PK | Unique Identifier |
| `Username` | `NVARCHAR(50)` | No | Login ID |
| `PasswordHash` | `NVARCHAR(MAX)` | No | Securely hashed |
| `FullName` | `NVARCHAR(100)` | No | Display Name |
| `Email` | `NVARCHAR(100)` | No | Notification Target |
| `Role` | `Enum (Int)` | No | Employee, Manager, HR, Admin |
| `ManagerId` | `Guid` | **Yes** | **FK to Users.Id**. Defines Approval Chain. |
| `DepartmentId` | `Guid` | Yes | FK to Departments |

### 4.2. Entity: `LeaveRequests`
*Core domain entity for tracking time off.*

| Column | Type | Nullable | Description |
| :--- | :--- | :--- | :--- |
| `Id` | `Guid` | PK | |
| `UserId` | `Guid` | No | FK to Users (Requester) |
| `StartDate` | `Date` | No | |
| `EndDate` | `Date` | No | |
| `LeaveType` | `Enum` | No | Sick, Vacation, etc. |
| `Status` | `Enum` | No | Draft, Submitted, PendingMgr, PendingHR, Approved, Rejected |
| `Reason` | `NVARCHAR(500)` | Yes | |
| `CreatedAt` | `DateTime` | No | |

### 4.3. Entity: `ApprovalLogs`
*Audit trail for every decision made.*

| Column | Type | Nullable | Description |
| :--- | :--- | :--- | :--- |
| `Id` | `Int` | PK | Auto-increment |
| `RequestId` | `Guid` | No | FK to Leave/Overtime Requests |
| `RequestType` | `String` | No | "Leave" or "Overtime" |
| `ActionByUserId` | `Guid` | No | Who performed the action (Manager/HR) |
| `OldStatus` | `Enum` | No | Status before action |
| `NewStatus` | `Enum` | No | Status after action |
| `ActionDate` | `DateTime` | No | |
| `Comments` | `NVARCHAR(MAX)` | Yes | Rejection reasons or override notes |

### 4.4. Entity: `AttendanceLogs`
*Raw data from external devices.*

| Column | Type | Nullable | Description |
| :--- | :--- | :--- | :--- |
| `Id` | `BigInt` | PK | |
| `UserId` | `Guid` | No | FK to Users |
| `Timestamp` | `DateTime` | No | Check-in/out time |
| `Type` | `Enum` | No | CheckIn or CheckOut |
| `Source` | `String` | Yes | "BiometricDevice_01", "Manual_HR_Sync" |
