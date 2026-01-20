/**
 * User roles in the system - matches backend RoleType enum
 */
export enum RoleType {
    Employee = 1,
    Manager = 2,
    HR = 3,
    Admin = 4
}

/**
 * Helper object for role display names
 */
export const RoleTypeLabels: { [key in RoleType]: string } = {
    [RoleType.Employee]: 'Employee',
    [RoleType.Manager]: 'Manager',
    [RoleType.HR]: 'HR',
    [RoleType.Admin]: 'Admin'
};

/**
 * Get display label for a role type
 */
export function getRoleLabel(role: RoleType | number): string {
    return RoleTypeLabels[role as RoleType] || 'Unknown';
}

/**
 * Get all role options for dropdowns
 */
export function getRoleOptions(): { value: RoleType; label: string }[] {
    return [
        { value: RoleType.Employee, label: 'Employee' },
        { value: RoleType.Manager, label: 'Manager' },
        { value: RoleType.HR, label: 'HR' },
        { value: RoleType.Admin, label: 'Admin' }
    ];
}
