# Enterprise HR Leave & Overtime System

## Project Overview
This is a comprehensive full-stack application designed to manage employee leave requests, overtime tracking, attendance monitoring, and HR reporting. It utilizes a modern **.NET 10** backend following Clean Architecture principles and an **Angular 19** frontend leveraging Signals and Standalone Components.

### Key Technologies
*   **Backend:** .NET 10, Entity Framework Core (SQL Server), Clean Architecture.
*   **Frontend:** Angular 19, TypeScript 5.6+, RxJS, SCSS.
*   **Database:** SQL Server.
*   **Authentication:** JWT-based with Role-Based Access Control (RBAC).

---

## Building and Running

### Backend (.NET)
The backend is located in the `backend/` directory.

1.  **Prerequisites:** .NET 10 SDK, SQL Server.
2.  **Configuration:** Update connection strings and JWT settings in `backend/HrSystem.Api/appsettings.json`.
3.  **Database Setup:**
    ```bash
    dotnet ef database update --project HrSystem.Infrastructure --startup-project HrSystem.Api
    ```
4.  **Run API:**
    ```bash
    cd backend
    dotnet run --project HrSystem.Api
    ```
    *   **Default URLs:** `https://localhost:54660` and `http://localhost:54661` (check `launchSettings.json`).
    *   *Note:* The documentation mentions `https://localhost:5001`, but `launchSettings.json` dictates the actual ports.

5.  **Run Tests:**
    ```bash
    dotnet test
    ```

### Frontend (Angular)
The frontend is located in the `frontend/` directory.

1.  **Prerequisites:** Node.js 18+, npm 9+, Angular CLI 19.
2.  **Setup:**
    ```bash
    cd frontend
    npm install
    ```
3.  **Configuration:** Ensure `src/app/core/services/api.service.ts` points to the running backend API URL.
4.  **Run Development Server:**
    ```bash
    npm start
    ```
    *   **URL:** `http://localhost:4235` (custom port defined in `package.json`).
5.  **Build for Production:**
    ```bash
    npm run build
    ```
6.  **Run Tests:**
    ```bash
    npm test
    ```

---

## Architecture & Conventions

### Backend (Clean Architecture)
The solution is structured into four main layers:
*   **Domain (`HrSystem.Domain`):** Core business logic, entities, enums, and interfaces. No external dependencies.
*   **Application (`HrSystem.Application`):** DTOs, validators, and service interfaces. Orchestrates business logic.
*   **Infrastructure (`HrSystem.Infrastructure`):** Implementation of interfaces (repositories, email, auth), database context (EF Core), and migrations.
*   **API (`HrSystem.Api`):** Web API controllers, middleware, and entry point.

**Key Patterns:**
*   **Repository Pattern:** For data access abstraction.
*   **Result Pattern:** For consistent error handling and service responses.
*   **Validation:** FluentValidation is used for DTO validation.

### Frontend (Angular)
*   **Standalone Components:** No `NgModule` boilerplate.
*   **State Management:** Uses **Signals** for reactive local state and services.
*   **Project Structure:**
    *   `core/`: Singleton services, auth logic, models.
    *   `shared/`: Reusable UI components and pipes.
    *   `features/`: Lazy-loaded feature modules (Dashboard, Requests, Reports, etc.).
*   **Styles:** SCSS with global styles in `styles.scss`.

---

## Key Documentation
*   `docs/COMPLETE_PROJECT_SUMMARY.md`: High-level overview and status.
*   `docs/technical-architecture.md`: Detailed architectural decisions.
*   `backend/README.md`: Specific backend details.
*   `frontend/README.md`: Specific frontend details.
