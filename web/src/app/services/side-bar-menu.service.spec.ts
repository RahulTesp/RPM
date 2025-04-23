import { TestBed } from '@angular/core/testing';

import { SideBarMenuService } from './side-bar-menu.service';

describe('SideBarMenuService', () => {
  let service: SideBarMenuService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(SideBarMenuService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
