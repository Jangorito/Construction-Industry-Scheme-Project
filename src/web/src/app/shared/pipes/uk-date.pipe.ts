import { Pipe, PipeTransform } from '@angular/core';

// Ported from the mockup's fmtDate(): "30 Jul 2026" style. The explicit
// T00:00:00 avoids the date-only string being parsed as UTC midnight and
// displaying as the previous day in a UK-negative-offset-adjacent timezone.
@Pipe({ name: 'ukDate', standalone: true })
export class UkDatePipe implements PipeTransform {
  transform(dateIso: string): string {
    return new Date(`${dateIso}T00:00:00`).toLocaleDateString('en-GB', {
      day: '2-digit',
      month: 'short',
      year: 'numeric',
    });
  }
}
