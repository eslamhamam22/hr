# Angular 19 HR System Frontend - Setup Guide

## Quick Start

### 1. Install Dependencies
```bash
npm install
```

### 2. Configure Backend Connection
Edit `src/app/core/services/api.service.ts`:
```typescript
private apiUrl = 'https://localhost:5001/api';
```

### 3. Start Development Server
```bash
npm start
```

Navigate to `http://localhost:4200/`

### 4. Login
**Demo Credentials:**
- Username: `john.doe`
- Password: `password123`

## Project Structure

```
frontend/
├── src/
│   ├── app/
│   │   ├── core/              # Auth, services, models
│   │   ├── shared/            # Reusable components
│   │   ├── features/          # Feature modules
│   │   ├── app.routes.ts      # Route configuration
│   │   ├── app.config.ts      # App configuration
│   │   └── app.component.ts   # Root component
│   ├── main.ts                # Bootstrap
│   ├── index.html             # HTML template
│   └── styles.scss            # Global styles
├── package.json
├── angular.json
├── tsconfig.json
└── README.md
```

## Key Technologies

- **Angular 19** - Latest framework with Signals
- **TypeScript 5.6** - Strict type checking
- **RxJS 7.8** - Reactive programming
- **SCSS** - Advanced styling
- **Standalone Components** - No module dependencies

## Features

### Authentication
- JWT-based login/logout
- Token auto-persistence
- Role-based access control

### Employee Features
- Dashboard with quick stats
- Leave request submission
- Request history tracking
- Attendance monitoring

### Manager Features
- Approval queue
- Request review and decision

### HR Features
- User management
- Advanced reporting
- Department analytics
- Export capabilities (Excel, CSV, PDF)

### Admin Features
- System configuration
- User role management
- Manager assignment

## Development Commands

**Start development server:**
```bash
npm start
```

**Build for production:**
```bash
npm run build
```

**Run unit tests:**
```bash
npm test
```

**Lint code:**
```bash
npm run lint
```

**Watch mode for development:**
```bash
npm run watch
```

## Component Overview

### Auth Components
- `LoginComponent` - User authentication

### Dashboard Components
- `DashboardComponent` - Employee summary

### Request Components
- `RequestFormComponent` - Create/edit requests
- `RequestListComponent` - View request history

### Approval Components
- `ApprovalQueueComponent` - Manager approvals

### Admin Components
- `UserListComponent` - User management
- `UserEditorComponent` - User creation/editing

### Report Components
- `ReportViewerComponent` - Analytics dashboard
- `ReportFiltersComponent` - Filter controls

### Shared Components
- `DataTableComponent` - Generic table
- `ConfirmDialogComponent` - Modal dialog

## Services

### AuthService
Authentication and user state management using Signals.

```typescript
// Login
await authService.login(username, password);

// Get current user
const user = authService.user();

// Check authentication
const isAuth = authService.isAuthenticated();

// Logout
authService.logout();
```

### ApiService
Generic HTTP communication wrapper.

```typescript
// GET request
this.apiService.get<T>(endpoint, params);

// POST request
this.apiService.post<T>(endpoint, body);

// PUT request
this.apiService.put<T>(endpoint, body);

// DELETE request
this.apiService.delete<T>(endpoint);
```

### RequestService
Leave and overtime request management.

```typescript
// Create request
this.requestService.createLeaveRequest(request);

// Get history
this.requestService.getRequestHistory(userId);

// Approve
this.requestService.approveRequest(requestId);

// Reject
this.requestService.rejectRequest(requestId, reason);
```

### ReportService
HR analytics and reporting.

```typescript
// Get attendance report
this.reportService.getAttendanceReport(startDate, endDate);

// Get leave summary
this.reportService.getLeaveSummaryReport();

// Export data
this.reportService.exportToExcel(data, filename);
```

## Routing

All routes are lazy-loaded for better performance.

| Route | Component | Auth Required |
|-------|-----------|---|
| `/auth/login` | LoginComponent | No |
| `/dashboard` | DashboardComponent | Yes |
| `/requests/new` | RequestFormComponent | Yes |
| `/requests/list` | RequestListComponent | Yes |
| `/approvals` | ApprovalQueueComponent | Yes (Manager/HR) |
| `/admin/users` | UserListComponent | Yes (Admin) |
| `/reports` | ReportViewerComponent | Yes (HR/Admin) |

## Signals (Reactive State)

Components use Angular Signals for state management:

```typescript
// Create signal
count = signal(0);

// Read value
console.log(count()); // Get current value

// Update value
count.set(5);
count.update(v => v + 1);

// Computed derived state
doubled = computed(() => this.count() * 2);

// React to changes
effect(() => {
  console.log('Count changed:', this.count());
});
```

## Standalone Components

All components are standalone (no NgModule):

```typescript
@Component({
  selector: 'app-my-component',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `...`,
  styles: [`...`]
})
export class MyComponent {}
```

## Form Validation

Uses Reactive Forms with built-in validators:

```typescript
form = this.fb.group({
  email: ['', [Validators.required, Validators.email]],
  password: ['', [Validators.required, Validators.minLength(8)]],
  username: ['', [Validators.required, Validators.minLength(3)]]
});
```

## HTTP Interceptor

The `AuthInterceptor` automatically adds JWT token to all requests:

```
Authorization: Bearer {token}
```

## Environment Variables

Create `src/environments/environment.ts`:

```typescript
export const environment = {
  apiUrl: 'https://localhost:5001/api',
  production: false
};
```

Use in service:

```typescript
import { environment } from '../../environments/environment';

private apiUrl = environment.apiUrl;
```

## Responsive Design

The application is fully responsive:
- **Desktop:** Full layout with all features
- **Tablet:** Optimized grid system
- **Mobile:** Stacked layout and touch-friendly controls

## Browser Support

- Chrome/Chromium (latest)
- Firefox (latest)
- Safari (latest)
- Edge (latest)

## Troubleshooting

### Port Already in Use
```bash
ng serve --port 4201
```

### CORS Errors
Update backend CORS configuration to accept frontend origin.

### 401 Unauthorized
Token may be expired. Clear localStorage and login again.

### Module Not Found
Run `npm install` to ensure all dependencies are installed.

## Performance Tips

1. Use lazy loading for routes
2. Unsubscribe from observables in ngOnDestroy
3. Use OnPush change detection strategy
4. Implement trackBy in *ngFor loops
5. Optimize images and assets

## Security Best Practices

1. Store JWT in httpOnly cookies (backend handles this)
2. Use HTTPS in production
3. Implement CSRF protection
4. Validate all inputs
5. Sanitize user data with Angular's DomSanitizer
6. Keep dependencies up-to-date

## Production Deployment

### Build Optimized Bundle
```bash
npm run build
```

### Output Location
Build artifacts stored in `dist/hr-system/`

### Serve with Angular
```bash
npx http-server dist/hr-system/
```

### Deploy to Web Server
Copy contents of `dist/hr-system/` to web server root.

## Support & Resources

- [Angular Documentation](https://angular.io)
- [TypeScript Handbook](https://www.typescriptlang.org/docs/)
- [RxJS Documentation](https://rxjs.dev)

## License

MIT
