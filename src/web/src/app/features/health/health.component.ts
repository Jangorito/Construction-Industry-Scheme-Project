import { HttpClient } from '@angular/common/http';
import { Component, OnInit, inject, signal } from '@angular/core';

import { API_BASE_URL } from '../../core/config/app-config';

interface HealthResponse {
  status: string;
}

@Component({
  selector: 'app-health',
  standalone: true,
  templateUrl: './health.component.html'
})
export class HealthComponent implements OnInit {
  private readonly http = inject(HttpClient);

  protected readonly status = signal<string>('checking...');
  protected readonly error = signal<string | null>(null);

  ngOnInit(): void {
    this.http.get<HealthResponse>(`${API_BASE_URL}/health`).subscribe({
      next: (response) => this.status.set(response.status),
      error: () => this.error.set('Could not reach the API. Is CisCompliance.Api running?')
    });
  }
}
