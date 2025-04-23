import { TestBed } from '@angular/core/testing';

import { PatientDataDetailsService } from './patient-data-details.service';

describe('PatientDataDetailsService', () => {
  let service: PatientDataDetailsService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(PatientDataDetailsService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
