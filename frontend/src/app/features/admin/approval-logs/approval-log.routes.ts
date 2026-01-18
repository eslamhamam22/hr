import { Routes } from '@angular/router';

export const approvalLogRoutes: Routes = [
    {
        path: '',
        loadComponent: () => import('./approval-logs-list/approval-logs-list.component').then(m => m.ApprovalLogsListComponent)
    },
    {
        path: ':id',
        loadComponent: () => import('./approval-log-detail/approval-log-detail.component').then(m => m.ApprovalLogDetailComponent)
    }
];

