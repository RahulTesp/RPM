import { TestBed } from '@angular/core/testing';

import { TwilioVideoServiceService } from './twilio-video-service.service';

describe('TwilioVideoServiceService', () => {
  let service: TwilioVideoServiceService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(TwilioVideoServiceService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
