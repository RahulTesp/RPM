import { ComponentFixture, TestBed } from '@angular/core/testing';

import { SidePanelPatientComponent } from './side-panel-patient.component';

describe('SidePanelPatientComponent', () => {
  let component: SidePanelPatientComponent;
  let fixture: ComponentFixture<SidePanelPatientComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ SidePanelPatientComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(SidePanelPatientComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
