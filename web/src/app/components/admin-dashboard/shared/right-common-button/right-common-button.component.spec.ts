import { ComponentFixture, TestBed } from '@angular/core/testing';

import { RightCommonButtonComponent } from './right-common-button.component';

describe('RightCommonButtonComponent', () => {
  let component: RightCommonButtonComponent;
  let fixture: ComponentFixture<RightCommonButtonComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ RightCommonButtonComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(RightCommonButtonComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
