import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';

import { API_ENDPOINTS } from '../../../core/config/api-endpoints';
import { ActiveUser } from '../models/active-user';

@Injectable({
  providedIn: 'root'
})
export class UserService {
  private readonly http = inject(HttpClient);

  getActive(): Observable<ActiveUser[]> {
    return this.http.get<ActiveUser[]>(API_ENDPOINTS.users.active);
  }
}
