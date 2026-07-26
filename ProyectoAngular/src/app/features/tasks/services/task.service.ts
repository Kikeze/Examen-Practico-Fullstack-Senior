import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';

import { API_ENDPOINTS } from '../../../core/config/api-endpoints';
import { PagedResult } from '../../../shared/models/paged-result';
import { CreateTaskRequest } from '../models/create-task-request';
import { TaskFilters } from '../models/task-filters';
import { TaskItem } from '../models/task-item';
import { UpdateTaskRequest } from '../models/update-task-request';

@Injectable({
  providedIn: 'root'
})
export class TaskService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = API_ENDPOINTS.tasks;

  getAll(filters: TaskFilters): Observable<PagedResult<TaskItem>> {
    const params = this.buildParams(filters);

    return this.http.get<PagedResult<TaskItem>>(this.apiUrl, { params });
  }

  getById(id: number): Observable<TaskItem> {
    return this.http.get<TaskItem>(`${this.apiUrl}/${id}`);
  }

  create(request: CreateTaskRequest): Observable<TaskItem> {
    return this.http.post<TaskItem>(this.apiUrl, request);
  }

  update(id: number, request: UpdateTaskRequest): Observable<void> {
    return this.http.put<void>(
      `${this.apiUrl}/${id}`,
      request
    );
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }

  private buildParams(filters: TaskFilters): HttpParams {
    let params = new HttpParams()
      .set('page', filters.page)
      .set('pageSize', filters.pageSize);

    if (filters.priority !== undefined) {
      params = params.set('priority', filters.priority);
    }

    if (filters.status !== undefined) {
      params = params.set('status', filters.status);
    }

    if (filters.userId !== undefined) {
      params = params.set('userId', filters.userId);
    }

    return params;
  }

}
