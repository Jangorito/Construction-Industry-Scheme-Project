import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

import { API_BASE_URL } from '../config/app-config';
import { ClassifyRequest, ClassifyResponse } from '../models/classify-api.model';

// Thin wrapper, no error handling here on purpose, that's the caller's job
// (InvoiceStore), since what counts as a recoverable failure is a state
// concern, not an HTTP concern.
@Injectable({ providedIn: 'root' })
export class ClassifyApiService {
  private readonly http = inject(HttpClient);

  classify(request: ClassifyRequest): Observable<ClassifyResponse> {
    return this.http.post<ClassifyResponse>(`${API_BASE_URL}/classify`, request);
  }
}
