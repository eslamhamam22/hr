import { Routes } from '@angular/router';

export const overtimeRoutes: Routes = [
    {
        path: '',
        loadComponent: () => import('./overtime-list/overtime-list.component').then(m => m.OvertimeListComponent)
    },
    {
        path: 'new',
        loadComponent: () => import('./overtime-form/overtime-form.component').then(m => m.OvertimeFormComponent)
    },
    {
        path: ':id',
        loadComponent: () => import('./overtime-detail/overtime-detail.component').then(m => m.OvertimeDetailComponent)
    }
];

