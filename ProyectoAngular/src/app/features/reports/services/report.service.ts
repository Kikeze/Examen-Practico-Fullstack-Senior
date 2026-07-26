import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';

import { API_ENDPOINTS } from '../../../core/config/api-endpoints';
import { PagedResult } from '../../../shared/models/paged-result';
import { PendingTaskReport } from '../models/pending-task-report';
import { PendingTaskReportQuery } from '../models/pending-task-report-query';

@Injectable({
  providedIn: 'root'
})
export class ReportService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = API_ENDPOINTS.reports.pendingTasks;

  getPendingTasks(query: PendingTaskReportQuery): Observable<PagedResult<PendingTaskReport>> {
    const params = new HttpParams()
      .set('page', query.page)
      .set('pageSize', query.pageSize);

    return this.http.get<PagedResult<PendingTaskReport>>(this.apiUrl, { params });
  }
}
