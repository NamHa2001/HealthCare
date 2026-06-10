import { Injectable, inject } from '@angular/core';
import { Observable, tap } from 'rxjs';
import { ApiService } from '../services/api.service';
import { AuthStore } from './auth.store';
import { AuthResponse, User } from './models/auth-response.model';
import { LoginRequest } from './models/login-request.model';
import { RegisterRequest } from './models/register-request.model';

const ACCESS_TOKEN_KEY = 'hp_access_token';
const REFRESH_TOKEN_KEY = 'hp_refresh_token';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly api = inject(ApiService);
  private readonly store = inject(AuthStore);

  login(request: LoginRequest): Observable<AuthResponse> {
    return this.api.post<AuthResponse>('Auth/login', request).pipe(
      tap((res) => this.handleAuth(res)),
    );
  }

  register(request: RegisterRequest): Observable<string> {
    return this.api.post<string>('Auth/register', request);
  }

  logout(): Observable<void> {
    const refreshToken = this.getRefreshToken();
    return this.api.post<void>('Auth/logout', { refreshToken }).pipe(
      tap(() => this.clearAuth()),
    );
  }

  refreshToken(): Observable<AuthResponse> {
    const refreshToken = this.getRefreshToken();
    return this.api.post<AuthResponse>('Auth/refresh-token', { refreshToken }).pipe(
      tap((res) => this.handleAuth(res)),
    );
  }

  loadCurrentUser(): Observable<User> {
    return this.api.get<User>('Auth/me').pipe(tap((user) => this.store.setUser(user)));
  }

  getAccessToken(): string | null {
    return localStorage.getItem(ACCESS_TOKEN_KEY);
  }

  getRefreshToken(): string | null {
    return localStorage.getItem(REFRESH_TOKEN_KEY);
  }

  /** Khôi phục phiên từ localStorage khi app khởi động lại. */
  restoreSession(): void {
    const accessToken = this.getAccessToken();
    const refreshToken = this.getRefreshToken();
    if (accessToken && refreshToken) {
      this.store.setTokens(accessToken, refreshToken);
    }
  }

  private handleAuth(res: AuthResponse): void {
    localStorage.setItem(ACCESS_TOKEN_KEY, res.accessToken);
    localStorage.setItem(REFRESH_TOKEN_KEY, res.refreshToken);
    this.store.setAuth(res.user, res.accessToken, res.refreshToken);
  }

  private clearAuth(): void {
    localStorage.removeItem(ACCESS_TOKEN_KEY);
    localStorage.removeItem(REFRESH_TOKEN_KEY);
    this.store.clear();
  }
}
