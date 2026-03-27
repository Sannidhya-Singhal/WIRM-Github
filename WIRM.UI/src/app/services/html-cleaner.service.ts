import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class HtmlCleanerService {
  stripHtmlTags(input: string): string {
    const parser = new DOMParser();
    const doc = parser.parseFromString(input, 'text/html');

    const unwanted = doc.querySelectorAll('script, style');
    unwanted.forEach(tag => tag.remove());

    return doc.body.textContent || '';
  }
}
