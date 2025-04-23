import { ComponentFixture, TestBed } from '@angular/core/testing';

import { PatientDataReportComponent } from './patient-data-report.component';

describe('PatientDataReportComponent', () => {
  let component: PatientDataReportComponent;
  let fixture: ComponentFixture<PatientDataReportComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ PatientDataReportComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(PatientDataReportComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
