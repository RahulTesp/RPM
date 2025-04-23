import { TestBed } from '@angular/core/testing';

import { NotificationPanelService } from './notification-panel.service';

describe('NotificationPanelService', () => {
  let service: NotificationPanelService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(NotificationPanelService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
