import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ConfirmDialogPanelComponent } from './confirm-dialog-panel.component';

describe('ConfirmDialogPanelComponent', () => {
  let component: ConfirmDialogPanelComponent;
  let fixture: ComponentFixture<ConfirmDialogPanelComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ ConfirmDialogPanelComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(ConfirmDialogPanelComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
