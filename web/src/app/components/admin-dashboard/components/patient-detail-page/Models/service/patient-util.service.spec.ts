import { TestBed } from '@angular/core/testing';

import { PatientUtilService } from './patient-util.service';

describe('PatientUtilService', () => {
  let service: PatientUtilService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(PatientUtilService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
