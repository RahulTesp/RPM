import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ProgramRenewComponent } from './program-renew.component';

describe('ProgramRenewComponent', () => {
  let component: ProgramRenewComponent;
  let fixture: ComponentFixture<ProgramRenewComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ProgramRenewComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ProgramRenewComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
