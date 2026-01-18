# Backend Folder Structure

## Overview
.NET 10 Clean Architecture with 5 projects

```
HrSystem.Domain/                    # Core domain - NO external dependencies
├── Common/
│   ├── Entity.cs                  # Base entity class (Id, CreatedAt)
│   ├── IAggregateRoot.cs          # Marker interface
│   └── Result.cs                  # Result pattern wrapper
├── Entities/
│   ├── User.cs                    # User with Role, ManagerId
│   ├── Department.cs              # Department entity
│   ├── LeaveRequest.cs            # Leave request logic
│   ├── OvertimeRequest.cs         # Overtime request logic
│   ├── ApprovalLog.cs             # Audit trail
│   └── AttendanceLog.cs           # Daily attendance records
├── Enums/
│   ├── RoleType.cs                # Employee, Manager, HR, Admin
│   ├── RequestStatus.cs           # Draft, Submitted, PendingManager...
│   └── LeaveType.cs               # Sick, Vacation, Unpaid
├── Interfaces/
│   ├── IUnitOfWork.cs
│   ├── IRepository.cs             # Generic Repository interface
│   ├── IEmailService.cs           # Email abstraction
│   └── IAttendanceProvider.cs     # External attendance sync
└── Specifications/                # Query specification pattern

HrSystem.Application/               # Use cases & DTOs - Depends on Domain
├── Common/
│   ├── Interfaces/
│   │   ├── IApplicationDbContext.cs
│   │   ├── ICurrentUserService.cs
│   │   └── IIdentityService.cs
│   ├── Mappings/                  # Mapster/AutoMapper profiles
│   └── Behaviors/                 # MediatR pipeline behaviors (Logging, Validation)
├── DTOs/
│   ├── Auth/
│   │   ├── LoginRequest.cs
│   │   └── LoginResponse.cs
│   ├── Requests/
│   │   ├── CreateLeaveDto.cs
│   │   └── RequestSummaryDto.cs
│   └── Reports/
│       └── AttendanceReportDto.cs
├── Services/                      # Application services (Use cases)
│   ├── Auth/
│   │   └── AuthService.cs
│   ├── Requests/
│   │   └── LeaveRequestService.cs
│   └── Reports/
│       └── ReportService.cs
└── Validators/                    # FluentValidation rules
    └── CreateLeaveDtoValidator.cs

HrSystem.Infrastructure/            # Data access & external services - Depends on Domain & Application
├── Data/
│   ├── Context/
│   │   └── HrDbContext.cs         # EF Core DbContext
│   ├── Config/                    # Entity Type Configurations
│   │   ├── UserConfig.cs
│   │   └── LeaveRequestConfig.cs
│   ├── Migrations/                # EF Core Migrations
│   └── Repositories/
│       ├── EfRepository.cs        # Generic repository implementation
│       └── LeaveRepository.cs     # Custom repository
├── Services/
│   ├── SmtpEmailService.cs
│   ├── SqlAttendanceProvider.cs   # External attendance integration
│   └── IdentityService.cs         # Identity service implementation
└── DependencyInjection.cs         # Service registration extension

HrSystem.Api/                       # Web API - Depends on Application & Infrastructure
├── Controllers/
│   ├── AuthController.cs
│   ├── EmployeesController.cs
│   ├── RequestsController.cs
│   └── ReportsController.cs
├── Middleware/
│   ├── ErrorHandlingMiddleware.cs
│   └── JwtMiddleware.cs
├── Services/
│   └── CurrentUserService.cs      # Implementation of ICurrentUserService
├── Program.cs                     # Startup, DI registration, Policies
└── appsettings.json

HrSystem.Tests/                     # xUnit Tests - Depends on Domain & Application
├── Domain/
│   ├── LeaveRequestTests.cs
│   └── UserTests.cs
├── Application/
│   ├── LeaveRequestServiceTests.cs
│   └── ValidatorTests.cs
└── Integration/
    └── ApiTests.cs
```

## Key Principles
- **Domain Layer**: Pure C#, no external dependencies, contains business logic
- **Application Layer**: Service interfaces, DTOs, validators, no database access
- **Infrastructure Layer**: EF Core, repositories, email, external integrations
- **API Layer**: Controllers, middleware, dependency injection
- **Tests**: Unit, integration, and API tests using xUnit
