import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

import { API_BASE_URL } from '../config/app-config';
import { ApiLabel } from '../models/classify-api.model';
import { StoredInvoiceDto } from '../models/stored-invoice.model';

// Kept separate from ClassifyApiService, which is scoped specifically to
// the live-classify call, this one covers the read/mutate-persisted-data
// concerns instead.
@Injectable({ providedIn: 'root' })
export class InvoiceApiService {
  private readonly http = inject(HttpClient);

  getAll(): Observable<StoredInvoiceDto[]> {
    return this.http.get<StoredInvoiceDto[]>(`${API_BASE_URL}/invoices`);
  }

  overrideLine(invoiceId: string, lineId: string, newLabel: ApiLabel, reason: string): Observable<StoredInvoiceDto> {
    return this.http.patch<StoredInvoiceDto>(
      `${API_BASE_URL}/invoices/${invoiceId}/lines/${lineId}/override`,
      { newLabel, reason },
    );
  }
}
