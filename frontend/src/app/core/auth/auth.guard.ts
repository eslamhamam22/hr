import { inject } from '@angular/core';
import { Router, CanActivateFn, ActivatedRouteSnapshot, RouterStateSnapshot } from '@angular/router';
import { AuthService } from './auth.service';

export const authGuard: CanActivateFn = (route: ActivatedRouteSnapshot, state: RouterStateSnapshot) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  if (!authService.isAuthenticated()) {
    router.navigate(['/auth/login'], { queryParams: { returnUrl: state.url } });
    return false;
  }

  const requiredRoles = route.data['roles'] as string[];
  if (requiredRoles && requiredRoles.length > 0) {
    if (!authService.hasAnyRole(requiredRoles)) {
      // Redirect to a 'forbidden' page or dashboard if authorized but not for this route
      // For now, let's redirect to dashboard if they are logged in but don't have permission
      // Or maybe stay on current page? But we are navigating...
      // Let's redirect to dashboard
      router.navigate(['/dashboard']); 
      return false;
    }
  }

  return true;
};