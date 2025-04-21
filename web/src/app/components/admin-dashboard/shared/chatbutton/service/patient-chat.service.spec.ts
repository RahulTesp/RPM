import { TestBed } from '@angular/core/testing';

import { PatientChatService } from './patient-chat.service';

describe('PatientChatService', () => {
  let service: PatientChatService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(PatientChatService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
