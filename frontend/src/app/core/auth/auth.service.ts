import { Injectable } from '@angular/core';
import { signal, computed } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { firstValueFrom } from 'rxjs';
import { User, AuthState } from '../models/user.model';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private apiUrl = 'https://localhost:5001/api';
  
  // Signals for reactive state management
  private authStateSignal = signal<AuthState>({
    user: null,
    token: null,
    isAuthenticated: false,
    isLoading: false,
    error: null
  });

  // Public computed signals
  readonly user = computed(() => this.authStateSignal().user);
  readonly isAuthenticated = computed(() => this.authStateSignal().isAuthenticated);
  readonly isLoading = computed(() => this.authStateSignal().isLoading);
  readonly token = computed(() => this.authStateSignal().token);
  readonly error = computed(() => this.authStateSignal().error);

  constructor(private http: HttpClient) {
    this.loadStoredAuth();
  }

  async login(username: string, password: string): Promise<boolean> {
    this.authStateSignal.update(state => ({ ...state, isLoading: true, error: null }));
    
    try {
      const response = await firstValueFrom(
        this.http.post<any>(`${this.apiUrl}/auth/login`, { username, password })
      );

      if (response.success) {
        this.authStateSignal.update(state => ({
          ...state,
          user: response.user,
          token: response.token,
          isAuthenticated: true,
          isLoading: false
        }));
        
        this.storeAuth(response.user, response.token);
        return true;
      } else {
        this.authStateSignal.update(state => ({
          ...state,
          error: response.message || 'Login failed',
          isLoading: false
        }));
        return false;
      }
    } catch (error: any) {
      const errorMessage = error.error?.message || 'An error occurred during login';
      this.authStateSignal.update(state => ({
        ...state,
        error: errorMessage,
        isLoading: false
      }));
      return false;
    }
  }

  logout(): void {
    this.authStateSignal.set({
      user: null,
      token: null,
      isAuthenticated: false,
      isLoading: false,
      error: null
    });
    localStorage.removeItem('user');
    localStorage.removeItem('token');
  }

  private storeAuth(user: User, token: string): void {
    localStorage.setItem('user', JSON.stringify(user));
    localStorage.setItem('token', token);
  }

  private loadStoredAuth(): void {
    const storedUser = localStorage.getItem('user');
    const storedToken = localStorage.getItem('token');

    if (storedUser && storedToken) {
      try {
        const user = JSON.parse(storedUser);
        this.authStateSignal.update(state => ({
          ...state,
          user,
          token: storedToken,
          isAuthenticated: true
        }));
      } catch (error) {
        localStorage.removeItem('user');
        localStorage.removeItem('token');
      }
    }
  }

  getToken(): string | null {
    return this.token();
  }

  hasRole(role: string): boolean {
    const user = this.user();
    return user?.role === role;
  }

  hasAnyRole(roles: string[]): boolean {
    const user = this.user();
    return user ? roles.includes(user.role) : false;
  }
}
