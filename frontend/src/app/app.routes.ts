import { Routes } from '@angular/router';
import { LoginComponent } from './features/auth/login/login.component';
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
    path: 'dashboard',
    component: DashboardComponent,
    canActivate: [authGuard]
  },
  {
    path: 'requests',
    canActivate: [authGuard],
    children: [
      {
        path: 'new',
        component: RequestFormComponent
      },
      {
        path: 'list',
        component: RequestListComponent
      }
    ]
  },
  {
    path: 'approvals',
    component: ApprovalQueueComponent,
    canActivate: [authGuard],
    data: { roles: ['Manager', 'HR', 'Admin'] }
  },
  {
    path: 'admin',
    canActivate: [authGuard],
    data: { roles: ['Admin'] },
    children: [
      {
        path: 'users',
        component: UserListComponent
      },
      {
        path: 'users/new',
        component: UserEditorComponent
      }
    ]
  },
  {
    path: 'reports',
    component: ReportViewerComponent,
    canActivate: [authGuard],
    data: { roles: ['Manager', 'HR', 'Admin'] }
  },
  {
    path: '',
    redirectTo: '/dashboard',
    pathMatch: 'full'
  }
];
