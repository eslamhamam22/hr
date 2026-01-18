# Frontend Folder Structure

## Overview
Angular 19 with Standalone Components, Signals, and Feature-based architecture

```
src/app/
├── core/                          # Singleton services, guards, interceptors
│   ├── auth/
│   │   ├── auth.service.ts        # User state management with Signals
│   │   ├── auth.interceptor.ts    # JWT token injection
│   │   └── auth.guard.ts          # Route protection
│   ├── services/
│   │   ├── api.service.ts         # Generic HTTP wrapper
│   │   └── signalR.service.ts     # (Optional) Real-time updates
│   └── models/
│       ├── user.model.ts
│       └── request.model.ts
├── shared/                        # Reusable components & utilities
│   ├── components/
│   │   ├── data-table/            # Generic table with @for
│   │   │   └── data-table.component.ts
│   │   └── confirm-dialog/
│   │       └── confirm-dialog.component.ts
│   └── pipes/
│       └── status-color.pipe.ts
├── features/                      # Feature modules with lazy loading
│   ├── auth/
│   │   └── login/
│   │       ├── login.component.ts
│   │       ├── login.component.html
│   │       └── login.component.scss
│   ├── dashboard/                 # Employee landing page
│   │   ├── employee-summary.component.ts
│   │   ├── employee-summary.component.html
│   │   └── employee-summary.component.scss
│   ├── requests/                  # Leave & Overtime requests
│   │   ├── request-form/          # Create/Edit forms
│   │   │   ├── request-form.component.ts
│   │   │   ├── request-form.component.html
│   │   │   └── request-form.component.scss
│   │   └── request-list/          # Request history
│   │       ├── request-list.component.ts
│   │       ├── request-list.component.html
│   │       └── request-list.component.scss
│   ├── approvals/                 # Manager approval actions
│   │   ├── approval-queue.component.ts
│   │   ├── approval-queue.component.html
│   │   └── approval-queue.component.scss
│   ├── admin/                     # HR & Admin management
│   │   ├── user-list/
│   │   │   ├── user-list.component.ts
│   │   │   ├── user-list.component.html
│   │   │   └── user-list.component.scss
│   │   └── user-editor/           # Assign ManagerId, roles
│   │       ├── user-editor.component.ts
│   │       ├── user-editor.component.html
│   │       └── user-editor.component.scss
│   └── reports/                   # HR reporting module
│       ├── report-viewer/         # Display reports with export
│       │   ├── report-viewer.component.ts
│       │   ├── report-viewer.component.html
│       │   └── report-viewer.component.scss
│       └── report-filters/        # Signals-based filtering
│           ├── report-filters.component.ts
│           ├── report-filters.component.html
│           └── report-filters.component.scss
├── app.routes.ts                  # Lazy-loaded routes configuration
└── app.config.ts                  # ApplicationConfig & Providers
```

## Key Principles
- **Core Module**: Singleton services, auth guards, interceptors (imported once in root)
- **Shared Module**: Reusable UI components, pipes, utilities (imported in features)
- **Feature Modules**: Self-contained features with components, services, models
- **Standalone**: All components are standalone (no NgModule)
- **Signals**: Used for reactive state management (auth, filtering)
- **Lazy Loading**: Routes use lazy-loaded component imports for performance
- **@for / @if**: New Angular control flow syntax (no *ngFor, *ngIf)

## File Organization
- Each feature has its own folder
- Components include `.ts`, `.html`, `.scss` (component styles)
- Models and interfaces in `core/models/`
- API communication through `core/services/api.service.ts`
- State management via Angular Signals
