import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

import { API_BASE_URL } from '../config/app-config';
import { ExtractResponse } from '../models/extract-api.model';

@Injectable({ providedIn: 'root' })
export class ExtractApiService {
  private readonly http = inject(HttpClient);

  extract(file: File): Observable<ExtractResponse> {
    // Do not set Content-Type manually: the browser sets it automatically,
    // including the required boundary= parameter, when given a FormData
    // body. Setting it by hand is a common way to silently break multipart
    // requests.
    const formData = new FormData();
    formData.append('file', file, file.name);
    return this.http.post<ExtractResponse>(`${API_BASE_URL}/extract`, formData);
  }
}
