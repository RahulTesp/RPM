import { ComponentFixture, TestBed } from '@angular/core/testing';

import { DateRangeControlComponent } from './date-range-control.component';

describe('DateRangeControlComponent', () => {
  let component: DateRangeControlComponent;
  let fixture: ComponentFixture<DateRangeControlComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ DateRangeControlComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(DateRangeControlComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
