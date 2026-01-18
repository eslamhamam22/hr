# HR System Backend (.NET 10)

Complete backend implementation of the Enterprise HR Leave & Overtime System using Clean Architecture.

## Project Structure

- **HrSystem.Domain**: Core business logic and domain entities (no external dependencies)
- **HrSystem.Application**: DTOs, validators, and service interfaces
- **HrSystem.Infrastructure**: Data access, external services, and EF Core implementation
- **HrSystem.Api**: Web API endpoints and startup configuration
- **HrSystem.Tests**: Unit and integration tests

## Getting Started

### Prerequisites
- .NET 10 SDK or later
- SQL Server (local or remote)
- Visual Studio 2022 or VS Code

### Setup

1. **Restore NuGet Packages**
   ```bash
   dotnet restore
   ```

2. **Configure Connection Strings**
   Edit `HrSystem.Api/appsettings.json` and update:
   - `ConnectionStrings.DefaultConnection` - Main HR database
   - `ConnectionStrings.AttendanceDb` - External attendance system (optional)
   - `Jwt.Key` - JWT secret key (change this in production!)
   - `Smtp.*` - Email configuration

3. **Create Database**
   ```bash
   dotnet ef database update --project HrSystem.Infrastructure --startup-project HrSystem.Api
   ```

4. **Run the API**
   ```bash
   dotnet run --project HrSystem.Api
   ```

   The API will be available at `https://localhost:5001` (or configured port)

## API Endpoints

### Authentication
- `POST /api/auth/login` - User login

### Requests (Leave & Overtime)
- `POST /api/requests/leave` - Create leave request
- `POST /api/requests/{id}/submit` - Submit request for approval

### Reports (HR Only)
- `GET /api/reports/attendance` - Attendance summary
- `GET /api/reports/leave-summary` - Leave taken vs remaining
- `GET /api/reports/overtime-audit` - Overtime audit trail

## Features

- ✅ JWT-based authentication and authorization
- ✅ Role-based access control (Employee, Manager, HR, Admin)
- ✅ Leave and overtime request management
- ✅ Multi-level approval workflow
- ✅ HR reporting and analytics
- ✅ External attendance system integration
- ✅ Email notifications
- ✅ Audit logging
- ✅ Policy-based authorization

## Authorization Policies

- `RequireHRRole` - HR and Admin only
- `RequireManagerRole` - Manager and Admin only
- `RequireAdminRole` - Admin only

## Testing

Run unit tests:
```bash
dotnet test
```

## Database Schema

Key entities:
- **Users** - All system users with roles
- **Departments** - Employee departments
- **LeaveRequests** - Leave request tracking
- **OvertimeRequests** - Overtime request tracking
- **ApprovalLogs** - Audit trail of approvals
- **AttendanceLogs** - Daily attendance records

## Architecture Notes

This project follows **Clean Architecture** principles:

1. **Domain Layer**: Pure business logic, no dependencies
2. **Application Layer**: Use cases, DTOs, validators
3. **Infrastructure Layer**: Data access, external services
4. **API Layer**: HTTP contracts and middleware

## Contributing

Follow clean code principles and add tests for new features.
