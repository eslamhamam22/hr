import { Routes } from '@angular/router';
import { LoginComponent } from './features/auth/login/login.component';
import { LayoutComponent } from './shared/layout/layout.component';
import { DashboardComponent } from './features/dashboard/employee-summary.component';
import { RequestFormComponent } from './features/requests/request-form/request-form.component';
import { RequestListComponent } from './features/requests/request-list/request-list.component';
import { ApprovalQueueComponent } from './features/approvals/approval-queue.component';
import { UserListComponent } from './features/admin/user-list/user-list.component';
import { UserEditorComponent } from './features/admin/user-editor/user-editor.component';
import { ReportViewerComponent } from './features/reports/report-viewer/report-viewer.component';
import { authGuard } from './core/auth/auth.guard';

export const routes: Routes = [
  {
    path: 'auth/login',
    component: LoginComponent
  },
  {
    path: '',
    component: LayoutComponent,
    canActivate: [authGuard],
    children: [
      {
        path: 'dashboard',
        component: DashboardComponent
      },
      {
        path: 'requests',
        children: [
          {
            path: 'new',
            component: RequestFormComponent
          },
          {
            path: 'list',
            component: RequestListComponent
          },
          {
            path: 'overtime',
            loadChildren: () => import('./features/requests/overtime/overtime.routes').then(m => m.overtimeRoutes)
          }
        ]
      },
      {
        path: 'approvals',
        component: ApprovalQueueComponent,
        data: { roles: ['Manager', 'HR', 'Admin'] }
      },
      {
        path: 'admin',
        data: { roles: ['Admin', 'HR'] },
        children: [
          {
            path: 'users',
            component: UserListComponent,
            data: { roles: ['Admin'] }
          },
          {
            path: 'users/new',
            component: UserEditorComponent,
            data: { roles: ['Admin'] }
          },
          {
            path: 'departments',
            loadChildren: () => import('./features/admin/departments/department.routes').then(m => m.departmentRoutes)
          },
          {
            path: 'attendance',
            loadChildren: () => import('./features/admin/attendance/attendance.routes').then(m => m.attendanceRoutes)
          },
          {
            path: 'approval-logs',
            loadChildren: () => import('./features/admin/approval-logs/approval-log.routes').then(m => m.approvalLogRoutes)
          }
        ]
      },
      {
        path: 'reports',
        component: ReportViewerComponent,
        data: { roles: ['Manager', 'HR', 'Admin'] }
      },
      {
        path: '',
        redirectTo: '/dashboard',
        pathMatch: 'full'
      }
    ]
  }
];

