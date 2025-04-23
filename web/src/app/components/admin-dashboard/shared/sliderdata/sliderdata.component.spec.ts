import { ComponentFixture, TestBed } from '@angular/core/testing';

import { SliderdataComponent } from './sliderdata.component';

describe('SliderdataComponent', () => {
  let component: SliderdataComponent;
  let fixture: ComponentFixture<SliderdataComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ SliderdataComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(SliderdataComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
