import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable, map } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResponse } from '../models/api-response.model';
import { PagedResult } from '../models/paged-result.model';

type QueryParams = Record<string, string | number | boolean | undefined | null>;

/**
 * Wrapper cho HttpClient. Tự động tháo envelope SRS §8.2:
 * - get/post/put trả về `data`.
 * - getPaged gộp `data` (items) + `meta` thành PagedResult.
 * - delete trả về void (backend trả 204 No Content).
 */
@Injectable({ providedIn: 'root' })
export class ApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = environment.apiUrl;

  get<T>(path: string, params?: QueryParams): Observable<T> {
    return this.http
      .get<ApiResponse<T>>(`${this.baseUrl}/${path}`, { params: this.buildParams(params) })
      .pipe(map((res) => res.data));
  }

  getPaged<T>(path: string, params?: QueryParams): Observable<PagedResult<T>> {
    return this.http
      .get<ApiResponse<T[]>>(`${this.baseUrl}/${path}`, { params: this.buildParams(params) })
      .pipe(
        map((res) => {
          const items = res.data ?? [];
          const meta = res.meta ?? { page: 1, pageSize: items.length, total: items.length };
          const totalPages = meta.pageSize > 0 ? Math.ceil(meta.total / meta.pageSize) : 0;
          return { items, page: meta.page, pageSize: meta.pageSize, total: meta.total, totalPages };
        }),
      );
  }

  post<T>(path: string, body: unknown): Observable<T> {
    return this.http
      .post<ApiResponse<T>>(`${this.baseUrl}/${path}`, body)
      .pipe(map((res) => res.data));
  }

  put<T>(path: string, body: unknown): Observable<T> {
    return this.http
      .put<ApiResponse<T>>(`${this.baseUrl}/${path}`, body)
      .pipe(map((res) => res.data));
  }

  delete(path: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${path}`).pipe(map(() => undefined));
  }

  private buildParams(params?: QueryParams): HttpParams {
    let httpParams = new HttpParams();
    if (params) {
      for (const [key, value] of Object.entries(params)) {
        if (value !== undefined && value !== null) {
          httpParams = httpParams.set(key, String(value));
        }
      }
    }
    return httpParams;
  }
}
