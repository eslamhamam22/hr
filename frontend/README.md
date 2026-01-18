# HR System Frontend (Angular 19)

Modern HR management system frontend using Angular 19 with Signals, Standalone Components, and new control flow syntax.

## Features

✅ **Authentication**
- JWT-based login
- Token persistence with localStorage
- Auto-logout on token expiration
- Role-based route protection

✅ **Employee Module**
- Dashboard with quick stats
- Leave request management
- Request history tracking
- Attendance view

✅ **Manager Module**
- Approval queue for direct reports
- Request review and decision
- Performance analytics

✅ **HR Module**
- User management (CRUD)
- Advanced reporting
- Attendance audit
- Leave and overtime tracking
- Export to Excel/CSV/PDF

✅ **Admin Module**
- System configuration
- User role management
- Manager assignment

✅ **Responsive Design**
- Mobile-friendly layout
- Adaptive grid system
- Touch-friendly buttons

## Tech Stack

- **Angular:** 19.x
- **TypeScript:** 5.6+
- **RxJS:** 7.8+
- **Standalone Components:** Yes
- **Signals:** Yes (reactive state management)
- **Routing:** Lazy-loaded feature modules
- **HTTP:** HttpClient with interceptors
- **Styling:** SCSS with global styles

## Project Structure

```
src/
├── app/
│   ├── core/                 # Singleton services, auth, models
│   │   ├── auth/            # Authentication service, guard, interceptor
│   │   ├── services/        # API, reporting, request services
│   │   └── models/          # TypeScript interfaces
│   ├── shared/              # Reusable components, pipes, utilities
│   │   ├── components/      # Data table, dialogs
│   │   └── pipes/           # Custom pipes
│   ├── features/            # Feature modules with lazy loading
│   │   ├── auth/            # Login component
│   │   ├── dashboard/       # Employee summary
│   │   ├── requests/        # Leave/Overtime forms and history
│   │   ├── approvals/       # Manager approval queue
│   │   ├── admin/           # User and system management
│   │   └── reports/         # HR analytics and reporting
│   ├── app.routes.ts        # Route configuration
│   ├── app.config.ts        # Application configuration
│   └── app.component.ts     # Root component
├── main.ts                  # Bootstrap application
├── index.html               # HTML template
└── styles.scss              # Global styles
```

## Getting Started

### Prerequisites
- Node.js 18+ and npm 9+
- Angular CLI 19.x

### Installation

1. **Install dependencies**
   ```bash
   npm install
   ```

2. **Configure API endpoint**
   Edit `src/app/core/services/api.service.ts` and update the `apiUrl`:
   ```typescript
   private apiUrl = 'https://your-api-url/api';
   ```

3. **Start development server**
   ```bash
   npm start
   # or
   ng serve
   ```

   Navigate to `http://localhost:4200/`

### Build for Production

```bash
npm run build
# or
ng build --configuration production
```

Output will be in `dist/hr-system/`

## API Integration

The frontend communicates with the backend via:

- **Login:** `POST /api/auth/login`
- **Requests:** `POST /api/requests/leave`, `GET /api/requests/history`
- **Approvals:** `POST /api/requests/{id}/approve`, `GET /api/requests/pending-approvals`
- **Reports:** `GET /api/reports/attendance`, `GET /api/reports/leave-summary`
- **Users:** `GET /api/users`, `POST /api/users`, `PUT /api/users/{id}`

## Core Services

### AuthService
Manages user authentication and state using Signals.

```typescript
// Usage
const success = await authService.login(username, password);
const user = authService.user();
const isAuthenticated = authService.isAuthenticated();
```

### ApiService
Generic HTTP wrapper for all API calls.

```typescript
// Usage
this.apiService.get<T>(endpoint, params)
this.apiService.post<T>(endpoint, body)
this.apiService.put<T>(endpoint, body)
this.apiService.delete<T>(endpoint)
```

### RequestService
Leave and overtime request management.

```typescript
// Usage
this.requestService.createLeaveRequest(request)
this.requestService.submitRequest(requestId)
this.requestService.getRequestHistory(userId)
this.requestService.approveRequest(requestId)
```

### ReportService
HR reporting and analytics.

```typescript
// Usage
this.reportService.getAttendanceReport(startDate, endDate, departmentId)
this.reportService.getLeaveSummaryReport(departmentId)
this.reportService.getOvertimeAuditReport(startDate, endDate, departmentId)
```

## Key Components

### LoginComponent
Authentication entry point with form validation.

### DashboardComponent
Employee home page with quick stats and action buttons.

### RequestFormComponent
Create/edit leave and overtime requests with validation.

### RequestListComponent
View personal leave request history with status tracking.

### ApprovalQueueComponent
Manager view for pending approvals (Manager/HR roles).

### ReportViewerComponent
HR analytics dashboard with filtering and export options.

### UserListComponent
Admin user management with search and filter.

### UserEditorComponent
Create/edit user profiles with role assignment.

## Signals (Reactive State)

The application uses Angular Signals for reactive state management:

```typescript
// Create a signal
const count = signal(0);

// Read value
console.log(count()); // 0

// Update value
count.set(1);
count.update(value => value + 1);

// Computed derived state
const doubled = computed(() => count() * 2);
```

## HTTP Interceptor

The `AuthInterceptor` automatically appends JWT tokens to all requests:

```typescript
// Automatically adds: Authorization: Bearer {token}
```

## Routing

Routes are lazy-loaded for better performance:

```
/auth/login                 - Login page
/dashboard                  - Employee dashboard
/requests/new              - Create leave request
/requests/list             - Request history
/approvals                 - Approval queue (Manager/HR)
/admin/users              - User management (Admin)
/reports                   - HR reports (HR/Admin)
```

## Authentication Flow

1. User enters credentials on `/auth/login`
2. `AuthService` sends login request to backend
3. Backend returns JWT token and user info
4. Token is stored in localStorage
5. `AuthInterceptor` attaches token to all requests
6. On logout, token and user data are cleared
7. Routes require authentication via guards

## Testing

Run unit tests:
```bash
npm test
```

Run tests with coverage:
```bash
npm test -- --code-coverage
```

## Styling

Global styles use SCSS with CSS Grid and Flexbox for responsive layouts.

Theming variables can be updated in `styles.scss`:
```scss
// Primary color: #667eea
// Secondary color: #764ba2
// Success: #198754
// Error: #dc3545
```

## Environment Configuration

Create `environment.ts` files for different environments:

```typescript
// environment.development.ts
export const environment = {
  apiUrl: 'https://localhost:5001/api',
  production: false
};

// environment.production.ts
export const environment = {
  apiUrl: 'https://api.example.com/api',
  production: true
};
```

## Contributing

Follow Angular style guide and best practices:
- Use standalone components
- Keep components focused on single responsibility
- Use services for shared logic
- Document public APIs with JSDoc comments

## Troubleshooting

### CORS Errors
Ensure backend CORS is configured to accept requests from frontend origin.

### 401 Unauthorized
Check if token is expired or missing. Re-login if necessary.

### Empty Reports
Verify filters and date ranges. Ensure backend has data for selected criteria.

## License

MIT
