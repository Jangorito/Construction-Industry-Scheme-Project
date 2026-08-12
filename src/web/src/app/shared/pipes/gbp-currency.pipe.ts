import { Pipe, PipeTransform } from '@angular/core';

// Ported from the mockup's fmt(): rounds to the nearest pound, no decimals,
// thousands separator. Not a calibrated currency formatter, matches the
// prototype's display convention exactly.
@Pipe({ name: 'gbp', standalone: true })
export class GbpCurrencyPipe implements PipeTransform {
  transform(value: number): string {
    return '£' + Math.round(value).toLocaleString('en-GB');
  }
}
