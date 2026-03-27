import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
  name: 'min',
  standalone: true,
})
export class MinPipe implements PipeTransform {
  transform(start: number, end: number): number {
    return Math.min(start, end);
  }
}
