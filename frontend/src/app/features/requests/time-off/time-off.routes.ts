import { Routes } from '@angular/router';

export const timeOffRoutes: Routes = [
    {
        path: '',
        loadComponent: () => import('./time-off-list/time-off-list.component').then(m => m.TimeOffListComponent)
    },

];
