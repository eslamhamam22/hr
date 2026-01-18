# Architecture Analysis: Enterprise HR Leave & Overtime System (v2)

## 1. High-Level Architecture
The solution follows a standard N-Tier architecture, emphasizing a clear separation of concerns between the presentation layer (SPA), the API layer (REST), and the data layer (SQL Server). The system is designed to be modular, scalable, and maintainable, leveraging the latest features of .NET 10 and Angular 19.

### Diagram Context
-   **Client:** Angular 19 SPA running in the browser.
-   **API:** .NET 10 Web API serving JSON data.
-   **Database:** SQL Server for relational data storage.
-   **External Integration:** Attendance hardware/DB accessed via the `IAttendanceProvider` abstraction.

---

## 2. Backend Architecture (.NET 10 / C# 14)

The backend solution will adhere to **Clean Architecture** principles to ensure separation of concerns, testability, and independence from external frameworks.

### 2.1. Solution Structure & Dependencies
The Visual Studio solution will consist of the following projects, enforcing the "Dependency Rule" (dependencies point inwards).

1.  **`HrSystem.Domain` (Core)**
    -   **Role:** The heart of the software. Contains enterprise logic and types.
    -   **Dependencies:** None.
    -   **Components:**
        -   **Entities:** `Employee`, `LeaveRequest`, `OvertimeRequest`, `ApprovalLog` (using C# 14 field-backed properties).
        -   **Interfaces:** `IUnitOfWork`, `ILeaveRepository`, `IAttendanceProvider` (Abstractions).
        -   **Value Objects & Enums:** `RequestStatus`, `RoleType`.
        -   **Domain Services:** Pure business logic validation (e.g., `LeaveCalculator`).

2.  **`HrSystem.Infrastructure`**
    -   **Role:** Implementation of external concerns and data access.
    -   **Dependencies:** `HrSystem.Domain`.
    -   **Components:**
        -   **Persistence:** Entity Framework Core `HrDbContext`, `EfRepository` implementations.
        -   **External Services:** Implementation of `IAttendanceProvider` (e.g., SQL/API adapters).
        -   **Migrations:** Database schema management.

3.  **`HrSystem.Api` (Web API)**
    -   **Role:** The entry point and presentation layer for the backend.
    -   **Dependencies:** `HrSystem.Domain`, `HrSystem.Infrastructure`.
    -   **Components:**
        -   **Controllers:** REST endpoints handling HTTP requests.
        -   **DTOs:** Data Transfer Objects (Request/Response models) to decouple API contract from Domain Entities.
        -   **Configuration:** Dependency Injection (DI) setup, Authentication/Authorization policies (`Program.cs`).
        -   **Mappers:** Mapping logic (e.g., Mapster or AutoMapper) between Entities and DTOs.

4.  **`HrSystem.Tests`**
    -   **Role:** Verification of business logic.
    -   **Dependencies:** `HrSystem.Domain`, `HrSystem.Infrastructure` (referenced for integration tests if needed, mostly mocks).
    -   **Components:**
        -   **Unit Tests:** xUnit tests targeting `HrSystem.Domain` logic.
        -   **Mocking:** Using `Moq` or `NSubstitute` to mock `IRepository` and `IAttendanceProvider` interfaces.

### 2.2. Authentication & Authorization
-   **JWT (JSON Web Tokens):** Stateless authentication handled in the API layer.
-   **Policy-Based Authorization:**
    -   Define policies in `Program.cs` (API Layer):
        ```csharp
        options.AddPolicy("RequireHRRole", policy => policy.RequireRole("HR", "Admin"));
        ```
    -   Apply via attributes: `[Authorize(Policy = "RequireHRRole")]`.

### 2.3. Data Access Strategy
-   **Repository Pattern:** `IRepository<T>` defined in **Domain**, implemented in **Infrastructure**.
-   **CQRS-Lite:**
    -   **Commands (Writes):** Processed via Domain Services and Repositories to ensure invariants.
    -   **Queries (Reads):** potentially bypassed or optimized (using Dapper or projected EF queries) within the Infrastructure layer for the Reporting Module to return Read-Only DTOs directly.

---

## 3. Frontend Architecture (Angular 19)

### 3.1. Modern Angular Practices
-   **Standalone Components:** No `NgModule` boilerplate. All components, directives, and pipes are standalone.
-   **Signals:** Used for all state management and reactivity.
    -   `input()` for component props.
    -   `computed()` for derived state (e.g., filtered report lists).
    -   `effect()` for side effects (e.g., logging or external sync triggers).
-   **Control Flow:** Use of new syntax `@if`, `@for`, `@switch` in templates.

### 3.2. Module Structure
-   **Core:** Singleton services (AuthService, ApiService), Guards, Interceptors.
-   **Shared:** Reusable UI components (Buttons, Tables, Loaders).
-   **Features:**
    -   `auth/`: Login.
    -   `dashboard/`: Employee summary.
    -   `requests/`: Leave/Overtime forms and history.
    -   `approvals/`: Manager approval views.
    -   `reports/`: **(HR Only)** Protected by `RoleGuard`. Contains `ReportViewer` component.
    -   `admin/`: **(Admin Only)** User management.

### 3.3. Security
-   **Guards:**
    -   `AuthGuard`: Checks for valid JWT.
    -   `RoleGuard`: Checks user roles claims against route data requirements.
-   **Interceptors:** Automatically attach Bearer token to outgoing requests.

---

## 4. Database Design Schema Analysis

### 4.1. Key Tables
-   **Users (and Employee Entity)**
    -   `Id` (PK), `Username`, `PasswordHash`, `DepartmentId`
    -   `ManagerId` (FK to Users.Id, Nullable): Defines the reporting line for the sequential approval pipeline.
-   **Roles**
    -   `Id` (PK), `Name` (Unique: 'Employee', 'Manager', 'HR', 'Admin')
-   **UserRoles**
    -   `UserId` (FK), `RoleId` (FK)
-   **LeaveRequests**
    -   `Id`, `UserId`, `StartDate`, `EndDate`, `Type`, `Status`, `Reason`
-   **OvertimeRequests**
    -   `Id`, `UserId`, `Date`, `Hours`, `Status`, `Reason`
-   **ApprovalLogs**
    -   `Id`, `RequestId`, `RequestType` (Leave/Overtime), `ActionByUserId`, `ActionDate`, `OldStatus`, `NewStatus`, `Comments`
    -   *Crucial for tracking HR Overrides.*
-   **AttendanceLogs**
    -   `Id`, `UserId`, `CheckInTime`, `CheckOutTime`, `Source` (Manual/System)

---

## 5. Deployment Strategy
-   **Containerization:** Docker support for both Backend (ASP.NET Core image) and Frontend (Nginx image serving static build).
-   **CI/CD:** Pipelines to run tests (`dotnet test`, `ng test`), build, and deploy to staging/production environments.
