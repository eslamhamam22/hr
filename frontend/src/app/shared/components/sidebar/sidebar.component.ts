import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { AuthService } from '../../../core/auth/auth.service';

interface MenuItem {
    label: string;
    icon: string;
    route?: string;
    children?: MenuItem[];
    roles?: string[];
}

@Component({
    selector: 'app-sidebar',
    standalone: true,
    imports: [CommonModule, RouterModule],
    templateUrl: './sidebar.component.html',
    styleUrls: ['./sidebar.component.scss']
})
export class SidebarComponent {
    isCollapsed = false;
    expandedMenus: Set<string> = new Set();

    menuItems: MenuItem[] = [
        {
            label: 'Dashboard',
            icon: '📊',
            route: '/dashboard'
        },
        {
            label: 'Requests',
            icon: '📝',
            children: [
                {
                    label: 'All Employees Requests',
                    icon: '📊',
                    route: '/requests/all-requests',
                    roles: ['Manager', 'HR', 'Admin']
                },
                {
                    label: 'Leave Requests',
                    icon: '📋',
                    route: '/requests/leave'
                },
                {
                    label: 'Overtime Requests',
                    icon: '⏱️',
                    route: '/requests/overtime'
                },
                {
                    label: 'Work From Home',
                    icon: '🏠',
                    route: '/requests/work-from-home'
                }
            ]
        },
        {
            label: 'Approvals',
            icon: '✅',
            route: '/approvals',
            roles: ['Manager', 'HR', 'Admin']
        },
        {
            label: 'Management',
            icon: '⚙️',
            roles: ['HR', 'Admin'],
            children: [
                {
                    label: 'Users',
                    icon: '👥',
                    route: '/admin/users',
                    roles: ['Admin']
                },
                {
                    label: 'Departments',
                    icon: '🏢',
                    route: '/admin/departments',
                    roles: ['HR', 'Admin']
                },
                {
                    label: 'Attendance',
                    icon: '📅',
                    route: '/admin/attendance',
                    roles: ['HR', 'Admin']
                },
                {
                    label: 'Approval Logs',
                    icon: '📜',
                    route: '/admin/approval-logs',
                    roles: ['HR', 'Admin']
                }
            ]
        },
        {
            label: 'Reports',
            icon: '📈',
            route: '/reports',
            roles: ['Manager', 'HR', 'Admin']
        }
    ];

    constructor(
        public authService: AuthService,
        private router: Router
    ) { }

    toggleSidebar(): void {
        this.isCollapsed = !this.isCollapsed;
    }

    toggleMenu(label: string): void {
        if (this.expandedMenus.has(label)) {
            this.expandedMenus.delete(label);
        } else {
            this.expandedMenus.add(label);
        }
    }

    isMenuExpanded(label: string): boolean {
        return this.expandedMenus.has(label);
    }

    hasAccess(item: MenuItem): boolean {
        if (!item.roles || item.roles.length === 0) {
            return true;
        }
        return this.authService.hasAnyRole(item.roles);
    }

    navigateTo(route: string): void {
        this.router.navigate([route]);
    }

    logout(): void {
        this.authService.logout();
        this.router.navigate(['/auth/login']);
    }
}
