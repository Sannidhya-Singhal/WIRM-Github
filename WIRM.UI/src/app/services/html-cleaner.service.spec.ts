import { TestBed } from '@angular/core/testing';

import { HtmlCleanerService } from './html-cleaner.service';

describe('HtmlCleanerService', () => {
  let service: HtmlCleanerService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(HtmlCleanerService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
